using System;
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
        protected ConnectionState ConnState;
        protected LocalGameState LocalState;
        protected SharedGameState SharedState;
        protected SharedGameLogic SharedLogic;

        protected NetServer Server;

        protected int Port;
        

        public ServerGameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic, int port)
        {
            ConnState = ConnectionState.Waiting;

            Port = port;
            LocalState = localState;
            SharedState = sharedState;
            SharedLogic = sharedLogic;
        }
        

        public void BeginListening()
        {
            var cfg = new NetPeerConfiguration("basebuilder");
            cfg.Port = Port;

            Server = new NetServer(cfg);
            Server.Start();
        }

        public override IEnumerable<IGamePacket> ReadIncomingPackets()
        {
            return ReadIncomingPackets(Server);
        }

        public override void SendPacket(IGamePacket packet)
        {
            SendPacket(packet, Server, NetDeliveryMethod.ReliableOrdered);
        }

        public override void ConsiderGameUpdate()
        {
            switch(ConnState)
            {
                case ConnectionState.Waiting:
                    /*
                     * While we're waiting as the server we want to decide if it's a good time to
                     * switch to syncing. We want to sync as often as possible, but no more often.
                     * 
                     * For right now, let's just try to sync whenever we're in the waiting state.
                     */
                    var syncStart = Context.GetPoolFromPacketType(typeof(SyncStartPacket)).GetGamePacketFromPool() as SyncStartPacket;
                    SendPacket(syncStart);
                    syncStart.Recycle();

                    OnSyncStart();
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

        /// <summary>
        /// This function handles what we, being a player, have to do to get ready to start syncing.
        /// </summary>
        protected void OnSyncStart()
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
        protected void OnSimulateStart(int timeMS)
        {
            ConnState = ConnectionState.Simulating;

            SharedLogic.SimulateTimePassing(SharedState, timeMS);

            ConnState = ConnectionState.Waiting;
        }
    }
}
