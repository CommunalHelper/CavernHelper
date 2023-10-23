using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CavernHelper {
    [Tracked]
    // Component that makes Crystal Bombs explode when they touch the collider.
    public class CrystalBombExploderCollider : Component {
        public Collider Collider;

        public CrystalBombExploderCollider(Collider collider = null)
            : base(false, false) {
            Collider = collider;
        }

        internal bool Check(CrystalBomb bomb) {
            Collider origCollider = Entity.Collider;
            if (Collider != null) {
                Entity.Collider = Collider;
            }

            bool result = false;

            if (bomb.CollideCheck(Entity)) {
                result = true;
            }

            Entity.Collider = origCollider;

            return result;
        }
    }
}
