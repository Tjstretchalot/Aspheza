using BaseBuilder.Engine.Utility;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders
{
    /// <summary>
    /// Identifies transfer result deciders
    /// </summary>
    public class TransferResultDeciderIdentifier
    {
        /// <summary>
        /// The bidictionary of ids to decider types
        /// </summary>
        protected static BiDictionary<short, Type> IdsToDeciderTypes;

        /// <summary>
        /// The constructor params tos earch for
        /// </summary>
        protected static Type[] ConstructorParams;

        static TransferResultDeciderIdentifier()
        {
            ConstructorParams = new[] { typeof(NetIncomingMessage) };

            IdsToDeciderTypes = new BiDictionary<short, Type>();

            Register(typeof(FromInventoryResultDecider), 1);
            Register(typeof(ToInventoryResultDecider), 2);
        }

        /// <summary>
        /// Registers a new transfer result decider
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public static void Register(Type type, short id)
        {
            IdsToDeciderTypes.Add(id, type);
        }

        /// <summary>
        /// Gets the id that identifies the specified type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns></returns>
        public static short GetIDOfType(Type type)
        {
            return IdsToDeciderTypes[type];
        }

        /// <summary>
        /// Gets the type that corresponds with the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The type</returns>
        public static Type GetTypeOfID(short id)
        {
            return IdsToDeciderTypes[id];
        }

        /// <summary>
        /// Creates a new transfer result decider of the specified type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="message">The message</param>
        /// <returns>The transfer result decider</returns>
        public static ITransferResultDecider Init(Type type, NetIncomingMessage message)
        {
            var constr = type.GetConstructor(ConstructorParams);

            return (ITransferResultDecider)constr.Invoke(new[] { message });
        }
    }
}
