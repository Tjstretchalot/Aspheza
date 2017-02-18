using BaseBuilder.Engine.Logic.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;

namespace BaseBuilder.Engine.Networking.Packets
{
    /// <summary>
    /// A sync packet is what is sent during the Syncing connection state. The sync
    /// packet contains the orders for one player that will be simulated upon switching
    /// to the Simulated step. A sync packet will be recieved for each player, even if
    /// it contains an empty list.
    /// </summary>
    public class SyncPacket : GamePacket
    {
        public int PlayerID;
        public List<IOrder> Orders;

        public SyncPacket(GamePacketPool pool) : base(pool)
        {
            Orders = new List<IOrder>();
        }

        public override void Clear()
        {
            PlayerID = -1;
            Orders.Clear();
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
            PlayerID = message.ReadInt32();
            int numOrders = message.ReadInt32();

            for(int orderNum = 0; orderNum < numOrders; orderNum++)
            {
                int orderID = message.ReadInt32();
                var pool = context.GetPoolFromPacketID(orderID);

                var order = pool.GetGamePacketFromPool();
                order.LoadFrom(context, message);
                Orders.Add((IOrder)order);
            }
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
            message.Write(PlayerID);
            message.Write(Orders.Count);

            foreach(var order in Orders)
            {
                int orderId = context.GetPoolFromPacketType(order.GetType()).PacketIdentifier;

                message.Write(orderId);

                order.SaveTo(context, message);
            }
        }
    }
}
