using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
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
        }

        public static void Register(Type entityType, short id)
        {
            IdsToEntities.Add(id, entityType);
            EntitiesToIds.Add(entityType, id);
        }

        public static short GetIDOfEntity(Type tileType)
        {
            return EntitiesToIds[tileType];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToEntities[id];
        }

        public static Entity InitEntity(Type type, NetIncomingMessage message)
        {
            var entity = type.GetConstructor(EntityConstructorParamTypes).Invoke(new object[] { }) as Entity;
            entity.FromMessage(message);
            return entity;
        }
    }
}
