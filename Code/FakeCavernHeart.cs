using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.CavernHelper {
    [CustomEntity("cavern/fakecavernheart")]
    internal class FakeCavernHeart : Entity {
        private Sprite sprite;
        private Wiggler ScaleWiggler;
        private Wiggler moveWiggler;
        private ParticleType shineParticle;
        private float timer;
        private Vector2 moveWiggleDir;
        private float bounceSfxDelay;

        public FakeCavernHeart(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            Add(new MirrorReflection());
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Level level = Scene as Level;
            AreaKey area = level.Session.Area;
            string id = "heartgem" + (int)area.Mode;
            Add(sprite = GFX.SpriteBank.Create(id));
            sprite.Play("spin", false, false);
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
            sprite.OnLoop = delegate (string anim) {
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
                ScaleWiggler.Start();
                (Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f, null, null);
            };
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
                sprite.Scale = Vector2.One * (1f + (f * 0.25f));
            }, false, false));

            Color color;
            switch (area.Mode) {
                case AreaMode.Normal:
                    color = Color.Aqua;
                    shineParticle = HeartGem.P_BlueShine;
                    break;
                case AreaMode.BSide:
                    color = Color.Red;
                    shineParticle = HeartGem.P_RedShine;
                    break;
                default:
                    color = Color.Gold;
                    shineParticle = HeartGem.P_GoldShine;
                    break;
            }

            color = Color.Lerp(color, Color.White, 0.5f);
            Add(new VertexLight(color, 1f, 32, 64));
            moveWiggler = Wiggler.Create(0.8f, 2f, null, false, false);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
        }

        public override void Update() {
            base.Update();
            timer += Engine.DeltaTime;
            bounceSfxDelay -= Engine.DeltaTime;
            sprite.Position = (Vector2.UnitY * (float)Math.Sin((double)(timer * 2f)) * 2f) + (moveWiggleDir * moveWiggler.Value * -8f);
            if (Scene.OnInterval(0.1f)) {
                SceneAs<Level>().Particles.Emit(shineParticle, 1, Center, Vector2.One * 8f);
            }
        }

        public void OnPlayer(Player player) {
            if (bounceSfxDelay <= 0f) {
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounceSfxDelay = 0.1f;
            }

            moveWiggler.Start();
            ScaleWiggler.Start();
            moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
    }
}
