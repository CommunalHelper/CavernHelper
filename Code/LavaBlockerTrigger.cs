using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.CavernHelper
{
    public class LavaBlockerTrigger : Trigger
    {
        List<RisingLava> risingLavas;
        List<SandwichLava> sandwichLavas;
        private bool canReenter;
        private bool enabled = true;

        public LavaBlockerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            canReenter = data.Bool("canReenter", false);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            risingLavas = scene.Entities.OfType<RisingLava>().ToList<RisingLava>();
            sandwichLavas = scene.Entities.OfType<SandwichLava>().ToList<SandwichLava>();
        }

        public override void OnStay(Player player)
        {
            if (enabled)
            {
                foreach (RisingLava lava in risingLavas)
                {
                    var data = new DynData<RisingLava>(lava);
                    data.Set<bool>("waiting", true);
                }
                foreach (SandwichLava sLava in sandwichLavas)
                {
                    var data = new DynData<SandwichLava>(sLava);
                    data.Set<bool>("Waiting", true);
                }
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (!canReenter)
            {
                enabled = false;
            }
        }
    }
}