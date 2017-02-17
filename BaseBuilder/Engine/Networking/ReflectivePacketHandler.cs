using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// This class will scan an object for methods with the attribute
    /// HandlesPacket with a specified packet type. It will then be able
    /// to call the appropriate functions when packets are received of
    /// the packet type that the corresponding method requests.
    /// </summary>
    public class ReflectivePacketHandler
    {
        protected object Handler;
        protected Dictionary<Type, HashSet<MethodBase>> PacketTypesToMethodBases;

        public ReflectivePacketHandler(object handler)
        {
            Handler = handler;

            PacketTypesToMethodBases = new Dictionary<Type, HashSet<MethodBase>>();

            DetectMethods();
        }

        protected void RegisterMethod(MethodBase method)
        {
            var packetHandlers = method.GetCustomAttributes<PacketHandler>();

            foreach (var packetHandler in packetHandlers)
            {
                HashSet<MethodBase> set;
                if (!PacketTypesToMethodBases.TryGetValue(packetHandler.PacketType, out set))
                {
                    set = new HashSet<MethodBase>();
                    set.Add(method);
                    PacketTypesToMethodBases.Add(packetHandler.PacketType, set);
                }
                else
                {
                    set.Add(method);
                }
            }
        }

        protected void DetectMethods()
        {
            var type = Handler.GetType();

            while (type != typeof(object))
            {
                foreach (var methodInfo in Handler.GetType().GetRuntimeMethods())
                {
                    RegisterMethod(methodInfo);
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Broadcasts the specified packet
        /// </summary>
        /// <param name="packet">the packet</param>
        public void BroadcastPacket(IGamePacket packet)
        {
            HashSet<MethodBase> methods;
            if(PacketTypesToMethodBases.TryGetValue(packet.GetType(), out methods))
            {
                foreach(var method in methods)
                {
                    method.Invoke(Handler, new[] { packet });
                }
            }
        }
    }
}
