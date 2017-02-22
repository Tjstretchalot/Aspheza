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

            Register(typeof(EntityFailerTask), 1);
            Register(typeof(EntityMoveTask), 2);
            Register(typeof(EntityRepeaterTask), 3);
            Register(typeof(EntityRepeatUntilFailTask), 4);
            Register(typeof(EntitySelectorTask), 5);
            Register(typeof(EntitySequenceTask), 6);
            Register(typeof(EntitySucceederTask), 7);
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
