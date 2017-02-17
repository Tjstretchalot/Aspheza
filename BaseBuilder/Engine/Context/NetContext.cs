using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Context
{
    public class NetContext
    {
        public List<GamePacketPool> GamePacketPools;
        public Dictionary<Type, int> PacketTypesToIndexInGamePacketPools;
        public Dictionary<int, int> PacketIdsToIndexInGamePacketPools;

        public NetContext()
        {
            GamePacketPools = new List<GamePacketPool>();
            PacketTypesToIndexInGamePacketPools = new Dictionary<Type, int>();
            PacketIdsToIndexInGamePacketPools = new Dictionary<int, int>();
        }

        public void RegisterPacketType(int packetId, Type packetType)
        {
            GamePacketPools.Add(new GamePacketPool(packetId, packetType));
            PacketTypesToIndexInGamePacketPools.Add(packetType, GamePacketPools.Count - 1);
            PacketIdsToIndexInGamePacketPools.Add(packetId, GamePacketPools.Count - 1);
        }

        public void RegisterPackets()
        {
            RegisterPacketType(1, typeof(WorldDownloadPacket));

            RegisterPacketType(101, typeof(SyncStartPacket));
            RegisterPacketType(102, typeof(SyncPacket));
            RegisterPacketType(103, typeof(SimulationStartPacket));
        }

        public GamePacketPool GetPoolFromPacketType(Type type)
        {
            return GamePacketPools[PacketTypesToIndexInGamePacketPools[type]];
        }

        public GamePacketPool GetPoolFromPacketID(int id)
        {
            return GamePacketPools[PacketIdsToIndexInGamePacketPools[id]];
        }
    }
}
