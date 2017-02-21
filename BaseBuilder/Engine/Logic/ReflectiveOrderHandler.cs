using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Logic.Players;
using BaseBuilder.Engine.State;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BaseBuilder.Engine.Logic
{
    public class ReflectiveOrderHandler
    {
        protected object Handler;
        protected Dictionary<Type, HashSet<MethodBase>> OrderTypesToMethodBases;

        public ReflectiveOrderHandler(object handler)
        {
            Handler = handler;

            OrderTypesToMethodBases = new Dictionary<Type, HashSet<MethodBase>>();

            DetectMethods();
        }

        protected void RegisterMethod(MethodBase method)
        {
            var orderHandlers = method.GetCustomAttributes<OrderHandler>();

            foreach (var packetHandler in orderHandlers)
            {
                HashSet<MethodBase> set;
                if (!OrderTypesToMethodBases.TryGetValue(packetHandler.OrderType, out set))
                {
                    set = new HashSet<MethodBase>();
                    set.Add(method);
                    OrderTypesToMethodBases.Add(packetHandler.OrderType, set);
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
        public void BroadcastOrder(SharedGameState gameState, Player player, IOrder order)
        {
            HashSet<MethodBase> methods;
            if (OrderTypesToMethodBases.TryGetValue(order.GetType(), out methods))
            {
                foreach (var method in methods)
                {
                    method.Invoke(Handler, new object[] { gameState, player, order });
                }
            }
        }
    }
}