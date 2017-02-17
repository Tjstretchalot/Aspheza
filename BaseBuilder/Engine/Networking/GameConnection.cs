using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.State;
using System.Reflection;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Parent class for game connections for client and server.
    /// </summary>
    public abstract class GameConnection : IGameConnection
    {
        public ConnectionState ConnState;
        public LocalGameState LocalState;
        public SharedGameState SharedState;
        public SharedGameLogic SharedLogic;

        protected NetContext Context;

        protected Dictionary<Type, MethodBase> PacketTypesToMethodBases;

        protected ReflectivePacketHandler ReflectivePacketHandlerObj;

        public abstract void ConsiderGameUpdate();
        public abstract void SendPacket(IGamePacket packet);


        protected GameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic)
        {
            Context = new NetContext();
            ConnState = ConnectionState.Waiting;
            
            LocalState = localState;
            SharedState = sharedState;
            SharedLogic = sharedLogic;

            Context.RegisterPackets();

            ReflectivePacketHandlerObj = new ReflectivePacketHandler(this);
        }


        /// <summary>
        /// Reads incoming packets from the specified peer.
        /// </summary>
        /// <param name="peer">The peer</param>
        /// <returns>Any incoming packets.</returns>
        protected void HandleIncomingMessages(NetPeer peer)
        {
            NetIncomingMessage msg;

            while((msg = peer.ReadMessage()) != null)
            {
                HandleMessage(peer, msg);
            }
        }

        protected virtual void HandleMessage(NetPeer peer, NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    Console.WriteLine(msg.ReadString());
                    peer.Recycle(msg);
                    break;
                case NetIncomingMessageType.Data:
                    var id = msg.ReadInt32();
                    var pool = Context.GetPoolFromPacketID(id);
                    var packet = pool.GetGamePacket(Context, msg);
                    ReflectivePacketHandlerObj.BroadcastPacket(packet);
                    peer.Recycle(msg);
                    break;
                default:
                    Console.WriteLine("Unhandled type: " + msg.MessageType);
                    peer.Recycle(msg);
                    break;
            }
        }
        
        /// <summary>
        /// Sends the specified packet to the specified peer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="packet">The packet to send.</param>
        /// <param name="peer">The peer to send the packet to.</param>
        protected void SendPacket(IGamePacket packet, NetPeer peer, NetDeliveryMethod method)
        {
            var outgoing = peer.CreateMessage();

            outgoing.Write(Context.GetPoolFromPacketType(packet.GetType()).PacketIdentifier);
            packet.SaveTo(Context, outgoing);

            foreach (var conn in peer.Connections)
            {
                peer.SendMessage(outgoing, conn, method);
            }
        }

        [PacketHandler(packetType: typeof(SyncPacket))]
        public void HandleSyncPacket(SyncPacket packet)
        {
            var syncPacket = packet as SyncPacket;

            var player = SharedState.GetPlayerByID(syncPacket.PlayerID);

            if (ConnState != ConnectionState.Syncing)
                throw new InvalidProgramException($"Recieved sync packet from player id={player.ID}, name={player.Name} while state is {ConnState} (expected connection state Syncing)");

            if (player.OrdersRecieved)
                throw new InvalidProgramException($"Player id={player.ID}, name={player.Name} sent SyncPacket when OrdersRecieved=true!");


            player.CurrentOrders.AddRange(syncPacket.Orders);
            player.OrdersRecieved = true;
        }

        /// <summary>
        /// This function handles what we, being a player, have to do to get ready to start syncing.
        /// </summary>
        protected virtual void OnSyncStart()
        {
            ConnState = ConnectionState.Syncing;

            var syncPacket = Context.GetPoolFromPacketType(typeof(SyncPacket)).GetGamePacketFromPool() as SyncPacket;
            syncPacket.PlayerID = LocalState.LocalPlayerID;
            syncPacket.Orders.AddRange(LocalState.Orders);
            LocalState.Orders.Clear();
            var localPlayer = SharedState.GetPlayerByID(LocalState.LocalPlayerID);
            localPlayer.CurrentOrders.AddRange(syncPacket.Orders);
            localPlayer.OrdersRecieved = true;
            SendPacket(syncPacket);
            syncPacket.Recycle();
        }

        /// <summary>
        /// This function handles what we, being a player, have to do to 
        /// </summary>
        protected virtual void OnSimulateStart(int timeMS)
        {
            ConnState = ConnectionState.Simulating;

            SharedLogic.SimulateTimePassing(SharedState, timeMS);

            foreach(var pl in SharedState.Players)
            {
                foreach(var order in pl.CurrentOrders)
                {
                    order.Recycle();
                }

                pl.CurrentOrders.Clear();
                pl.OrdersRecieved = false;
            }

            ConnState = ConnectionState.Waiting;
        }
    }
}
