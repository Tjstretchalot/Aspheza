using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders
{
    /// <summary>
    /// Spits out a result based on the items in the froms inventory. The 
    /// condition is that the sender has no more than a specified number 
    /// of items.
    /// </summary>
    public class FromInventoryResultDecider : ITransferResultDecider
    {
        /// <summary>
        /// The material that we care about, or null
        /// </summary>
        public Material KeyMaterial;

        /// <summary>
        /// The key quantity or null
        /// </summary>
        public int KeyQuantity;

        /// <summary>
        /// The result if the condition is unmet
        /// </summary>
        public EntityTaskStatus ResultIfConditionUnmet;
        
        /// <summary>
        /// Initializes a new result decider that will return resultIfConditionUnmet if there is more than
        /// quantity of material (or anything if material is null) in the from inventory.
        /// </summary>
        /// <param name="material">The material</param>
        /// <param name="quantity">The quantity</param>
        /// <param name="resultIfConditionUnmet">The result if condition not met</param>
        public FromInventoryResultDecider(Material material, int quantity, EntityTaskStatus resultIfConditionUnmet)
        {
            KeyMaterial = material;
            KeyQuantity = quantity;
            ResultIfConditionUnmet = resultIfConditionUnmet;
        }

        /// <summary>
        /// Initializes a new result decider that was of this type and serialized with Write
        /// </summary>
        /// <param name="message">The message</param>
        public FromInventoryResultDecider(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
                KeyMaterial = Material.GetMaterialByID(message.ReadInt32());

            KeyQuantity = message.ReadInt32();
            ResultIfConditionUnmet = (EntityTaskStatus)message.ReadInt32();
        }

        /// <summary>
        /// Writes this result decider to the message to be used in the 
        /// constructor accepting a NetIncomingMessage
        /// </summary>
        /// <param name="message">The message</param>
        public void Write(NetOutgoingMessage message)
        {
            if(KeyMaterial == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(KeyMaterial.ID);
            }

            message.Write(KeyQuantity);
            message.Write((int)ResultIfConditionUnmet);
        }

        /// <summary>
        /// Determines the result of the transfer item task
        /// </summary>
        /// <param name="from">the inventory items came from</param>
        /// <param name="to">the inventory items went to</param>
        /// <param name="itemsTransferred">what items were transferred</param>
        /// <returns>the result of the transfer item task</returns>
        public EntityTaskStatus GetResult(Container from, Container to, Dictionary<Material, int> itemsTransferred)
        {
            int amountWeHave = 0;

            if(KeyMaterial != null)
            {
                amountWeHave = from.Inventory.GetAmountOf(KeyMaterial);
            }else
            {
                amountWeHave = from.Inventory.GetCount();
            }

            if (amountWeHave > KeyQuantity)
                return ResultIfConditionUnmet;

            return EntityTaskStatus.Success;
        }

        /// <summary>
        /// Determines if this task will be able to run and do something
        /// that could feasibly be desired
        /// </summary>
        /// <returns>If this result decider is valid</returns>
        public bool IsValid()
        {
            return KeyQuantity > 0;
        }

        /// <summary>
        /// Resets this result decider
        /// </summary>
        public void Reset()
        {
        }
    }
}
