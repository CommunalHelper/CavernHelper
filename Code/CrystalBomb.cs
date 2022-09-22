using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CavernHelper
{
    public class CrystalBomb : Actor
    {
        public Holdable Hold;
        private Sprite sprite;

        private Collision onCollideH;
        private Collision onCollideV;
        public Vector2 Speed;
        private float noGravityTimer;
        private Vector2 prevLiftSpeed;
        private float hardVerticalHitSoundCooldown;

        private Level Level;

        private bool exploded;

        private Vector2 previousPosition;

        private List<Spring> springs;
        private List<CassetteBlock> cassetteBlocks;
        private List<IcyFloor> icyFloors;
        private Player playerEntity = null;

        private Circle pushRadius;
        private Hitbox hitBox;
        private Vector2 startPos;

        private float maxRespawnTime;
        private float respawnTime = 0f;

        private float frameCount;

        private float maxExplodeTimer;
        private float explodeTimer = 0f;
        private bool exploding = false;
        private bool explodeOnSpawn = false;
        private bool respawnOnExplode = true;
        private bool breakDashBlocks = false;

        private BirdTutorialGui tutorialGui;
        private bool shouldShowTutorial = true;

        private bool playedFuseSound = false;

        private enum RespawnTimes
        {
            Short = 5,
            Medium = 10,
            Long = 15
        }

        private enum ExplodeTimes
        {
            Short = 1,
            Medium,
            Long = 5
        }

        private Level level
        {
            get
            {
                return (Level)base.Scene;
            }
        }

        public CrystalBomb(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            base.Depth = 100;
            this.maxRespawnTime = data.Float("respawnTime", 5f);
            this.maxExplodeTimer = data.Float("explodeTime", 1f);
            this.explodeOnSpawn = data.Bool("explodeOnSpawn", false);
            this.respawnOnExplode = data.Bool("respawnOnExplode", true);
            this.breakDashBlocks = data.Bool("breakDashBlocks", false);
            this.pushRadius = new Circle(40f, 0f, 0f);
            this.hitBox = new Hitbox(8f, 10f, -4f, -10f);
            base.Collider = this.hitBox;
            base.Add(this.sprite = CavernModule.SpriteBank.Create("crystalbomb"));
            base.Add(this.Hold = new Holdable());
            this.Hold.PickupCollider = new Hitbox(16f, 16f, -8f, -16f);
            this.Hold.OnPickup = new Action(this.OnPickup);
            this.Hold.OnRelease = new Action<Vector2>(this.OnRelease);
            this.onCollideH = new Collision(this.OnCollideH);
            this.onCollideV = new Collision(this.OnCollideV);
            this.Hold.SpeedGetter = () => this.Speed;
            base.Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            base.Add(new MirrorReflection());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            this.Level = base.SceneAs<Level>();
            if (level == null)
                return;
        }

        void OnCollideSpring(Spring spring)
        {
            Spring.Orientations orientation;
            Sprite springSprite = spring.Get<Sprite>();
            if (springSprite.Rotation == 1.57079637f)
            {
                orientation = Spring.Orientations.WallLeft;
            }
            else if (springSprite.Rotation == -1.57079637f)
            {
                orientation = Spring.Orientations.WallRight;
            }
            else
            {
                orientation = Spring.Orientations.Floor;
            }
            Audio.Play("event:/game/general/spring", spring.BottomCenter);
            spring.Get<StaticMover>().TriggerPlatform();
            spring.Get<Wiggler>().Start();
            spring.Get<Sprite>().Play("bounce", true, false);
            HitSpring(orientation);
        }
        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            GetPlayer();
            this.springs = scene.Entities.OfType<Spring>().ToList<Spring>();
            this.cassetteBlocks = scene.Entities.OfType<CassetteBlock>().ToList<CassetteBlock>();
            this.icyFloors = scene.Entities.OfType<IcyFloor>().ToList<IcyFloor>();
            this.startPos = this.Position;
            if (this.explodeOnSpawn)
            {
                this.exploding = true;
            }
            if (this.Level.Session.Level == "a-05tut")
            {
                this.tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry", null), new object[]
                {
                    Dialog.Clean("tutorial_hold", null),
                    Input.Grab
                });
                this.tutorialGui.Open = false;
                this.shouldShowTutorial = true;
                base.Scene.Add(this.tutorialGui);
            }
        }

        private void OnPickup()
        {
            if (this.tutorialGui != null)
            {
                this.shouldShowTutorial = false;
            }
            this.Speed = Vector2.Zero;
            this.exploding = true;
        }

        bool CheckForIcyFloor()
        {
            bool collidesWithIcyFloor = false;
            for (int i = 0; i < icyFloors.Count; i++)
            {
                if (this.CollideCheck(icyFloors[i]))
                {
                    collidesWithIcyFloor = true;
                    break;
                }
            }
            return collidesWithIcyFloor;
        }

        public void HitSpring(Spring.Orientations springOrientation)
        {
            if (!this.Hold.IsHeld)
            {
                if (springOrientation == Spring.Orientations.Floor)
                {
                    this.Speed.X = this.Speed.X * 0.5f;
                    this.Speed.Y = -160f;
                    this.noGravityTimer = 0.15f;
                }
                else
                {
                    this.Speed.X = 240 * (springOrientation == Spring.Orientations.WallLeft ? 1 : -1);
                    this.Speed.Y = -140f;
                    this.noGravityTimer = 0.15f;
                }
            }
        }

        private void OnRelease(Vector2 force)
        {
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            this.Speed = force * 200f;
            if (this.Speed != Vector2.Zero)
            {
                this.noGravityTimer = 0.1f;
            }
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * (float)Math.Sign(this.Speed.X));
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", this.Position);
            if (Math.Abs(this.Speed.X) > 100f)
            {
                this.ImpactParticles(data.Direction);
            }
            this.Speed.X = this.Speed.X * -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * (float)Math.Sign(this.Speed.Y));
            }
            if (this.Speed.Y > 0f && this.frameCount > 15f)
            {
                if (this.hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", Calc.ClampedMap(this.Speed.Y, 0f, 200f, 0f, 1f));
                    this.hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", this.Position, "crystal_velocity", 0f);
                }
            }
            
            if (this.Speed.Y > 160f)
            {
                this.ImpactParticles(data.Direction);
            }
            if (CheckForIcyFloor())
            {
                this.Speed.Y = 0;
            }
            else if (this.Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                this.Speed.Y = this.Speed.Y * -0.6f;
            }
            else
            {
                this.Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = 3.14159274f;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else
            {
                if (dir.X < 0f)
                {
                    direction = 0f;
                    position = new Vector2(base.Left, base.Y - 4f);
                    positionRange = Vector2.UnitY * 6f;
                }
                else
                {
                    if (dir.Y > 0f)
                    {
                        direction = -1.57079637f;
                        position = new Vector2(base.X, base.Bottom);
                        positionRange = Vector2.UnitX * 6f;
                    }
                    else
                    {
                        direction = 1.57079637f;
                        position = new Vector2(base.X, base.Top);
                        positionRange = Vector2.UnitX * 6f;
                    }
                }
            }
            this.Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        private void Explode()
        {
            if (!this.exploded)
            {
                this.Collider = new Circle(10f, 0f, 0f);
                this.exploding = false;
                this.explodeTimer = 0f;
                bool collidesWithCassetteBlock = false;
                for (int i = 0; i < cassetteBlocks.Count; i++)
                {
                    if (this.CollideCheck(cassetteBlocks[i]))
                    {
                        collidesWithCassetteBlock = true;
                        break;
                    }
                }
                base.Collider = this.pushRadius;
                this.sprite.Play("idle", true, false);
                this.level.Displacement.AddBurst(this.Position, 0.35f, 8f, 48f, 0.25f, null, null);
                CrystalBomb.RecoverBlast.Spawn(this.Position);
                for (int i = 0; i < 10; i++)
                {
                    Audio.Play("event:/game/06_reflection/fall_spike_smash", this.Position);
                }
                
                if (!collidesWithCassetteBlock)
                {
                    CrystalDebris.Burst(this.Position + Vector2.UnitY * -10f, Calc.HexToColor("639bff"), false, 32);
                }
                Player player = base.CollideFirst<Player>();
                if (player != null && !base.Scene.CollideCheck<Solid>(this.Position + Vector2.UnitY * -10f, player.Center))
                {
                    player.ExplodeLaunch(this.Position, true);
                }
                foreach (Entity entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
                {
                    TempleCrackedBlock wall = (TempleCrackedBlock)entity;
                    if (base.CollideCheck(wall))
                    {
                        wall.Break(this.Position);
                    }
                }
                if (this.breakDashBlocks)
                {
                    foreach (Entity entity in base.Scene.Tracker.GetEntities<DashBlock>())
                    {
                        DashBlock block = (DashBlock)entity;
                        if (base.CollideCheck(block))
                        {
                            block.Break(this.Position, this.Position - block.Position, true, true);
                        }
                    }
                }
                if (!this.respawnOnExplode)
                {
                    base.RemoveSelf();
                }
                this.exploded = true;
                base.Collider = this.hitBox;
                this.Visible = false;
                this.Collidable = false;
                this.Speed = Vector2.Zero;
                if (player != null)
                {
                    if (player.Holding != null)
                    {
                        if (player.Holding == this.Hold)
                        {
                            this.Hold.Release(Vector2.Zero);
                            player.Holding = null;
                            player.Get<Sprite>().Update();
                        }
                    }
                }
                this.Position = this.startPos;
            }
        }

        protected override void OnSquish(CollisionData data)
        {
            if (!base.TrySquishWiggle(data))
            {
                this.Explode();
            }
        }

        void GetPlayer()
        {
            List<Player> entities = level.Entities.OfType<Player>().ToList();
            if (entities.Count > 0)
            {
                playerEntity = entities[0];
            }
        }

        public override void Update()
        {
            base.Update();
            this.frameCount += 1f;
            if (playerEntity == null)
            {
                GetPlayer();
            }
            if (!this.exploded)
            {
                if (!this.Hold.IsHeld)
                {
                    foreach (Spring spring in springs)
                    {
                        if (this.CollideCheck(spring))
                        {
                            this.OnCollideSpring(spring);
                        }
                    }
                    if (this.tutorialGui != null)
                    {
                        if (this.shouldShowTutorial)
                        {
                            this.tutorialGui.Open = ((playerEntity.Position - this.Position).Length() < 64);
                        }
                        else
                        {
                            this.tutorialGui.Open = false;
                        }
                    }
                }
                if (this.exploding)
                {
                    if (!playedFuseSound)
                    {
                        Audio.Play("event:/game/04_cliffside/arrowblock_debris", this.Position);
                        playedFuseSound = true;
                    }
                    this.explodeTimer += Engine.DeltaTime;
                    this.sprite.Play("crbomb", false, false);
                    this.sprite.SetAnimationFrame((int)Math.Floor((double)(this.explodeTimer / this.maxExplodeTimer * 60f)));
                    if (this.explodeTimer >= this.maxExplodeTimer)
                    {
                        this.Explode();
                    }
                }
                this.hardVerticalHitSoundCooldown -= Engine.DeltaTime;
                base.Depth = 100;
                if (this.Hold.IsHeld)
                {
                    this.prevLiftSpeed = Vector2.Zero;
                }
                else
                {
                    if (base.OnGround(1))
                    {
                        float target;
                        if (!base.OnGround(this.Position + Vector2.UnitX * 3f, 1))
                        {
                            target = 20f;
                        }
                        else
                        {
                            if (!base.OnGround(this.Position - Vector2.UnitX * 3f, 1))
                            {
                                target = -20f;
                            }
                            else
                            {
                                target = 0f;
                            }
                        }
                        this.Speed.X = Calc.Approach(this.Speed.X, target, 800f * Engine.DeltaTime * (CheckForIcyFloor() ? 0.01f : 1));
                        Vector2 liftSpeed = base.LiftSpeed;
                        if (liftSpeed == Vector2.Zero && this.prevLiftSpeed != Vector2.Zero)
                        {
                            this.Speed = this.prevLiftSpeed;
                            this.prevLiftSpeed = Vector2.Zero;
                            this.Speed.Y = Math.Min(this.Speed.Y * 0.6f, 0f);
                            if (this.Speed.X != 0f && this.Speed.Y == 0f)
                            {
                                this.Speed.Y = -60f;
                            }
                            if (this.Speed.Y < 0f)
                            {
                                this.noGravityTimer = 0.15f;
                            }
                        }
                        else
                        {
                            this.prevLiftSpeed = liftSpeed;
                            if (liftSpeed.Y < 0f && this.Speed.Y < 0f)
                            {
                                this.Speed.Y = 0f;
                            }
                        }
                    }
                    else
                    {
                        bool shouldHaveGravity = this.Hold.ShouldHaveGravity;
                        if (shouldHaveGravity)
                        {
                            float num = 800f;
                            if (Math.Abs(this.Speed.Y) <= 30f)
                            {
                                num *= 0.5f;
                            }
                            float num2 = 350f;
                            if (this.Speed.Y < 0f)
                            {
                                num2 *= 0.5f;
                            }
                            this.Speed.X = Calc.Approach(this.Speed.X, 0f, num2 * Engine.DeltaTime);
                            if (this.noGravityTimer > 0f)
                            {
                                this.noGravityTimer -= Engine.DeltaTime;
                            }
                            else
                            {
                                this.Speed.Y = Calc.Approach(this.Speed.Y, 200f, num * Engine.DeltaTime);
                            }
                        }
                    }
                    this.previousPosition = base.ExactPosition;
                    base.MoveH(this.Speed.X * Engine.DeltaTime, this.onCollideH, null);
                    base.MoveV(this.Speed.Y * Engine.DeltaTime, this.onCollideV, null);
                    if (base.Center.X > (float)this.Level.Bounds.Right)
                    {
                        base.MoveH(32f * Engine.DeltaTime, null, null);
                        if (base.Right > (float)this.Level.Bounds.Right)
                        {
                            base.Right = (float)this.Level.Bounds.Right;
                            this.Speed.X = this.Speed.X * -0.4f;
                        }
                    }
                    else
                    {
                        if (base.Left < (float)this.Level.Bounds.Left)
                        {
                            base.Left = (float)this.Level.Bounds.Left;
                            this.Speed.X = this.Speed.X * -0.4f;
                        }
                        else
                        {
                            if (base.Top < (float)(this.Level.Bounds.Top - 4))
                            {
                                base.Top = (float)(this.Level.Bounds.Top + 4);
                                this.Speed.Y = 0f;
                            }
                            else
                            {
                                if (base.Bottom > (float)this.Level.Bounds.Bottom + 4)
                                {
                                    if (!this.exploded)
                                    {
                                        this.Explode();
                                    }
                                    base.Bottom = (float)(this.Level.Bounds.Bottom - 4);
                                }
                            }
                        }
                    }
                    if (base.X < (float)(this.Level.Bounds.Left + 10))
                    {
                        base.MoveH(32f * Engine.DeltaTime, null, null);
                    }
                    Player entity = base.Scene.Tracker.GetEntity<Player>();
                    TempleGate templeGate = base.CollideFirst<TempleGate>();
                    if (templeGate != null && entity != null)
                    {
                        templeGate.Collidable = false;
                        base.MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime, null, null);
                        templeGate.Collidable = true;
                    }
                }
            }
            else
            {
                if (this.respawnTime < this.maxRespawnTime)
                {
                    this.respawnTime += Engine.DeltaTime;
                }
                else
                {
                    this.respawnTime = 0f;
                    this.exploded = false;
                    this.Visible = true;
                    this.Collidable = true;
                    this.playedFuseSound = false;
                    this.sprite.Play("idle", true, false);
                    this.Position = this.startPos;
                    if (this.explodeOnSpawn)
                    {
                        this.exploding = true;
                    }
                }
            }
        }

        [Pooled]
        private class RecoverBlast : Entity
        {
            private Sprite sprite;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                base.Depth = -199;
                bool flag = this.sprite == null;
                if (flag)
                {
                    base.Add(this.sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    this.sprite.OnLastFrame = delegate (string a)
                    {
                        base.RemoveSelf();
                    };
                }
                this.sprite.Play("shockwave", true, false);
                sprite.Rate = 5;
            }

            public static void Spawn(Vector2 position)
            {
                CrystalBomb.RecoverBlast recoverBlast = Engine.Pooler.Create<CrystalBomb.RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }
    }
}