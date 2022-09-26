using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.CavernHelper {
    [ModExportName("CavernHelper")]
    public static class CavernInterop {
        internal static void Load() {
            typeof(CavernInterop).ModInterop();
        }

        // Creates and returns a CrystalBombExplosionCollider as a Component.
        //   Action<Vector2> action: the delegate that will be called when the entity collides with the explosion.
        //   Collider collider: the collider to use for the check. Defaults to the entity collider.
        public static Component GetCrystalBombExplosionCollider(Action<Vector2> action, Collider collider = null) {
            return new CrystalBombExplosionCollider(action, collider);
        }

        // Returns whether or not an entity is touching an Icy Floor.
        //   Entity entity: the entity to check.
        public static bool IsOnIcyFloor(Entity entity) {
            return entity.CollideCheck<IcyFloor>();
        }
    }
}
