using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking.Packets
{
    /// <summary>
    /// This is sent to the server when a client has finished downloading a map
    /// and is ready to participate in Syncing
    /// </summary>
    public class ReadyForSyncPacket : GamePacket
    {
        public int PlayerID;

        public ReadyForSyncPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            PlayerID = -1;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            PlayerID = message.ReadInt32();
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(PlayerID);
        }
    }
}
