using BaseBuilder.Engine.Logic.Orders;
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
        public const int READY_FOR_SYNC_PACKET_ID = 104;
        public const int PLAYER_JOINED_PACKET_ID = 105;

        public const int TASK_ORDER_PACKET_ID = 201;
        public const int CANCEL_TASKS_PACKET_ID = 202;
        public const int ISSUE_MESSAGE_PACKET_ID = 203;
        public const int CHANGE_NAME_PACKET_ID = 204;
        public const int BUILD_ORDER_PACKET_ID = 205;
        public const int DECONSTRUCT_ORDER_PACKET_ID = 206;
        public const int REPLACE_TASKS_PACKET_ID = 207;
        public const int TOGGLE_PAUSED_PACKET_ID = 208;

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
            RegisterPacketType(READY_FOR_SYNC_PACKET_ID,   typeof(ReadyForSyncPacket));
            RegisterPacketType(PLAYER_JOINED_PACKET_ID,    typeof(PlayerJoinedPacket));

            RegisterPacketType(TASK_ORDER_PACKET_ID,           typeof(IssueTaskOrder));
            RegisterPacketType(CANCEL_TASKS_PACKET_ID,         typeof(CancelTasksOrder));
            RegisterPacketType(ISSUE_MESSAGE_PACKET_ID,        typeof(IssueMessageOrder));
            RegisterPacketType(CHANGE_NAME_PACKET_ID,          typeof(ChangeNameOrder));
            RegisterPacketType(BUILD_ORDER_PACKET_ID,          typeof(BuildOrder));
            RegisterPacketType(DECONSTRUCT_ORDER_PACKET_ID,    typeof(DeconstructOrder));
            RegisterPacketType(REPLACE_TASKS_PACKET_ID,        typeof(ReplaceTasksOrder));
            RegisterPacketType(TOGGLE_PAUSED_PACKET_ID,        typeof(TogglePausedTasksOrder));
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
