using System;
using System.Collections.Generic;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using Microsoft.Xna.Framework.Audio;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// This is the task item for a EntityFailerTask
    /// </summary>
    public class FailerTaskItem : SimpleTaskItem
    {
        protected const string _InspectDescription = @"A fail task is a decorator that modifies the result 
of its child task. If the child returns running, the
failer returns running. If the child returns success,
the failer returns failure. If the child returns 
failure, the failer returns failure.";

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
            if (task.Child != null)
            {
                var child = TaskItemIdentifier.Init(task.Child);
                child.Parent = this;
                Children.Add(child);
            }

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
        
        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if(Children.Count == 0)
            {
                return new EntityFailerTask(null, "none");
            }
            var childTask = Children[0].CreateEntityTask(taskable, sharedState, localState, netContext);

            return new EntityFailerTask(childTask, childTask.GetType().Name);
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 1 && Children[0].IsValid(sharedState, localState, netContext);
        }

        protected override void OnInspectAddPressed()
        {
            if(Children.Count == 1)
            {
                Content.Load<SoundEffect>("UI/TextAreaError").Play();
                return;
            }

            base.OnInspectAddPressed();
        }
    }
}
