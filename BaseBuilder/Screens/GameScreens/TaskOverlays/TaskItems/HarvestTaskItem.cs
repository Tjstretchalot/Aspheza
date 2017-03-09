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
    public class HarvestTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"Harvests something, such as a farm, tree, or 
production building.";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public HarvestTaskItem(EntityHarvestTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Harvest";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public HarvestTaskItem()
        {
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Harvest";
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
