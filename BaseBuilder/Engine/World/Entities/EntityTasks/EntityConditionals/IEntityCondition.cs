using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals
{
    /// <summary>
    /// The compnoent for an EntityConditionTask. Unlike regular tasks these
    /// should be designed not to require resetting. Should have a constructor
    /// that accepts a net incoming mesage
    /// </summary>
    public interface IEntityCondition
    {
        /// <summary>
        /// Determines if the entity meets the condition.
        /// </summary>
        /// <param name="gameState">The current gamestate</param>
        /// <param name="entity">The entity running the condition</param>
        /// <returns>The condition</returns>
        bool Decide(SharedGameState gameState, Entity entity);

        /// <summary>
        /// Writes this condition to the message
        /// </summary>
        /// <param name="message">The message</param>
        void Write(NetOutgoingMessage message);
    }
}
