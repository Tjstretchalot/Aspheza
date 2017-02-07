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
        public abstract IEnumerable<IGamePacket> ReadIncomingPackets(UpdateContext context);
        public abstract void SendPacket(UpdateContext context, IGamePacket packet);

        private List<GamePacketPool> GamePacketPools;
        private Dictionary<Type, int> PacketTypesToIndexInGamePacketPools;
        private Dictionary<int, int> PacketIdsToIndexInGamePacketPools;

        protected GameConnection()
        {
            GamePacketPools = new List<GamePacketPool>();
            PacketTypesToIndexInGamePacketPools = new Dictionary<Type, int>();
            PacketIdsToIndexInGamePacketPools = new Dictionary<int, int>();

            RegisterPackets();
        }

        protected void RegisterPacketType(int packetId, Type packetType)
        {
            GamePacketPools.Add(new GamePacketPool(packetId, packetType));
            PacketTypesToIndexInGamePacketPools.Add(packetType, GamePacketPools.Count - 1);
            PacketIdsToIndexInGamePacketPools.Add(packetId, GamePacketPools.Count - 1);
        }

        protected void RegisterPackets()
        {
            RegisterPacketType(1, typeof(WorldDownloadPacket));
        }

        /// <summary>
        /// Reads incoming packets from the specified peer.
        /// </summary>
        /// <param name="context">Update context.</param>
        /// <param name="peer">The peer</param>
        /// <returns>Any incoming packets.</returns>
        protected IEnumerable<IGamePacket> ReadIncomingPackets(UpdateContext context, NetPeer peer)
        {
            NetIncomingMessage msg;

            while((msg = peer.ReadMessage()) != null)
            {
                var packet = HandleMessage(context, peer, msg);

                if (packet != null)
                    yield return packet;
            }
        }

        protected virtual IGamePacket HandleMessage(UpdateContext context, NetPeer peer, NetIncomingMessage msg)
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
                    var packet = ReadIncomingPacket(context, msg);
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
        /// <param name="context">Update context.</param>
        /// <param name="msg">The message (of NetIncomingMessageType.Data)</param>
        /// <returns>The game packet from the message</returns>
        protected IGamePacket ReadIncomingPacket(UpdateContext context, NetIncomingMessage msg)
        {
            var id = msg.ReadInt32();

            var pool = GamePacketPools[PacketIdsToIndexInGamePacketPools[id]];
            var packet = pool.GetGamePacket(context, msg);

            return packet;
        }

        /// <summary>
        /// Sends the specified packet to the specified peer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="packet">The packet to send.</param>
        /// <param name="peer">The peer to send the packet to.</param>
        protected void SendPacket(UpdateContext context, IGamePacket packet, NetPeer peer, NetDeliveryMethod method)
        {
            var outgoing = peer.CreateMessage();

            outgoing.Write(GamePacketPools[PacketTypesToIndexInGamePacketPools[packet.GetType()]].PacketIdentifier);
            packet.SaveTo(context, outgoing);

            foreach (var conn in peer.Connections)
            {
                peer.SendMessage(outgoing, conn, method);
            }
        }
    }
}
