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

namespace BaseBuilder.Engine.Networking
{
    public class ServerGameConnection : GameConnection
    {
        protected NetServer Server;
        protected List<NetConnection> ConnectionsWaitingForDownload;

        protected int Port;

        protected int PlayerIDCounter;
        

        public ServerGameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic, int port) : base(localState, sharedState, sharedLogic)
        {
            ConnState = ConnectionState.Waiting;

            ConnectionsWaitingForDownload = new List<NetConnection>();

            Port = port;

            sharedState.GetPlayerByID(localState.LocalPlayerID).ReadyForSync = true;

            PlayerIDCounter = localState.LocalPlayerID + 1;
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
            SharedState.Players[packet.PlayerID].ReadyForSync = true;
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
                    foreach (var conn in ConnectionsWaitingForDownload)
                    {
                        waitingForPlayers = true;
                        var syncPacket = (SharedGameStateDownloadPacket)Context.GetPoolFromPacketType(typeof(SharedGameStateDownloadPacket)).GetGamePacketFromPool();
                        syncPacket.SharedState = SharedState;
                        syncPacket.LocalPlayerID = GetUniquePlayerID();
                        SendPacket(syncPacket, Server, conn, NetDeliveryMethod.ReliableOrdered);
                        syncPacket.Recycle();
                    }

                    ConnectionsWaitingForDownload.Clear();

                    for(int i = 0; i < SharedState.Players.Count && !waitingForPlayers; i++)
                    {
                        waitingForPlayers = !SharedState.Players[i].ReadyForSync;
                    }

                    if (!waitingForPlayers)
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
                        var simulationTime = 16; // TODO don't use a constant here

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
