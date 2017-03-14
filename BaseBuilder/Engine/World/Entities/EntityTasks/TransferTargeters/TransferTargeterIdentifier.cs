using BaseBuilder.Engine.Utility;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters
{
    /// <summary>
    /// Identifies the transfer targeters
    /// </summary>
    public class TransferTargeterIdentifier
    {
        protected static BiDictionary<short, Type> IdsToTargeterTypes;

        protected static Type[] ConstructorParams;

        static TransferTargeterIdentifier()
        {
            IdsToTargeterTypes = new BiDictionary<short, Type>();
            ConstructorParams = new [] { typeof(NetIncomingMessage) };

            Register(typeof(TransferTargetByID), 1);
            Register(typeof(TransferTargetByPosition), 2);
            Register(typeof(TransferTargetByRelativePosition), 3);
        }

        public static void Register(Type type, short id)
        {
            IdsToTargeterTypes.Add(id, type);
        }

        public static short GetIDOfType(Type type)
        {
            return IdsToTargeterTypes[type];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToTargeterTypes[id];
        }

        public static ITransferTargeter Init(Type type, NetIncomingMessage message)
        {
            var constr = type.GetConstructor(ConstructorParams);

            return (ITransferTargeter)constr.Invoke(new[] { message });
        }
    }
}
