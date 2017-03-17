using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree;
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
            
            Register(typeof(OverseerMage), 1);
            Register(typeof(CaveManWorker), 2);

            Register(typeof(House), 1001);
            Register(typeof(MageTower), 1002);
            Register(typeof(Sign), 1003);
            Register(typeof(Tree), 1004);
            Register(typeof(GoldOre), 1005);
            Register(typeof(StorageBarn), 1006);
            Register(typeof(Farm), 1007);
            Register(typeof(WaterMill), 1008);
            Register(typeof(Bakery), 1009);
            Register(typeof(Tavern), 1010);
            Register(typeof(Library), 1011);
            Register(typeof(Temple), 1012);
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
