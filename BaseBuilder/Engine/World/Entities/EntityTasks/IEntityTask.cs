using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// <para>Entities can be assigned tasks, such as a MoveTask or an AttackTask.</para>
    /// 
    /// <para>Tasks differ from orders in that Tasks are part of the shared game state (whereas
    /// orders are part of the local game state), tasks always apply to entities (whereas orders
    /// can apply to anything that modifies the shared game state, such as chatting), and tasks
    /// act as a behavior tree (whereas orders never repeat themself).</para>
    /// </summary>
    public interface IEntityTask
    {
        /// <summary>
        /// The name of this task, for debugging
        /// </summary>
        string TaskName { get; }

        /// <summary>
        /// The description of this task, for debugging
        /// </summary>
        string TaskDescription { get; }

        /// <summary>
        /// The status of this task, for debugging
        /// </summary>
        string TaskStatus { get; }

        /// <summary>
        /// Simulates time passing for the specified entity
        /// </summary>
        /// <param name="gameState">The game state for the entity</param>
        /// <returns>The status of this task</returns>
        EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS);

        /// <summary>
        /// Reset this task so it can be run again.
        /// </summary>
        /// <param name="gameState">The game state</param>
        void Reset(SharedGameState gameState);

        /// <summary>
        /// Writes this task to the outgoing message
        /// </summary>
        /// <param name="message"></param>
        void Write(NetOutgoingMessage message);
    }
}
