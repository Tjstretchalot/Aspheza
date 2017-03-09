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
    public class RepeatUntilFailTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = "WIP";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public RepeatUntilFailTaskItem(EntityRepeatUntilFailTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Repeat until fail";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public RepeatUntilFailTaskItem()
        {
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Repeat until fail";
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return false;
        }
    }
}
