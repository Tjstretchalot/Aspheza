using BaseBuilder.Engine.State;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Identifies tasks
    /// </summary>
    public class TaskIdentifier
    {
        protected static Type[] TaskConstructorParamTypes;
        protected static Dictionary<short, Type> IdsToTasks;
        protected static Dictionary<Type, short> TasksToIds;

        static TaskIdentifier()
        {
            TaskConstructorParamTypes = new Type[] { typeof(SharedGameState), typeof(NetIncomingMessage) };
            IdsToTasks = new Dictionary<short, Type>();
            TasksToIds = new Dictionary<Type, short>();

            Register(typeof(EntityConditionTask), 1);
            Register(typeof(EntityFailerTask), 2);
            Register(typeof(EntityHarvestTask), 4);
            Register(typeof(EntityInverterTask), 5);
            Register(typeof(EntityMoveTask), 7);
            Register(typeof(EntityRepeaterTask), 8);
            Register(typeof(EntitySelectorTask), 10);
            Register(typeof(EntitySequenceTask), 11);
            Register(typeof(EntitySucceederTask), 12);
            Register(typeof(EntityTransferItemTask), 13);
        }

        public static void Register(Type type, short id)
        {
            IdsToTasks.Add(id, type);
            TasksToIds.Add(type, id);
        }

        public static short GetIDOfTask(Type type)
        {
            return TasksToIds[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToTasks[id];
        }

        public static IEntityTask InitEntityTask(Type type, SharedGameState gameState, NetIncomingMessage message)
        {
            return type.GetConstructor(TaskConstructorParamTypes).Invoke(new object[] { gameState, message }) as IEntityTask;
        }
    }
}
