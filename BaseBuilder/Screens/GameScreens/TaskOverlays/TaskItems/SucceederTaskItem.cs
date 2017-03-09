using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using Microsoft.Xna.Framework.Audio;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class SucceederTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"A succeeder is a decorator that modifies the result
of its child. If the child returns running, the
succeeder returns running. If the child returns
success, the succeeder returns success. However
if the child returns failure, the succeeder
returns success.";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public SucceederTaskItem(EntitySucceederTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();

            if(task.Child != null)
            {
                var child = TaskItemIdentifier.Init(task.Child);
                child.Parent = this;
                Children.Add(child);
            }

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Succeed";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public SucceederTaskItem()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Succeed";
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if (Children.Count == 0)
            {
                return new EntitySucceederTask(null, "none");
            }
            var childTask = Children[0].CreateEntityTask(taskable, sharedState, localState, netContext);

            return new EntitySucceederTask(childTask, childTask.GetType().Name);
        }


        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 1 && Children[0].IsValid(sharedState, localState, netContext);
        }

        protected override void OnInspectAddPressed()
        {
            if (Children.Count == 1)
            {
                Content.Load<SoundEffect>("UI/TextAreaError").Play();
                return;
            }

            base.OnInspectAddPressed();
        }
    }
}
