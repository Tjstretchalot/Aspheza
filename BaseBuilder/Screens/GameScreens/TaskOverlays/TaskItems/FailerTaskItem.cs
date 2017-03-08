using System;
using System.Collections.Generic;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// This is the task item for a EntityFailerTask
    /// </summary>
    public class FailerTaskItem : SimpleTaskItem
    {
        protected const string _InspectDescription = @"A failer task runs a child task. If the child
task returns running, the failer task returns
running. However, if the child returns failure
or success, the failer task will return failure.";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public FailerTaskItem(EntityFailerTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>(1);
            var child = TaskItemIdentifier.Init(task.Child);
            child.Parent = this;
            Children.Add(child);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Fail";
        }

        /// <summary>
        /// Creates a failer task item with a child already set.
        /// </summary>
        /// <param name="child">The child</param>
        public FailerTaskItem(ITaskItem child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            Children = new List<ITaskItem>(1);
            Children.Add(child);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Fail";
        }

        /// <summary>
        /// Creates a failer task item with no child. This will
        /// mean the failer task item is in a bad state.
        /// </summary>
        public FailerTaskItem()
        {
            Children = new List<ITaskItem>(1);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Fail";
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
