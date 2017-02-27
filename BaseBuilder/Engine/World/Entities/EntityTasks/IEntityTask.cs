using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
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
        /// The description that is displayed to the user for this task
        /// </summary>
        string PrettyDescription { get; }

        /// <summary>
        /// A number between 0 and 1 that indicates the progress made towards
        /// completing the thing specified in the pretty description. 0 means
        /// no progress has been made, 1 means the task is complete.
        /// </summary>
        double Progress { get; }

        /// <summary>
        /// Simulates time passing for the specified entity
        /// </summary>
        /// <param name="gameState">The game state for the entity</param>
        /// <returns>The status of this task</returns>
        EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS);

        /// <summary>
        /// Do stuff like play sounds here. Called once per frame.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game state</param>
        void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState);

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
