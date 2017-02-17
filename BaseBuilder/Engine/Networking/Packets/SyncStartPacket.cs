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
    /// The sync start packet is sent by the server when the connection state is
    /// switching from Waiting to Syncing. Upon recieving this packet, the client
    /// should:
    ///   - Copy the local game states orders to a temporary list
    ///   - Clear the local game state orders that are in the copied orders
    ///   - Send the copied orders to the server using a SyncPacket
    ///   - Await SyncPackets from the server and set them to the appropriate player
    /// </summary>
    public class SyncStartPacket : GamePacket
    {
        public SyncStartPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
        }
    }
}
