using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Describes a method that handles packets
    /// </summary>
    public class PacketHandler : Attribute
    {
        public Type PacketType;

        public PacketHandler(Type packetType)
        {
            PacketType = packetType;
        }
    }
}
