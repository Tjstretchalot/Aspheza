using BaseBuilder.Engine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Describes something which can be assigned tasks
    /// </summary>
    public interface ITaskable
    {
        /// <summary>
        /// This event should be raised whenever the current task completes but 
        /// before it's set to null / the next task
        /// </summary>
        event EventHandler TaskFinishing;

        /// <summary>
        /// This event should be raised whenever ClearTasks is called but NOT 
        /// when replace tasks is called
        /// </summary>
        event EventHandler TasksCancelled;

        /// <summary>
        /// Called when ReplaceTasks is called but hasn't been completed
        /// yet
        /// </summary>
        event EventHandler TasksReplacing;

        /// <summary>
        /// Called after tasks have been replaced
        /// </summary>
        event EventHandler TasksReplaced;

        /// <summary>
        /// Gets the current task
        /// </summary>
        IEntityTask CurrentTask { get; }

        /// <summary>
        /// Gets the queued task, not including the current task.
        /// </summary>
        Queue<IEntityTask> TaskQueue { get; }

        /// <summary>
        /// Queue the specified task to occur once the entity has finished other tasks
        /// </summary>
        /// <param name="task"></param>
        void QueueTask(IEntityTask task);

        /// <summary>
        /// Clear all tasks for this entity
        /// </summary>
        /// <param name="gameState">the game state</param>
        void ClearTasks(SharedGameState gameState);

        /// <summary>
        /// Set the current task to null, clear the task queue, and use the
        /// new queue instead. Then update the current task.
        /// </summary>
        /// <param name="newQueue">The new tasks to use</param>
        void ReplaceTasks(Queue<IEntityTask> newQueue);
    }
}
