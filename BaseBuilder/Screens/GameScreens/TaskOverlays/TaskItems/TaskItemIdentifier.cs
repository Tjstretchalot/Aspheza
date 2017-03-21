using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.EntityTasks.OverseerTasks;
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

            Register(typeof(EntityConditionTask), typeof(ConditionTaskItem));
            Register(typeof(EntityFailerTask), typeof(FailerTaskItem));
            Register(typeof(EntityHarvestTask), typeof(HarvestTaskItem));
            Register(typeof(EntityInverterTask), typeof(InverterTaskItem));
            Register(typeof(EntityMoveTask), typeof(MoveTaskItem));
            Register(typeof(EntityRepeaterTask), typeof(RepeaterTaskItem));
            Register(typeof(EntitySelectorTask), typeof(SelectorTaskItem));
            Register(typeof(EntitySequenceTask), typeof(SequenceTaskItem));
            Register(typeof(EntitySucceederTask), typeof(SucceederTaskItem));
            Register(typeof(EntityTransferItemTask), typeof(TransferItemTaskItem));
            Register(typeof(SummonChicken), typeof(OverseerSummonChickenTaskItem));
        }

        static void Register(Type entityTaskType, Type taskItemType)
        {
            EntityTaskTypesToTaskItemType[entityTaskType] = taskItemType;
        }

        public static ITaskItem Init(IEntityTask task)
        {
            var itemType = EntityTaskTypesToTaskItemType[task.GetType()];

            ConstructorInfo cstr = null;

            var constructors = itemType.GetConstructors();
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
                throw new InvalidProgramException($"Could not find a constructor that accepts {task.GetType().FullName} for {itemType.FullName}");

            var res = cstr.Invoke(new object[] { task }) as ITaskItem;
            if (res.Expandable)
                res.Expanded = true;
            return res;
        }
    }
}
