using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class MoveTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = "WIP";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public MoveTaskItem(EntityMoveTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Move";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public MoveTaskItem()
        {
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Move";
        }

        public override IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return false;
        }
    }
}
