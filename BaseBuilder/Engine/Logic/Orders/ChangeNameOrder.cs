using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class ChangeNameOrder : GamePacket, IOrder
    {
        public int PlayerID;
        public string NewName;

        public ChangeNameOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            PlayerID = -1;
            NewName = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            PlayerID = message.ReadInt32();
            NewName = message.ReadString();
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(PlayerID);
            message.Write(NewName);
        }
    }
}
