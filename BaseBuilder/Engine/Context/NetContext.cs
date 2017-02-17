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
        public const int WORLD_DOWNLOAD_PACKET_ID = 1;
        public const int SYNC_START_PACKET_ID = 101;
        public const int SYNC_PACKET_ID = 102;
        public const int SIMULATION_START_PACKET_ID = 103;

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
            RegisterPacketType(WORLD_DOWNLOAD_PACKET_ID,   typeof(SharedGameStateDownloadPacket));

            RegisterPacketType(SYNC_START_PACKET_ID,       typeof(SyncStartPacket));
            RegisterPacketType(SYNC_PACKET_ID,             typeof(SyncPacket));
            RegisterPacketType(SIMULATION_START_PACKET_ID, typeof(SimulationStartPacket));
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
