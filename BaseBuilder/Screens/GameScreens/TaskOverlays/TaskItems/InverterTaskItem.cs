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
    public class InverterTaskItem : SimpleTaskItem
    {
        protected const string _InspectDescription = @"An invert task is a decorator that modifies the result
of its child task. If the child returns running, the
inverter returns running. If the child returns success,
the inverter returns failure. If the child returns 
failure, the inverter returns success.";

        public InverterTaskItem()
        {
            Children = new List<ITaskItem>(1);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Invert";
        }
        
        public InverterTaskItem(IEntityTask task) : this()
        {
            Task = task;

            var inverter = task as EntityInverterTask;

            if(inverter.Child != null)
            {
                var child = TaskItemIdentifier.Init(inverter.Child);
                child.Parent = this;
                Children.Add(child);
            }
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if(Children.Count != 1)
            {
                return new EntityInverterTask(null);
            }else
            {
                var child = Children[0].CreateEntityTask(taskable, sharedState, localState, netContext);

                return new EntityInverterTask(child);
            }
        }

        public override bool IsValid()
        {
            if (Children.Count != 1)
                return false;

            return Children[0].IsValid();
        }
    }
}
