using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters
{
    /// <summary>
    /// Something that determines the target for a transfer item task
    /// </summary>
    public interface ITransferTargeter
    {
        /// <summary>
        /// Writes this targeter to the message.
        /// </summary>
        /// <param name="message">The message</param>
        void Write(NetOutgoingMessage message);

        /// <summary>
        /// Determines what the target container is. The source is the 
        /// entity who is running the transfer item task.
        /// </summary>
        /// <param name="sharedState">The shared state</param>
        /// <param name="source">The source</param>
        /// <returns>The target</returns>
        Container FindTarget(SharedGameState sharedState, MobileEntity source);

        /// <summary>
        /// Determines if this targeter is valid
        /// </summary>
        /// <returns>valid</returns>
        bool IsValid();
    }
}
