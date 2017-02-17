using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Describes a pool of a single type of game packet. Used
    /// in GameConnection.
    /// </summary>
    public class GamePacketPool
    {
        private static Type[] GamePacketConstructorTypes;

        static GamePacketPool()
        {
            GamePacketConstructorTypes = new Type[] { typeof(GamePacketPool) };
        }

        /// <summary>
        /// The identifier for this type of packet. This will be the first short 
        /// in the packet and will be assumed to have been read in GetGamePacket
        /// </summary>
        public int PacketIdentifier { get; }

        /// <summary>
        /// The type of packets this packet pool manages
        /// </summary>
        public Type PacketType { get; }

        /// <summary>
        /// The internal pool of packets
        /// </summary>
        protected Queue<IGamePacket> Pool;

        /// <summary>
        /// Sets up the pool with the specified packet identifier.
        /// </summary>
        /// <param name="packetIdentifier">Identifier for the packet</param>
        public GamePacketPool(int packetIdentifier, Type packetType)
        {
            PacketIdentifier = packetIdentifier;
            PacketType = packetType;
            Pool = new Queue<IGamePacket>();
        }

        /// <summary>
        /// Either creates a new packet or returns a recycled one.
        /// </summary>
        /// <returns>The packet</returns>
        public IGamePacket GetGamePacketFromPool()
        {
            if (Pool.Count > 0)
            {
                return Pool.Dequeue();
            }
            else
            {
                return (IGamePacket) PacketType.GetConstructor(GamePacketConstructorTypes).Invoke(new object[] { this });
            }
        }

        /// <summary>
        /// Gets the cgame packet from the specified message, where a short has already been read
        /// from the message and found to be the packet identifier.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="message">The message</param>
        /// <returns>The game packet representation of the data contained in the message</returns>
        public IGamePacket GetGamePacket(NetContext context, NetIncomingMessage message)
        {
            var result = GetGamePacketFromPool();
            result.LoadFrom(context, message);
            return result;
        }

        /// <summary>
        /// Allows the specified packet to be reused in calls to GetGamePacket
        /// </summary>
        /// <param name="t">The packet</param>
        public void Recycle(IGamePacket t)
        {
            t.Clear();
            Pool.Enqueue(t);
        }
    }
}
