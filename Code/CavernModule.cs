using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.CavernHelper
{
    public class CavernModule : EverestModule
    {
        public override Type SettingsType
        {
            get
            {
                return null;
            }
        }

        public static SpriteBank SpriteBank;
        private Level currentLevel;

        public override void Load()
        {
            Everest.Events.Level.OnLoadEntity += this.OnLoadEntity;
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
            On.Celeste.Player.BoostBegin += Player_BoostBegin;
        }

        private void Player_BoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self)
        {
            //"Fix" for dropping holdables when you enter a bubble
            if (currentLevel.Session.Area.SID == "exudias/2/CavernoftheAncients")
            {
                self.RefillDash();
                self.RefillStamina();
            }
            else
            {
                orig(self);
            }
        }

        public override void LoadContent(bool firstLoad)
        {
            CavernModule.SpriteBank = new SpriteBank(GFX.Game, "Graphics/CavernSprites.xml");
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadEntity -= this.OnLoadEntity;
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
            On.Celeste.Player.BoostBegin -= Player_BoostBegin;
        }

        private void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            currentLevel = level;
        }

        private bool OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            if (!entityData.Name.StartsWith("cavern/"))
            {
                return false;
            }
            string name = entityData.Name.Substring(7);
            switch (name)
            {
                case "crystalbomb":
                    level.Add(new CrystalBomb(entityData, offset));
                    return true;
                case "dialogtrigger":
                    int id = entityData.ID;
                    EntityID entityID = new EntityID(levelData.Name, id);
                    level.Add(new DialogCutsceneTrigger(entityData, offset, entityID));
                    return true;
                case "icyfloor":
                    level.Add(new IcyFloor(entityData, offset, entityData.Width));
                    return true;
                case "coremodetrigger":
                    level.Add(new CoreModeTrigger(entityData, offset));
                    return true;
                case "lavablockertrigger":
                    level.Add(new LavaBlockerTrigger(entityData, offset));
                    return true;
                case "fakecavernheart":
                    level.Add(new FakeCavernHeart(entityData, offset));
                    return true;
                default:
                    return false;
            }
        }
    }
}
