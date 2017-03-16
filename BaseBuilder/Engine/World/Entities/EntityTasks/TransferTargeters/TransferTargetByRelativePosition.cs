using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.WorldObject.Entities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters
{
    /// <summary>
    /// Finds a target based on its relative position to the source
    /// </summary>
    public class TransferTargetByRelativePosition : ITransferTargeter
    {
        public VectorD2D Offset;
        
        public TransferTargetByRelativePosition(VectorD2D offset)
        {
            Offset = offset;
        }

        public TransferTargetByRelativePosition(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
                Offset = new VectorD2D(message);
        }

        public void Write(NetOutgoingMessage message)
        {
            if(Offset == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                Offset.Write(message);
            }
        }

        public Entity FindTarget(SharedGameState sharedState, MobileEntity source)
        {
            var pos = source.Position + Offset;
            if (pos.X < 0 || pos.Y < 0 || pos.X >= sharedState.World.TileWidth || pos.Y >= sharedState.World.TileHeight)
                return null;

            return sharedState.World.GetEntityAtLocation(pos);
        }

        public bool IsValid()
        {
            return Offset != null;
        }

    }
}
