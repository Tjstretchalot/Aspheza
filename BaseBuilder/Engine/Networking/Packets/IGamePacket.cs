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
    /// Describes one chunk of information that can be sent over a GameConnection
    /// </summary>
    public interface IGamePacket
    {
        /// <summary>
        /// Sets the variables of the packet based on the message.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="message">The message</param>
        void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message);

        /// <summary>
        /// Writes this game packet to the specified message.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="message">The message to write</param>
        void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message);

        /// <summary>
        /// Called when a packet is recycled. Should clear any memory-intensive
        /// unless it wants to reuse them and believes it will be called often
        /// enough for that to be useful.
        /// </summary>
        void Clear();

        /// <summary>
        /// Called to recycle this packet.
        /// </summary>
        void Recycle();
    }
}
