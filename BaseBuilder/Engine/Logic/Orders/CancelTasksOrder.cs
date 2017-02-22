using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class CancelTasksOrder : GamePacket, IOrder
    {
        public int EntityID;

        public CancelTasksOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            EntityID = -1;
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
            EntityID = message.ReadInt32();
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
            message.Write(EntityID);
        }
    }
}
