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
    public class SequenceTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"A sequence will run each child task in order until 
it returns success. Until the sequence reaches the 
end of the list of children or a child returns 
failure, the sequence will return running. If a 
child task returns failure, the sequence will 
immediately return failure. If all children return
success, the sequence will return success.";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public SequenceTaskItem(EntitySequenceTask task)
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
            TaskName = "Sequence";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public SequenceTaskItem()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Sequence";
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var childrenTasks = new List<IEntityTask>();

            foreach(var child in Children)
            {
                childrenTasks.Add(child.CreateEntityTask(taskable, sharedState, localState, netContext));
            }

            return new EntitySequenceTask(childrenTasks, "autogen");
        }

        public override bool IsValid()
        {
            foreach(var child in Children)
            {
                if (!child.IsValid())
                    return false;
            }

            return Children.Count > 0;
        }
    }
}
