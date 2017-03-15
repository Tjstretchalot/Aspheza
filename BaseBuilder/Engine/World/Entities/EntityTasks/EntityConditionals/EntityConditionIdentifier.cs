using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals.InventoryConditionals;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals
{
    /// <summary>
    /// Identifies entity conditions.
    /// </summary>
    public class EntityConditionIdentifier
    {
        protected static BiDictionary<short, Type> IdsToConditions;
        protected static Type[] ConstructorParams;

        static EntityConditionIdentifier()
        {
            ConstructorParams = new[] { typeof(NetIncomingMessage) };

            IdsToConditions = new BiDictionary<short, Type>();

            Register(1, typeof(EntityInventoryHasOpenSlotCondition));
            Register(2, typeof(EntityInventoryCountCondition));
        }

        public static void Register(short id, Type type)
        {
            IdsToConditions.Add(id, type);
        }

        public static short GetIDOfType(Type type)
        {
            return IdsToConditions[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToConditions[id];
        }

        public static IEntityCondition Init(Type type, NetIncomingMessage message)
        {
            var constr = type.GetConstructor(ConstructorParams);
            return (IEntityCondition)constr.Invoke(new[] { message });
        }
    }
}
