﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Logic.Players;
using System.Diagnostics;

namespace BaseBuilder.Engine.Networking
{
    public class ServerGameConnection : GameConnection
    {
        protected NetServer Server;
        protected List<NetConnection> ConnectionsWaitingForDownload;

        /// <summary>
        /// Maps player ids to their index in Server.Connections
        /// </summary>
        protected Dictionary<int, long> PlayerIDsToNetConnectionUniqueIdentifier;

        protected int Port;

        protected int PlayerIDCounter;

        protected Stopwatch TickWatch;
        

        public ServerGameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic, int port) : base(localState, sharedState, sharedLogic)
        {
            ConnState = ConnectionState.Waiting;

            ConnectionsWaitingForDownload = new List<NetConnection>();

            Port = port;

            sharedState.GetPlayerByID(localState.LocalPlayerID).ReadyForSync = true;

            PlayerIDCounter = localState.LocalPlayerID + 1;

            PlayerIDsToNetConnectionUniqueIdentifier = new Dictionary<int, long>();
        }
        
        
        public void BeginListening()
        {
            var cfg = new NetPeerConfiguration("basebuilder");
            cfg.Port = Port;

            Server = new NetServer(cfg);
            Server.Start();
        }
        
        public override void SendPacket(IGamePacket packet)
        {
            SendPacket(packet, Server, NetDeliveryMethod.ReliableOrdered);
        }
        
        protected override void HandleMessage(NetPeer peer, NetIncomingMessage msg)
        {
            switch(msg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    switch(msg.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            OnNewConnection(msg.SenderConnection);
                            peer.Recycle(msg);
                            return;
                    }
                    break;
            }
            base.HandleMessage(peer, msg);
        }

        private void OnNewConnection(NetConnection conn)
        {
            ConnectionsWaitingForDownload.Add(conn);
        }

        private int GetUniquePlayerID()
        {
            return PlayerIDCounter++;
        }

        [PacketHandler(typeof(ReadyForSyncPacket))]
        public void OnPlayerReadyForSync(ReadyForSyncPacket packet)
        {
            Console.WriteLine($"Player {packet.PlayerID} has indicated he is ready to sync");
            SharedState.GetPlayerByID(packet.PlayerID).ReadyForSync = true;
        }

        [PacketHandler(typeof(SyncPacket))]
        public void EchoSyncPackets(SyncPacket packet)
        {
            HandleSyncPacket(packet);

            int ignorePlayerID = packet.PlayerID;
            
            var outgoing = Server.CreateMessage();

            outgoing.Write(Context.GetPoolFromPacketType(packet.GetType()).PacketIdentifier);
            packet.SaveTo(Context, SharedState, outgoing);
            var connsToSendTo = new List<NetConnection>();
            
            foreach (var conn in Server.Connections)
            {
                foreach(var player in SharedState.Players)
                {
                    if(player.ID != ignorePlayerID && player.ID != LocalState.LocalPlayerID)
                    {
                        var ruid = PlayerIDsToNetConnectionUniqueIdentifier[player.ID];

                        if(ruid == conn.RemoteUniqueIdentifier)
                        {
                            connsToSendTo.Add(conn);
                            break;
                        }
                    }
                }
            }

            SendPacket(packet, Server, connsToSendTo, NetDeliveryMethod.ReliableOrdered);
        }

        bool OkayToSync()
        {
            return true;
        }

        public override void ConsiderGameUpdate()
        {
            HandleIncomingMessages(Server);

            switch (ConnState)
            {
                case ConnectionState.Waiting:
                    /*
                     * While we're waiting as the server we want to decide if it's a good time to
                     * switch to syncing. We want to sync as often as possible, but no more often.
                     * 
                     * For right now, let's just try to sync whenever we're in the waiting state.
                     */

                    var waitingForPlayers = false;
                    if(ConnectionsWaitingForDownload.Count > 0) {
                        var conn = ConnectionsWaitingForDownload[0];
                        ConnectionsWaitingForDownload.RemoveAt(0);
                        waitingForPlayers = true;

                        var newPlayer = new Player(GetUniquePlayerID(), "not set");
                        PlayerIDsToNetConnectionUniqueIdentifier.Add(newPlayer.ID, conn.RemoteUniqueIdentifier);
                        SharedState.Players.Add(newPlayer);

                        var syncPacket = (SharedGameStateDownloadPacket)Context.GetPoolFromPacketType(typeof(SharedGameStateDownloadPacket)).GetGamePacketFromPool();
                        syncPacket.SharedState = SharedState;
                        syncPacket.LocalPlayerID = newPlayer.ID;
                        SendPacket(syncPacket, Server, conn, NetDeliveryMethod.ReliableOrdered);
                        syncPacket.Recycle();

                        var newPlayerPacket = (PlayerJoinedPacket)Context.GetPoolFromPacketType(typeof(PlayerJoinedPacket)).GetGamePacketFromPool();
                        newPlayerPacket.NewPlayer = newPlayer;
                        foreach(var player in SharedState.Players)
                        {
                            if (player.ID == newPlayer.ID || player.ID == LocalState.LocalPlayerID)
                                continue;
                            
                            player.ReadyForSync = false;
                        }

                        List<NetConnection> connectionsToSendTo = new List<NetConnection>(SharedState.Players.Count - 1);
                        foreach(var con in Server.Connections)
                        {
                            foreach(var player in SharedState.Players)
                            {
                                if (player.ID != newPlayer.ID && player.ID != LocalState.LocalPlayerID)
                                {
                                    var ruid = PlayerIDsToNetConnectionUniqueIdentifier[player.ID];

                                    if(con.RemoteUniqueIdentifier == ruid)
                                    {
                                        connectionsToSendTo.Add(con);
                                        break;
                                    }
                                }
                            }
                        }
                        SendPacket(newPlayerPacket, Server, connectionsToSendTo, NetDeliveryMethod.ReliableOrdered);
                        newPlayerPacket.Recycle();
                    }
                    
                    for(int i = 0; i < SharedState.Players.Count && !waitingForPlayers; i++)
                    {
                        waitingForPlayers = !SharedState.Players[i].ReadyForSync;
                    }

                    if (!waitingForPlayers && OkayToSync())
                    {
                        var syncStart = Context.GetPoolFromPacketType(typeof(SyncStartPacket)).GetGamePacketFromPool() as SyncStartPacket;
                        SendPacket(syncStart);
                        syncStart.Recycle();

                        OnSyncStart();
                    }
                    break;
                case ConnectionState.Syncing:
                    /*
                     * Once all players have sent a sync packet we want to begin simulating
                     */
                    var stillWaiting = false;
                    foreach(var pl in SharedState.Players)
                    {
                        if(!pl.OrdersRecieved)
                        {
                            stillWaiting = true;
                            break;
                        }
                    }

                    if (!stillWaiting)
                    {
                        int simulationTime;
                        if(TickWatch == null)
                        {
                            TickWatch = new Stopwatch();
                            TickWatch.Start();
                            simulationTime = 1;
                        }else
                        {
                            simulationTime = (int)TickWatch.ElapsedMilliseconds;
                            TickWatch.Restart();
                        }

                        var simulateStart = Context.GetPoolFromPacketType(typeof(SimulationStartPacket)).GetGamePacketFromPool() as SimulationStartPacket;
                        simulateStart.SimulationTime = simulationTime; 
                        SendPacket(simulateStart);
                        simulateStart.Recycle();
                        
                        OnSimulateStart(simulationTime);
                    }

                    break;
                case ConnectionState.Simulating:
                    /*
                     * For right now we shouldn't ever actually tick while in this state
                     */
                    break;
            }
        }
    }
}
