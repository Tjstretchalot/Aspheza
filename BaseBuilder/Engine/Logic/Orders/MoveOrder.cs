using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Context;
using Lidgren.Network;

namespace BaseBuilder.Engine.Logic.Orders
{
    class MoveOrder : GamePacket, IOrder
    {
        public MoveOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
            throw new NotImplementedException();
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
