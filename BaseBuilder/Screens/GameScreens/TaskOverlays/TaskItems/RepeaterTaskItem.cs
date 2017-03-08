using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class RepeaterTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = "WIP";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public RepeaterTaskItem(EntityRepeaterTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>(1);
            Children.Add(TaskItemIdentifier.Init(task));

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Repeat";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public RepeaterTaskItem()
        {
            Children = new List<ITaskItem>(1);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Repeat";
        }

        public override IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var childTask = Children[0].CreateEntityTask(sharedState, localState, netContext);

            return new EntityFailerTask(childTask, childTask.GetType().Name);
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 1 && Children[0].IsValid(sharedState, localState, netContext);
        }
    }
}
