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
    public class MineGoldTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = "WIP";

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public MineGoldTaskItem(EntityMineGoldTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Mine Gold";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public MineGoldTaskItem()
        {
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Mine Gold";
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
