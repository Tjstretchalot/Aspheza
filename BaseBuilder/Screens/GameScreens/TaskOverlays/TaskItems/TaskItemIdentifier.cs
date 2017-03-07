using BaseBuilder.Engine.World.Entities.EntityTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class TaskItemIdentifier
    {
        protected static Dictionary<Type, Type> EntityTaskTypesToTaskItemType;

        static TaskItemIdentifier()
        {
            EntityTaskTypesToTaskItemType = new Dictionary<Type, Type>();

            Register(typeof(EntityFailerTask), typeof(FailerTaskItem));
        }

        static void Register(Type entityTaskType, Type taskItemType)
        {
            EntityTaskTypesToTaskItemType[entityTaskType] = taskItemType;
        }

        public ITaskItem InitTaskItemFromEntityTask(IEntityTask task)
        {
            return null; // todo
        }
    }
}
