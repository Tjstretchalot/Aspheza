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
    public class TransferIdentifier
    {
        protected static Type[] RestrictConstructorParamTypes;
        protected static Dictionary<short, Type> IdsToRestrict;
        protected static Dictionary<Type, short> RestrictToIds;

        static TransferIdentifier()
        {
            RestrictConstructorParamTypes = new Type[] { typeof(NetIncomingMessage) };
            IdsToRestrict = new Dictionary<short, Type>();
            RestrictToIds = new Dictionary<Type, short>();

            Register(typeof(TargetMinLeft), 1);
            Register(typeof(PreventTransferOfMaterial), 2);
            Register(typeof(OnlyTransferMaterial), 3);
        }

        public static void Register(Type type, short id)
        {
            IdsToRestrict.Add(id, type);
            RestrictToIds.Add(type, id);
        }

        public static short GetIDOfTask(Type type)
        {
            return RestrictToIds[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToRestrict[id];
        }

        public static ITransferRestrictors InitTransferRestrict(Type type, NetIncomingMessage message)
        {
            return type.GetConstructor(RestrictConstructorParamTypes).Invoke(new object[] { message }) as ITransferRestrictors;
        }
    }
}
