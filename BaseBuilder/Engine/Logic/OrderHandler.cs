using System;

namespace BaseBuilder.Engine.Logic
{
    public class OrderHandler : Attribute
    {
        public Type OrderType;

        public OrderHandler(Type orderType)
        {
            OrderType = orderType;
        }
    }
}