using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Linq;

namespace Celeste.Mod.CavernHelper {
    [Tracked]
    [CustomEntity("cavern/crystalbomb")]
    public class CrystalBomb : Actor {
        public Holdable Hold;
        public Vector2 Speed;

        private readonly Sprite sprite;
        private readonly Collision onCollideH;
        private readonly Collision onCollideV;
        private readonly Circle pushRadius;
        private readonly Hitbox hitBox;
        private readonly float maxRespawnTime;
        private readonly float maxExplodeTimer;
        private readonly bool explodeOnSpawn = false;
        private readonly bool respawnOnExplode = true;
        private readonly bool breakDashBlocks = false;
        private readonly bool legacyMode = true;

        private BirdTutorialGui tutorialGui;
        private Vector2 prevLiftSpeed;
        private Vector2 startPos;
        public string startLevel;
        private float noGravityTimer;
        private float hardVerticalHitSoundCooldown;
        private float respawnTime = 0f;
        private float frameCount;
        private float explodeTimer = 0f;
        private bool exploded;
        private bool activated = false;
        private bool shouldShowTutorial = true;
        private bool playedFuseSound = false;

        public CrystalBomb(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            Depth = 100;
            maxRespawnTime = data.Float("respawnTime", 2f);
            maxExplodeTimer = data.Float("explodeTime", 1f);
            explodeOnSpawn = data.Bool("explodeOnSpawn", false);
            respawnOnExplode = data.Bool("respawnOnExplode", true);
            breakDashBlocks = data.Bool("breakDashBlocks", false);

            // Legacy behavior did not call Holdable.CheckAgainstColliders()
            // Defaults to true for old placements, false for new ones
            legacyMode = data.Bool("legacyMode", !data.Has("legacyMode"));

            pushRadius = new Circle(40f, 0f, 0f);
            hitBox = new Hitbox(8f, 10f, -4f, -10f);
            Collider = hitBox;
            onCollideH = new Collision(OnCollideH);
            onCollideV = new Collision(OnCollideV);

            Add(sprite = CavernModule.SpriteBank.Create("crystalbomb"));

            Add(Hold = new Holdable());
            Hold.PickupCollider = new Hitbox(16f, 16f, -8f, -16f);
            Hold.OnPickup = new Action(OnPickup);
            Hold.OnRelease = new Action<Vector2>(OnRelease);
            Hold.OnHitSpring = new Func<Spring, bool>(HitSpring);

            Hold.SpeedGetter = () => Speed;

            Add(new VertexLight(Collider.Center, Color.White, 1f, 32, 64));
            Add(new MirrorReflection());
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            startPos = Position;
            startLevel = SceneAs<Level>().Session.Level;
            if (explodeOnSpawn) {
                activated = true;
            }

            if (SceneAs<Level>().Session.Level == "a-05tut") {
                tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry", null), new object[]
                {
                    Dialog.Clean("tutorial_hold", null),
                    Input.Grab
                }) {
                    Open = false
                };
                shouldShowTutorial = true;
                Scene.Add(tutorialGui);
            }
        }

        public override void Update() {
            base.Update();
            frameCount += 1f;

            Level level = SceneAs<Level>();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (!exploded) {
                foreach (CrystalBombField field in Scene.Tracker.GetEntities<CrystalBombField>()) {
                    field.Collidable = true;
                    if (CollideCheck(field)) {
                        field.Collidable = false;
                        Explode();                        
                        return;
                    }
                    field.Collidable = false;
                }

                foreach (CrystalBombExploderCollider collider in Scene.Tracker.GetComponents<CrystalBombExploderCollider>()) {
                    if (collider.Check(this)) {
                        Explode();
                        return;
                    }
                }

                if (!Hold.IsHeld) {
                    if (legacyMode) {
                        foreach (Spring spring in Scene.Entities.OfType<Spring>()) {
                            if (CollideCheck(spring) && HitSpring(spring)) {
                                DynamicData.For(spring).Invoke("BounceAnimate");
                            }
                        }
                    }

                    if (tutorialGui != null) {
                        tutorialGui.Open = shouldShowTutorial && player != null && Vector2.Distance(player.Position, Position) < 64;
                    }
                }

                if (activated) {
                    if (!playedFuseSound) {
                        Audio.Play(SFX.game_04_arrowblock_debris, Position);
                        playedFuseSound = true;
                    }

                    explodeTimer += Engine.DeltaTime;
                    sprite.Play("crbomb", false, false);
                    if (maxExplodeTimer > 0) {
                        sprite.SetAnimationFrame((int)Math.Floor((double)(explodeTimer / maxExplodeTimer * 60f)));
                    }
                    
                    if (explodeTimer >= maxExplodeTimer) {
                        Explode();
                    }
                }

                hardVerticalHitSoundCooldown -= Engine.DeltaTime;
                Depth = 100;
                if (Hold.IsHeld) {
                    prevLiftSpeed = Vector2.Zero;
                } else {
                    if (OnGround(downCheck: 1)) {
                        float target = !OnGround(Position + (Vector2.UnitX * 3f), 1) ? 20f : !OnGround(Position - (Vector2.UnitX * 3f), 1) ? -20f : 0f;

                        Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime * (CollideCheck<IcyFloor>() ? 0.01f : 1));
                        Vector2 liftSpeed = LiftSpeed;
                        if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
                            Speed = prevLiftSpeed;
                            prevLiftSpeed = Vector2.Zero;
                            Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                            if (Speed.X != 0f && Speed.Y == 0f) {
                                Speed.Y = -60f;
                            }

                            if (Speed.Y < 0f) {
                                noGravityTimer = 0.15f;
                            }
                        } else {
                            prevLiftSpeed = liftSpeed;
                            if (liftSpeed.Y < 0f && Speed.Y < 0f) {
                                Speed.Y = 0f;
                            }
                        }
                    } else {
                        bool shouldHaveGravity = Hold.ShouldHaveGravity;
                        if (shouldHaveGravity) {
                            float num = 800f;
                            if (Math.Abs(Speed.Y) <= 30f) {
                                num *= 0.5f;
                            }

                            float num2 = 350f;
                            if (Speed.Y < 0f) {
                                num2 *= 0.5f;
                            }

                            Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                            if (noGravityTimer > 0f) {
                                noGravityTimer -= Engine.DeltaTime;
                            } else {
                                Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                            }
                        }
                    }

                    MoveH(Speed.X * Engine.DeltaTime, onCollideH, null);
                    MoveV(Speed.Y * Engine.DeltaTime, onCollideV, null);
                    if (Left < level.Bounds.Left) {
                        Left = level.Bounds.Left;
                        OnCollideH(new CollisionData {
                            Direction = -Vector2.UnitX
                        });
                    } else if (Right > level.Bounds.Right) {
                        Right = level.Bounds.Right;
                        OnCollideH(new CollisionData {
                            Direction = Vector2.UnitX
                        });
                    }

                    if (Top < level.Bounds.Top) {
                        Top = level.Bounds.Top;
                        OnCollideV(new CollisionData {
                            Direction = -Vector2.UnitY
                        });
                    } else if (Bottom > level.Bounds.Bottom + 4) {
                        if (!exploded) {
                            Explode();
                            return;
                        }
                    }
                }

                if (!legacyMode) {
                    Hold.CheckAgainstColliders();
                }
            } else {
                if (respawnTime < maxRespawnTime) {
                    respawnTime += Engine.DeltaTime;
                } else {
                    respawnTime = 0f;
                    exploded = false;
                    Visible = true;
                    Collidable = true;
                    playedFuseSound = false;
                    sprite.Play("idle", true, false);
                    Position = startPos;
                    if (explodeOnSpawn) {
                        activated = true;
                    }
                }
            }
        }

        protected override void OnSquish(CollisionData data) {
            if (!TrySquishWiggle(data)) {
                Explode();
            }
        }

        private bool HitSpring(Spring spring) {
            if (!Hold.IsHeld) {
                if (spring.Orientation == Spring.Orientations.Floor && (legacyMode || Speed.Y >= 0f)) {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                } else if (spring.Orientation == Spring.Orientations.WallLeft && (legacyMode || Speed.X <= 0f)) {
                    Speed.X = 240f;
                    Speed.Y = -140f;
                    noGravityTimer = 0.15f;
                    return true;
                } else if (spring.Orientation == Spring.Orientations.WallRight && (legacyMode || Speed.X >= 0f)) {
                    Speed.X = -240f;
                    Speed.Y = -140f;
                    noGravityTimer = 0.15f;
                    return true;
                }
            }

            return false;
        }

        private void OnPickup() {
            if (tutorialGui != null) {
                shouldShowTutorial = false;
            }

            if (!legacyMode) {
                AddTag(Tags.Persistent);
            }

            Speed = Vector2.Zero;
            activated = true;
        }

        private void OnRelease(Vector2 force) {
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }

            RemoveTag(Tags.Persistent);

            Speed = force * 200f;
            if (Speed != Vector2.Zero) {
                noGravityTimer = 0.1f;
            }
        }

        private void OnCollideH(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }

            Audio.Play(SFX.game_05_crystaltheo_impact_side, Position);
            if (Math.Abs(Speed.X) > 100f) {
                ImpactParticles(data.Direction);
            }

            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data) {
            if (data.Hit is DashSwitch) {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }

            if (Speed.Y > 0f && frameCount > 15f) {
                if (hardVerticalHitSoundCooldown <= 0f) {
                    Audio.Play(SFX.game_05_crystaltheo_impact_ground, Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f, 0f, 1f));
                    hardVerticalHitSoundCooldown = 0.5f;
                } else {
                    Audio.Play(SFX.game_05_crystaltheo_impact_ground, Position, "crystal_velocity", 0f);
                }
            }

            if (Speed.Y > 160f) {
                ImpactParticles(data.Direction);
            }

            if (CollideCheck<IcyFloor>()) {
                Speed.Y = 0;
            } else if (Speed.Y > 140f && data.Hit is not SwapBlock && data.Hit is not DashSwitch) {
                Speed.Y *= -0.6f;
            } else {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir) {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f) {
                direction = 3.14159274f;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.X < 0f) {
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            } else if (dir.Y > 0f) {
                direction = -1.57079637f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            } else {
                direction = 1.57079637f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }

            SceneAs<Level>().Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        internal void Explode() {
            if (!exploded) {
                exploded = true;
                activated = false;
                explodeTimer = 0f;
                
                sprite.Play("idle", true, false);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f, null, null);
                RecoverBlast.Spawn(Position);
                for (int i = 0; i < 10; i++) {
                    Audio.Play(SFX.game_06_fall_spike_smash, Position);
                }

                Collider = pushRadius;
                if (!CollideCheck<CassetteBlock>()) {
                    CrystalDebris.Burst(Position + (Vector2.UnitY * -10f), Calc.HexToColor("639bff"), false, 32);
                }

                Player player = CollideFirst<Player>();
                if (player != null && !Scene.CollideCheck<Solid>(Position + (Vector2.UnitY * -10f), player.Center)) {
                    player.ExplodeLaunch(Position, true);
                }

                foreach (TempleCrackedBlock crackedBlock in CollideAll<TempleCrackedBlock>()) {
                    crackedBlock.Break(Position);
                }

                if (breakDashBlocks) {
                    foreach (DashBlock dashBlock in CollideAll<DashBlock>()) {
                        dashBlock.Break(Position, Position - dashBlock.Position, true, true);
                    }
                }

                foreach (CrystalBombExplosionCollider ec in Scene.Tracker.GetComponents<CrystalBombExplosionCollider>()) {
                    ec.Check(this);
                }

                if (!legacyMode) {
                    foreach (TouchSwitch touchSwitch in CollideAll<TouchSwitch>()) {
                        touchSwitch.TurnOn();
                    }
                }

                if (Hold.IsHeld) {
                    Hold.Holder.Drop();
                }

                if (!respawnOnExplode || SceneAs<Level>().Session.Level != startLevel) {
                    RemoveSelf();
                }

                Collider = hitBox;
                Visible = false;
                Collidable = false;
                Position = startPos;
                Speed = Vector2.Zero;
            }
        }

        [Pooled]
        public class RecoverBlast : Entity {
            private Sprite sprite;
            public static void Spawn(Vector2 position) {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }

            public override void Added(Scene scene) {
                base.Added(scene);
                Depth = -199;
                if (sprite == null) {
                    Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    sprite.OnLastFrame = delegate (string a) {
                        RemoveSelf();
                    };
                }

                sprite.Play("shockwave", true, false);
                sprite.Rate = 5;
            }
        }
    }
}
