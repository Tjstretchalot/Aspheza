using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;
using BaseBuilder.Engine.Networking;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Parent class for game connections for client and server.
    /// </summary>
    public abstract class GameConnection : IGameConnection
    {
        public abstract void ConsiderGameUpdate();
        public abstract IEnumerable<IGamePacket> ReadIncomingPackets();
        public abstract void SendPacket(IGamePacket packet);

        protected NetContext Context;

        private List<GamePacketPool> GamePacketPools;
        private Dictionary<Type, int> PacketTypesToIndexInGamePacketPools;
        private Dictionary<int, int> PacketIdsToIndexInGamePacketPools;

        protected GameConnection()
        {
            Context = new NetContext();

            Context.RegisterPackets();
        }


        /// <summary>
        /// Reads incoming packets from the specified peer.
        /// </summary>
        /// <param name="peer">The peer</param>
        /// <returns>Any incoming packets.</returns>
        protected IEnumerable<IGamePacket> ReadIncomingPackets(NetPeer peer)
        {
            NetIncomingMessage msg;

            while((msg = peer.ReadMessage()) != null)
            {
                var packet = HandleMessage(peer, msg);

                if (packet != null)
                    yield return packet;
            }
        }

        protected virtual IGamePacket HandleMessage(NetPeer peer, NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    Console.WriteLine(msg.ReadString());
                    peer.Recycle(msg);
                    break;
                case NetIncomingMessageType.Data:
                    var packet = ReadIncomingPacket(msg);
                    peer.Recycle(msg);
                    return packet;
                default:
                    Console.WriteLine("Unhandled type: " + msg.MessageType);
                    peer.Recycle(msg);
                    break;
            }

            return null;
        }

        /// <summary>
        /// Reads the specified packet that was sent from the specified peer and is
        /// NetIncomingMessageType.Data.
        /// </summary>
        /// <param name="msg">The message (of NetIncomingMessageType.Data)</param>
        /// <returns>The game packet from the message</returns>
        protected IGamePacket ReadIncomingPacket(NetIncomingMessage msg)
        {
            var id = msg.ReadInt32();

            var pool = GamePacketPools[PacketIdsToIndexInGamePacketPools[id]];
            var packet = pool.GetGamePacket(Context, msg);

            return packet;
        }

        /// <summary>
        /// Sends the specified packet to the specified peer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="packet">The packet to send.</param>
        /// <param name="peer">The peer to send the packet to.</param>
        protected void SendPacket(IGamePacket packet, NetPeer peer, NetDeliveryMethod method)
        {
            var outgoing = peer.CreateMessage();

            outgoing.Write(Context.GetPoolFromPacketType(packet.GetType()).PacketIdentifier);
            packet.SaveTo(Context, outgoing);

            foreach (var conn in peer.Connections)
            {
                peer.SendMessage(outgoing, conn, method);
            }
        }


    }
}
