using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Identifies transfer restrictors
    /// </summary>
    public class TransferRestrictorIdentifier
    {
        protected static Type[] TransferRestrictorConstructorParamTypes;
        protected static BiDictionary<short, Type> IdsToTransferRestrictor;

        static TransferRestrictorIdentifier()
        {
            TransferRestrictorConstructorParamTypes = new Type[] { typeof(NetIncomingMessage) };
            IdsToTransferRestrictor = new BiDictionary<short, Type>();

            Register(typeof(InventoryRestriction), 1);
            Register(typeof(MaterialRestriction), 2);
            Register(typeof(QuantityRestriction), 3);
        }

        public static void Register(Type type, short id)
        {
            IdsToTransferRestrictor.Add(id, type);
        }

        public static short GetIDOfType(Type type)
        {
            return IdsToTransferRestrictor[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToTransferRestrictor[id];
        }

        public static ITransferRestrictor Init(Type type, NetIncomingMessage message)
        {
            return type.GetConstructor(TransferRestrictorConstructorParamTypes).Invoke(new object[] { message }) as ITransferRestrictor;
        }
    }
}
