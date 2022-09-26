using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CavernHelper {
    [Tracked]
    [CustomEntity("cavern/icyfloor")]
    public class IcyFloor : Entity {
        private readonly List<Sprite> tiles;

        public IcyFloor(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            Depth = 1999;
            Collider = new Hitbox(data.Width, 2, 0f, 6);
            tiles = BuildSprite();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            tiles.ForEach(delegate (Sprite t) {
                t.Play("ice", false, false);
                t.Rotation = -(float)Math.PI / 2;
            });
        }

        private List<Sprite> BuildSprite() {
            List<Sprite> list = new();
            int currentWidth = 0;
            while (currentWidth < Width) {
                string id;
                if (currentWidth == 0) {
                    id = "WallBoosterTop";
                } else if ((currentWidth + 16) > Width) {
                    id = "WallBoosterBottom";
                } else {
                    id = "WallBoosterMid";
                }

                Sprite sprite = GFX.SpriteBank.Create(id);
                sprite.Position = new Vector2(currentWidth, 8);
                list.Add(sprite);
                Add(sprite);
                currentWidth += 8;
            }

            return list;
        }
    }
}
