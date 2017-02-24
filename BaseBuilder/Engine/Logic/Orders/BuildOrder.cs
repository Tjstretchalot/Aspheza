using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.World.Entities;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.Logic.Orders
{
    /// <summary>
    /// An order to build a new building
    /// </summary>
    public class BuildOrder : GamePacket, IOrder
    {
        /// <summary>
        /// The entity to build
        /// </summary>
        public ImmobileEntity Entity;

        public BuildOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            Entity = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            var entTypeID = message.ReadInt16();
            Entity = EntityIdentifier.InitEntity(EntityIdentifier.GetTypeOfID(entTypeID), gameState, message) as ImmobileEntity;
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(EntityIdentifier.GetIDOfEntity(Entity.GetType()));
            Entity.Write(message);
        }
    }
}
