using BaseBuilder.Engine.State;
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
    /// Identifies tasks
    /// </summary>
    public class TransferRestrictorIdentifier
    {
        protected static Type[] TransferRestrictorConstructorParamTypes;
        protected static Dictionary<short, Type> IdsToTransferRestrictor;
        protected static Dictionary<Type, short> TransferRestrictorToIds;

        static TransferRestrictorIdentifier()
        {
            TransferRestrictorConstructorParamTypes = new Type[] { typeof(NetIncomingMessage) };
            IdsToTransferRestrictor = new Dictionary<short, Type>();
            TransferRestrictorToIds = new Dictionary<Type, short>();

            Register(typeof(TargetMinLeft), 1);
            Register(typeof(PreventTransferOfMaterial), 2);
            Register(typeof(OnlyTransferMaterial), 3);
        }

        public static void Register(Type type, short id)
        {
            IdsToTransferRestrictor.Add(id, type);
            TransferRestrictorToIds.Add(type, id);
        }

        public static short GetIDOfTransferRestrictor(Type type)
        {
            return TransferRestrictorToIds[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToTransferRestrictor[id];
        }

        public static ITransferRestrictor InitTransferRestrictor(Type type, NetIncomingMessage message)
        {
            return type.GetConstructor(TransferRestrictorConstructorParamTypes).Invoke(new object[] { message }) as ITransferRestrictor;
        }
    }
}
