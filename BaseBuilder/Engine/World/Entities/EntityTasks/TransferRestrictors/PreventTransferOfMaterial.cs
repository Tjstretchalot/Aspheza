using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors
{
    class PreventTransferOfMaterial : ITransferRestrictor
    {
        protected Material ClearMat;

        public PreventTransferOfMaterial(Material mat)
        {
            ClearMat = mat;
        }

        public PreventTransferOfMaterial(NetIncomingMessage message)
        {
            ClearMat = Material.GetMaterialByID(message.ReadInt32());
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(ClearMat.ID);
        }

        public void Restrict(SharedGameState sharedGameState, Container from, Entity to, Dictionary<Material, int> maxes)
        {
            maxes.Remove(ClearMat);
        }

        public void Reset()
        {
        }
    }
}
