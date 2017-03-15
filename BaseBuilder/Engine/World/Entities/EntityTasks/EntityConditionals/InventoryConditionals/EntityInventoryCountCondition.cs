using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals.InventoryConditionals
{
    /// <summary>
    /// Determines if an entities inventory has at least / at most a 
    /// certain amount of stuff (or optionally a specific material)
    /// </summary>
    public class EntityInventoryCountCondition : IEntityCondition
    {
        /// <summary>
        /// The key quantity. If AtMost then this is the maximum
        /// amount the inventory can have for true, otherwise
        /// this is the minimum amount the inventory can have
        /// for true.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// If the inventory must have at most the specified quantity
        /// </summary>
        public bool AtMost;

        /// <summary>
        /// The material that we care about, or null for all materials
        /// </summary>
        public Material Material;

        public EntityInventoryCountCondition(int quantity, bool atMost, Material material)
        {
            Quantity = quantity;
            AtMost = atMost;
            Material = material;
        }

        public EntityInventoryCountCondition(NetIncomingMessage message)
        {
            Quantity = message.ReadInt32();
            AtMost = message.ReadBoolean();
            
            if(message.ReadBoolean())
            {
                Material = Material.GetMaterialByID(message.ReadInt32());
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(Quantity);
            message.Write(AtMost);

            if(Material == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(Material.ID);
            }
        }

        public bool Decide(SharedGameState gameState, Entity entity)
        {
            var cont = entity as Container;
            if (cont == null)
                return false;

            var amount = 0;

            if(Material == null)
            {
                amount = cont.Inventory.GetCount();
            }else
            {
                amount = cont.Inventory.GetAmountOf(Material);
            }

            if (AtMost)
                return amount <= Quantity;
            else
                return amount >= Quantity;
        }

    }
}
