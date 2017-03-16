using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.WorldObject.Entities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters
{
    /// <summary>
    /// Finds a target at the specified position.
    /// </summary>
    public class TransferTargetByPosition : ITransferTargeter
    {
        /// <summary>
        /// The position
        /// </summary>
        public PointI2D Position;

        public TransferTargetByPosition(PointI2D position)
        {
            Position = position;
        }

        public TransferTargetByPosition(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
                Position = new PointI2D(message);
        }

        public void Write(NetOutgoingMessage message)
        {
            if(Position == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                Position.Write(message);
            }
        }

        public Entity FindTarget(SharedGameState sharedState, MobileEntity source)
        {
            if (Position.X < 0 || Position.Y < 0 || Position.X >= sharedState.World.TileWidth || Position.Y >= sharedState.World.TileHeight)
                return null;

            return sharedState.World.GetEntityAtLocation(Position);
        }

        public bool IsValid()
        {
            return Position != null;
        }
    }
}
