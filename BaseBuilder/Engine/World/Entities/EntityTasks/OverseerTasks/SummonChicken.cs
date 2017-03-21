using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using Lidgren.Network;
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.OverseerTasks
{
    /// <summary>
    /// Summon a chicken. Takes 2 minutes and requires 6 bread
    /// </summary>
    public class SummonChicken : EntityOverseerTask
    {
        public override string PrettyDescription
        {
            get
            {
                return "Summoning a Chicken";
            }
        }

        public override string TaskName
        {
            get
            {
                return "Summon a Chicken";
            }
        }

        public SummonChicken(int overseerId) : base(overseerId, 120000)
        {
        }

        public SummonChicken(NetIncomingMessage message) : base(message)
        {
        }
        
        protected override bool EatResources(SharedGameState gameState, OverseerMage mage)
        {
            if (mage.Inventory.GetAmountOf(Material.Wheat) < 6)
                return false;

            mage.Inventory.RemoveMaterial(Material.Wheat, 6);
            return true;
        }

        protected override void ReturnResources(SharedGameState gameState, OverseerMage mage)
        {
            mage.Inventory.AddMaterial(Material.Wheat, 6);
        }

        protected override bool ImplTask(SharedGameState gameState, OverseerMage mage)
        {
            return mage.Inventory.AddMaterial(Material.Chicken, 1) == 1;
        }
    }
}
