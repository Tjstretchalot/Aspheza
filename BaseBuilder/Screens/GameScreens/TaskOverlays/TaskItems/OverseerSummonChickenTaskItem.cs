using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.EntityTasks.OverseerTasks;
using BaseBuilder.Engine.World.Entities.Utilities;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Screens.Components.ScrollableComponents;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class OverseerSummonChickenTaskItem : ComplexTaskItem
    {
        public const string _InspectDescription = @"This is a special leaf task for the wizard, which
allows him to convert 6 wheat to 1 chicken over
the course of 2 minutes.";

        public OverseerSummonChickenTaskItem()
        {
            InspectDescription = _InspectDescription;

            Children = new List<ITaskItem>();
            TaskName = "Summon Chicken";
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            Savable = false;
        }

        public OverseerSummonChickenTaskItem(IEntityTask task) : this()
        {
            Task = task;
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return new SummonChicken(((Thing)taskable).ID);
        }

        public override bool IsValid()
        {
            return true;
        }

        protected override IScrollableComponent InitializeComponent(RenderContext context)
        {
            return CreatePadding(1, 1);
        }

        protected override void LoadFromTask(RenderContext context)
        {
        }

        public override bool CanBeAssignedTo(ITaskable taskable)
        {
            return typeof(OverseerMage).IsAssignableFrom(taskable.GetType());
        }
    }
}
