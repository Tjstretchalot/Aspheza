using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors
{
    public class TargetMinLeft : ITransferRestrictor
    {
        protected int MinLeft;

        public TargetMinLeft(int minleft)
        {
            MinLeft = minleft;
        }

        public TargetMinLeft(NetIncomingMessage message)
        {
            MinLeft = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(MinLeft);
        }

        public void Restrict(SharedGameState sharedGameState, Container from, Entity to, Dictionary<Material, int> maxes)
        {
            foreach (var tuples in maxes)
            {
                var amount = from.Inventory.GetAmountOf(tuples.Key) - MinLeft;
                if (amount <= 0)
                {
                    maxes.Remove(tuples.Key);
                }
                else
                {
                    maxes[tuples.Key] = Math.Min(MinLeft, amount);
                }
            }
        }

        public void Reset()
        {
        }
    }
}
