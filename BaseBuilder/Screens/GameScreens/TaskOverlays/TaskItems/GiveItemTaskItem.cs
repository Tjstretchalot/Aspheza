using System;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.Utilities;
using System.Collections.Generic;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class GiveItemTaskItem : SimpleTaskItem
    {
        protected const string _InspectDescription = "WIP";

        public GiveItemTaskItem(EntityGiveItemTask task) : this()
        {
            Task = task;
        }

        public GiveItemTaskItem()
        {
            InspectDescription = _InspectDescription;
            Children = new List<ITaskItem>();

            Expandable = false;
            Expanded = false;
            Savable = false;
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return new EntityGiveItemTask(taskable as Container, taskable as Container, 1);
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return true;
        }
    }
}