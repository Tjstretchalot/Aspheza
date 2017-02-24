using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class MoveOrder : GamePacket, IOrder
    {
        public int EntityID;
        public PointI2D End;

        public MoveOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            EntityID = -1;
            End = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            EntityID = message.ReadInt32();
            End = new PointI2D(message);
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(EntityID);
            End.Write(message);
        }
    }
}
