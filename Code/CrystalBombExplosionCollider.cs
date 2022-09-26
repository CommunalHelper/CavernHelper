using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.CavernHelper {
    [Tracked]
    public class CrystalBombExplosionCollider : Component {
        public Action<Vector2> OnExplode;
        public Collider Collider;

        public CrystalBombExplosionCollider(Action<Vector2> onExplode, Collider collider = null)
            : base(false, false) {
            OnExplode = onExplode;
            Collider = collider;
        }

        internal void Check(CrystalBomb bomb) {
            if (OnExplode != null) {
                Collider origCollider = Entity.Collider;
                if (Collider != null) {
                    Entity.Collider = Collider;
                }

                if (bomb.CollideCheck(Entity)) {
                    OnExplode.Invoke(bomb.Position);
                }

                Entity.Collider = origCollider;
            }
        }
    }
}
