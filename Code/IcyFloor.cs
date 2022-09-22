using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CavernHelper
{
    class IcyFloor : Entity
    {
        private StaticMover staticMover;
        private List<Sprite> tiles;
        private float width;

        public IcyFloor(EntityData data, Vector2 offset, float _width) : base(data.Position + offset)
        {
            base.Depth = 1999;
            this.width = _width;
            base.Collider = new Hitbox(width, 2, 0f, 6);
            base.Add(this.staticMover = new StaticMover());
            this.tiles = this.BuildSprite();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.tiles.ForEach(delegate (Sprite t)
            {
                t.Play("ice", false, false);
                t.Rotation = -(float)Math.PI / 2;
            });
        }

        private List<Sprite> BuildSprite()
        {
            List<Sprite> list = new List<Sprite>();
            int currentWidth = 0;
            while ((float)currentWidth < base.Width)
            {
                string id;
                if (currentWidth == 0)
                {
                    id = "WallBoosterTop";
                }
                else
                {
                    if ((float)(currentWidth + 16) > base.Width)
                    {
                        id = "WallBoosterBottom";
                    }
                    else
                    {
                        id = "WallBoosterMid";
                    }
                }
                Sprite sprite = GFX.SpriteBank.Create(id);
                sprite.Position = new Vector2((float)currentWidth, 8);
                list.Add(sprite);
                base.Add(sprite);
                currentWidth += 8;
            }
            return list;
        }
    }
}
