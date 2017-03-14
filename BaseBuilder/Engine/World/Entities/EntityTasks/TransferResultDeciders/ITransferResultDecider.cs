using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders
{
    /// <summary>
    /// Something that decides how the TransferItemTask will return. This is called
    /// after the restrictors have had a chance to restrict the transfer, then the items
    /// were transferred, and the amount passed in here is the items that were actually
    /// transferred.
    /// </summary>
    /// <remarks>
    /// Must have a constructor that accepts just a NetIncomingMessage
    /// </remarks>
    public interface ITransferResultDecider
    {
        /// <summary>
        /// Writes this result decider to the outgoing message
        /// </summary>
        /// <param name="message">The message</param>
        void Write(NetOutgoingMessage message);

        /// <summary>
        /// Gets the result now that an additional itemsTransferred have been transferred.
        /// </summary>
        /// <param name="from">The container the items came from</param> 
        /// <param name="to">The container the items went to</param>
        /// <param name="itemsTransferred">Materials to amounts transferred</param>
        EntityTaskStatus GetResult(Container from, Container to, Dictionary<Material, int> itemsTransferred);

        /// <summary>
        /// Resets this result decider
        /// </summary>
        void Reset();

        /// <summary>
        /// Determines if this result decider is in a good state
        /// </summary>
        /// <returns>if this is in a good state</returns>
        bool IsValid();
    }
}
