using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class FailerTaskItem : TaskItem
    {
        public override IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void LoadedOrChanged(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext)
        {
            throw new NotImplementedException();
        }
    }
}
