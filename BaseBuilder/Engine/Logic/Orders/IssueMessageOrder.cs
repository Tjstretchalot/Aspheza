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
    /// <summary>
    /// Describes an order that adds a message to the RecentMessages in the
    /// SharedGameState
    /// </summary>
    public class IssueMessageOrder : GamePacket, IOrder
    {
        public string Message;

        public IssueMessageOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            Message = null;
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
            Message = message.ReadString();
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
            message.Write(Message);
        }
    }
}
