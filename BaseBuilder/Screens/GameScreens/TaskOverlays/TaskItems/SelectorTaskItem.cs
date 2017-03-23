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
    public class SelectorTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"A selector will run each child task in order until 
a child task returns success.Once a child task
returns success, the selector immediately returns
success.If a child task returns failure or running,
the selector will move on to the next item in the
list. If the selector reaches the end of the list
before any children return success, the selector
returns failure.";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public SelectorTaskItem(EntitySelectorTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();

            foreach(var child in task.Children)
            {
                var tmp = TaskItemIdentifier.Init(child);
                tmp.Parent = this;
                Children.Add(tmp);
            }

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Select";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public SelectorTaskItem()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Select";
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var childrenTasks = new List<IEntityTask>();

            foreach (var child in Children)
            {
                childrenTasks.Add(child.CreateEntityTask(taskable, sharedState, localState, netContext));
            }

            return new EntitySelectorTask(childrenTasks, "autogen");
        }

        public override bool IsValid()
        {
            if (Children.Count == 0)
                return false;

            foreach(var child in Children)
            {
                if (!child.IsValid())
                    return false;
            }

            return true;
        }
    }
}
