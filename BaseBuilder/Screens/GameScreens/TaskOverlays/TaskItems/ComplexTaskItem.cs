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
    /// <summary>
    /// A complex task item is set up to allow for 
    /// </summary>
    public class ComplexTaskItem : SimpleTaskItem
    {
        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }
    }
}
