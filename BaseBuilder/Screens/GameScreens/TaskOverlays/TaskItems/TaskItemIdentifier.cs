using BaseBuilder.Engine.World.Entities.EntityTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static ITaskItem Init(IEntityTask task)
        {
            var itemType = EntityTaskTypesToTaskItemType[task.GetType()];

            ConstructorInfo cstr = null;

            var constructors = task.GetType().GetConstructors();
            foreach(var constructor in constructors)
            {
                var cstrParams = constructor.GetParameters();

                if (cstrParams.Length != 1)
                    continue;

                if(cstrParams[0].ParameterType.IsAssignableFrom(task.GetType()))
                {
                    cstr = constructor;
                    break;
                }
            }

            if (cstr == null)
                throw new InvalidProgramException($"Could not find a constructor that accepts {task.GetType().FullName} for {itemType.GetType().FullName}");

            return cstr.Invoke(new object[] { task }) as ITaskItem;
        }
    }
}
