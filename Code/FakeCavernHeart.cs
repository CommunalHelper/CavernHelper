using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CavernHelper
{
    public class FakeCavernHeart : Entity
    {
        private Sprite sprite;
        private Wiggler ScaleWiggler;
        private Wiggler moveWiggler;
        private VertexLight light;
        private ParticleType shineParticle;
        private float timer;
        private Vector2 moveWiggleDir;
        private float bounceSfxDelay;

        public FakeCavernHeart(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            base.Add(new MirrorReflection());
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = base.Scene as Level;
            AreaKey area = level.Session.Area;
            string id = "heartgem" + (int)area.Mode;
            base.Add(this.sprite = GFX.SpriteBank.Create(id));
            this.sprite.Play("spin", false, false);
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
            this.sprite.OnLoop = delegate (string anim)
            {
                Audio.Play("event:/game/general/crystalheart_pulse", this.Position);
                this.ScaleWiggler.Start();
                (base.Scene as Level).Displacement.AddBurst(this.Position, 0.35f, 8f, 48f, 0.25f, null, null);
            };
            base.Add(this.ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                this.sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }, false, false));
            Color color;
            if (area.Mode == AreaMode.Normal)
            {
                color = Color.Aqua;
                this.shineParticle = HeartGem.P_BlueShine;
            }
            else
            {
                if (area.Mode == AreaMode.BSide)
                {
                    color = Color.Red;
                    this.shineParticle = HeartGem.P_RedShine;
                }
                else
                {
                    color = Color.Gold;
                    this.shineParticle = HeartGem.P_GoldShine;
                }
            }
            color = Color.Lerp(color, Color.White, 0.5f);
            base.Add(this.light = new VertexLight(color, 1f, 32, 64));
            this.moveWiggler = Wiggler.Create(0.8f, 2f, null, false, false);
            this.moveWiggler.StartZero = true;
            base.Add(this.moveWiggler);
        }

        public override void Update()
        {
            base.Update();
            this.timer += Engine.DeltaTime;
            this.bounceSfxDelay -= Engine.DeltaTime;
            this.sprite.Position = Vector2.UnitY * (float)Math.Sin((double)(this.timer * 2f)) * 2f + this.moveWiggleDir * this.moveWiggler.Value * -8f;
            if (base.Scene.OnInterval(0.1f))
            {
                base.SceneAs<Level>().Particles.Emit(this.shineParticle, 1, base.Center, Vector2.One * 8f);
            }
        }

        public void OnPlayer(Player player)
        {
            if (this.bounceSfxDelay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", this.Position);
                this.bounceSfxDelay = 0.1f;
            }
            this.moveWiggler.Start();
            this.ScaleWiggler.Start();
            this.moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
    }
}