using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.Networking;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class DeconstructOrder : GamePacket, IOrder
    {
        public int EntityID;

        public DeconstructOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            EntityID = -1;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            EntityID = message.ReadInt32();
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(EntityID);
        }
    }
}
