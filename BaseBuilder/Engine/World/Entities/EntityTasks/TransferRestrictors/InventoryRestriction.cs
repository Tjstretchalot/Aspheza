using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors
{
    /// <summary>
    /// Restricts a transfer based on how many items would be left
    /// in the giving inventory or how many items would be had
    /// in the recieving inventory.
    /// </summary>
    public class InventoryRestriction : ITransferRestrictor
    {
        /// <summary>
        /// If the material is not null, it is the only material
        /// we will consider in the checks.
        /// </summary>
        public Material KeyMaterial;

        /// <summary>
        /// If we're checking how many items will be in the 
        /// recieving inventory. If false, we're checking how
        /// many items would remain in the giving inventory.
        /// </summary>
        public bool CheckRecievingInventory;

        /// <summary>
        /// The quantity that we are either not allowing the
        /// giver to get below or the reciever to get above.
        /// </summary>
        public int KeyQuantity;

        /// <summary>
        /// Initializes a new inventory restriction 
        /// </summary>
        /// <param name="checkRecieving">True to limit items in recieving, false to prevent giving from going below</param>
        /// <param name="quantity">Key quantity in checks</param>
        /// <param name="keyMaterial">If not null, the only material we check</param>
        public InventoryRestriction(bool checkRecieving, int quantity, Material keyMaterial = null)
        {
            CheckRecievingInventory = checkRecieving;
            KeyQuantity = quantity;
            KeyMaterial = keyMaterial;
        }

        public InventoryRestriction(NetIncomingMessage message)
        {
            CheckRecievingInventory = message.ReadBoolean();
            KeyQuantity = message.ReadInt32();

            if(message.ReadBoolean())
            {
                KeyMaterial = Material.GetMaterialByID(message.ReadInt32());
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(CheckRecievingInventory);
            message.Write(KeyQuantity);
            
            if(KeyMaterial == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(KeyMaterial.ID);
            }
        }
        
        public void Restrict(SharedGameState sharedGameState, Container from, Container to, Dictionary<Material, int> maxes)
        {
            var keyCont = CheckRecievingInventory ? to : from;

            int howMuchWeHave = 0;
            
            if (KeyMaterial != null)
                howMuchWeHave = keyCont.Inventory.GetAmountOf(KeyMaterial);
            else
                howMuchWeHave = keyCont.Inventory.GetCount();

            int currentTransferAmount = 0;
            if (KeyMaterial != null)
            {
                if (!maxes.TryGetValue(KeyMaterial, out currentTransferAmount))
                    currentTransferAmount = 0;
            }
            else
            {
                foreach (var kvp in maxes)
                {
                    currentTransferAmount += kvp.Value;
                }
            }

            if (currentTransferAmount == 0)
                return;

            int toRemove = 0;
            if (CheckRecievingInventory && howMuchWeHave + currentTransferAmount > KeyQuantity)
            {
                toRemove = (howMuchWeHave + currentTransferAmount) - KeyQuantity;
            }else if(!CheckRecievingInventory && howMuchWeHave - currentTransferAmount < KeyQuantity)
            {
                toRemove = KeyQuantity - (howMuchWeHave - currentTransferAmount);
            }
            
            if(toRemove > 0)
            {
                if (KeyMaterial != null)
                {
                    if (toRemove >= currentTransferAmount)
                        maxes.Remove(KeyMaterial);
                    else
                        maxes[KeyMaterial] -= toRemove;
                }
                else
                {
                    var keys = maxes.Keys.ToArray();

                    foreach (var key in keys)
                    {
                        var value = maxes[key];

                        if (value <= toRemove)
                        {
                            maxes.Remove(key);
                            toRemove -= value;

                            if (toRemove == 0)
                                break;
                        }
                        else
                        {
                            maxes[key] = value - toRemove;
                            toRemove = 0;
                            break;
                        }
                    }
                }
            }
        }

        public bool IsValid()
        {
            return KeyQuantity > 0;
        }

        public void Reset()
        {
        }

    }
}
