using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals.InventoryConditionals
{
    /// <summary>
    /// Determines if an entities inventory has any open slots available
    /// </summary>
    public class EntityInventoryHasOpenSlotCondition : IEntityCondition
    {
        public EntityInventoryHasOpenSlotCondition()
        {

        }

        public EntityInventoryHasOpenSlotCondition(NetIncomingMessage message)
        {

        }

        public void Write(NetOutgoingMessage message)
        {

        }

        public bool Decide(SharedGameState gameState, Entity entity)
        {
            var cont = entity as Container;
            if (cont == null)
                return false;

            return cont.Inventory.HasOpenSlot;
        }
    }
}
