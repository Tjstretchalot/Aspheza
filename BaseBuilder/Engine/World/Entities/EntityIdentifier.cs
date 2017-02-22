using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities
{
    public class EntityIdentifier
    {
        protected static Type[] EntityConstructorParamTypes;
        protected static Dictionary<short, Type> IdsToEntities;
        protected static Dictionary<Type, short> EntitiesToIds;

        static EntityIdentifier()
        {
            EntityConstructorParamTypes = new Type[] { };
            IdsToEntities = new Dictionary<short, Type>();
            EntitiesToIds = new Dictionary<Type, short>();

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Archer).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Overseer).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(OverseerMage).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CaveManWorker).TypeHandle);

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(House).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(MageTower).TypeHandle);
        }

        public static void Register(Type entityType, short id)
        {
            IdsToEntities.Add(id, entityType);
            EntitiesToIds.Add(entityType, id);
        }

        public static short GetIDOfEntity(Type entityType)
        {
            return EntitiesToIds[entityType];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToEntities[id];
        }

        public static Entity InitEntity(Type type, SharedGameState gameState, NetIncomingMessage message)
        {
            var entity = type.GetConstructor(EntityConstructorParamTypes).Invoke(new object[] { }) as Entity;
            entity.FromMessage(gameState, message);
            return entity;
        }
    }
}
