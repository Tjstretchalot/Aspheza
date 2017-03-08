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
    class OnlyTransferMaterial : ITransferRestrictors
    {
        protected Material Mat;

        public OnlyTransferMaterial(Material mat)
        {
            Mat = mat;
        }

        public OnlyTransferMaterial(NetIncomingMessage message)
        {
            Mat = Material.GetMaterialByID(message.ReadInt32());
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(Mat.ID);
        }

        public void Restrict(SharedGameState sharedGameState, Container from, Entity to, Dictionary<Material, int> maxes)
        {
            foreach (Material mat in maxes.Keys)
            {
                if(mat != Mat)
                {
                    maxes.Remove(mat);
                }
            }
        }

        public void Reset()
        {
        }
    }
}
