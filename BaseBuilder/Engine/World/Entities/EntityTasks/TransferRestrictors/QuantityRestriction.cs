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
    ///  Simply restricts the transfer to a certain number of items
    /// </summary>
    public class QuantityRestriction : ITransferRestrictor
    {
        public Material KeyMaterial;
        public int KeyQuantity;
        protected int AmountSent;

        public QuantityRestriction(Material material, int quantity)
        {
            KeyMaterial = material;
            KeyQuantity = quantity;
            AmountSent = 0;
        }

        public QuantityRestriction(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
                KeyMaterial = Material.GetMaterialByID(message.ReadInt32());

            KeyQuantity = message.ReadInt32();
            AmountSent = message.ReadInt32();
        }

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
            message.Write(AmountSent);
        }

        public void Restrict(SharedGameState sharedGameState, Container from, Container to, Dictionary<Material, int> maxes)
        {
            int amountToSend = 0;

            if(KeyMaterial != null)
            {
                if (!maxes.TryGetValue(KeyMaterial, out amountToSend))
                    amountToSend = 0;
            }else
            {
                foreach(var kvp in maxes)
                {
                    amountToSend += kvp.Value;
                }
            }

            int toRemove = (AmountSent + amountToSend) - KeyQuantity;
            AmountSent += Math.Max(0, amountToSend - toRemove);

            if (toRemove > 0)
            {
                if (KeyMaterial != null)
                {
                    if (toRemove >= amountToSend)
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

        public void Reset()
        {
            AmountSent = 0;
        }
        
        public bool IsValid()
        {
            return KeyQuantity > 0;
        }
    }
}
