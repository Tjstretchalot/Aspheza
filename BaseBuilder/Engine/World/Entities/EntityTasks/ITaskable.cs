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
        /// Queue the specified task to occur once the entity has finished other tasks
        /// </summary>
        /// <param name="task"></param>
        void QueueTask(IEntityTask task);

        /// <summary>
        /// Clear all tasks for this entity
        /// </summary>
        /// <param name="gameState">the game state</param>
        void ClearTasks(SharedGameState gameState);
    }
}
