using BaseBuilder.Engine.State.Resources;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class ITransferResultDecider
    {
        protected Dictionary<Material, int> AmountsToTransferBeforeContinuing;
        protected Dictionary<Material, int> AmountsTransfered;

        public ITransferResultDecider(Dictionary<Material, int> amountsToTransferBeforeContinuing = null)
        {
            AmountsToTransferBeforeContinuing = amountsToTransferBeforeContinuing;
            Reset();
        }

        public ITransferResultDecider(NetIncomingMessage message)
        {
            var count = message.ReadInt32();
            for (; count > 0; count--)
            {
                AmountsToTransferBeforeContinuing.Add(Material.GetMaterialByID(message.ReadInt32()), message.ReadInt32());
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(AmountsToTransferBeforeContinuing.Count);
            foreach(var tup in AmountsToTransferBeforeContinuing)
            {
                message.Write(tup.Key.ID);
                message.Write(tup.Value);
            }
        }

        public void Reset()
        {
            foreach (Material mat in AmountsToTransferBeforeContinuing.Keys)
            {
                AmountsTransfered.Add(mat, 0);
            }
        }

        public EntityTaskStatus Decide(Dictionary<Material, int> transferedThisTime)
        {
            if (AmountsToTransferBeforeContinuing == null)
            {
                return EntityTaskStatus.Success;
            }

            foreach (Material mat in AmountsToTransferBeforeContinuing.Keys)
            {
                AmountsTransfered[mat] += transferedThisTime[mat];
            }

            foreach (Material mat in AmountsToTransferBeforeContinuing.Keys)
            {
                if (AmountsToTransferBeforeContinuing[mat] > AmountsTransfered[mat])
                    return EntityTaskStatus.Running;
            }

            foreach (Material mat in AmountsToTransferBeforeContinuing.Keys)
            {
                AmountsTransfered[mat] = 0;
            }

            return EntityTaskStatus.Success;
        }
    }
}
