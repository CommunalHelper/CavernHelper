using Monocle;

namespace Celeste.Mod.CavernHelper {
    public class CavernModule : EverestModule {
        public static SpriteBank SpriteBank;

        public override void Load() {
            CavernInterop.Load();
            CrystalBombFieldRenderer.Load();
            On.Celeste.Player.BoostBegin += Player_BoostBegin;
        }

        public override void Unload() {
            CrystalBombFieldRenderer.Unload();
            On.Celeste.Player.BoostBegin -= Player_BoostBegin;
        }

        public override void LoadContent(bool firstLoad) {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/CavernSprites.xml");
        }

        private void Player_BoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self) {
            // "Fix" for dropping holdables when you enter a bubble
            // Note: this is now editable in metadata, but this map is no longer maintained
            if (self.SceneAs<Level>().Session.Area.SID == "exudias/2/CavernoftheAncients") {
                self.RefillDash();
                self.RefillStamina();
            } else {
                orig(self);
            }
        }
    }
}
