using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using BaseBuilder.Engine.World.WorldObject.Entities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters
{
    /// <summary>
    /// Finds a target by searching by id.
    /// </summary>
    public class TransferTargetByID : ITransferTargeter
    {
        public int TargetID;
        
        public TransferTargetByID(int id)
        {
            TargetID = id;
        }

        public TransferTargetByID(NetIncomingMessage message)
        {
            TargetID = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(TargetID);
        }

        public Entity FindTarget(SharedGameState sharedState, MobileEntity source)
        {
            return sharedState.World.GetEntityByID(TargetID);
        }

        public bool IsValid()
        {
            return TargetID >= 0;
        }
    }
}
