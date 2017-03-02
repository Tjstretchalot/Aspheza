using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Logic.Orders;

namespace BaseBuilder.Engine.Networking
{
    public class ClientGameConnection : GameConnection
    {
        public bool Connected { get; protected set; }

        private bool InitiatedConnection;

        protected NetClient Client;

        protected IPEndPoint Host;

        protected string DesiredName;

        protected int? LocalPlayerID;

        public ClientGameConnection(IPEndPoint host, string desiredName) : base(null, null, null)
        {
            Host = host;
            Connected = false;
            DesiredName = desiredName;
        }

        private void Connect()
        {
            var cfg = new NetPeerConfiguration("basebuilder");

            Client = new NetClient(cfg);
            Client.Start();
            Client.Connect(Host);
        }

        public override void SendPacket(IGamePacket packet)
        {
            SendPacket(packet, Client, NetDeliveryMethod.ReliableOrdered);
        }

        public override void ConsiderGameUpdate()
        {
            if (!Connected)
                throw new InvalidProgramException("Can't consider game update unless connected. You must can ContinueConnecting until Connected is true");


            HandleIncomingMessages(Client);
        }

        [PacketHandler(packetType: typeof(SimulationStartPacket))]
        public void OnSimulateStartRecieved(SimulationStartPacket packet)
        {
            if(Connected)
            {
                OnSimulateStart(packet.SimulationTime);
            }
        }
        
        [PacketHandler(packetType: typeof(SyncStartPacket))]
        public void OnSyncStartRecieved(SyncStartPacket packet)
        {
            if (Connected)
            {
                OnSyncStart();
            }
        }

        [PacketHandler(packetType: typeof(SyncPacket))]
        public void OnSyncReceieved(SyncPacket packet)
        {
            if(Connected)
            {
                HandleSyncPacket(packet);
            }
        }

        [PacketHandler(packetType: typeof(PlayerJoinedPacket))]
        public void OnPlayerJoined(PlayerJoinedPacket packet)
        {
            Console.WriteLine($"A player with id {packet.NewPlayer.ID} has connected to the game");
            if(!Connected)
            {
                throw new InvalidProgramException("This is a very fringe case that should be prevented on the server from happening");
            }

            var player = packet.NewPlayer;
            SharedState.Players.Add(player);
            
            var readyForSync = (ReadyForSyncPacket)Context.GetPoolFromPacketType(typeof(ReadyForSyncPacket)).GetGamePacketFromPool();
            readyForSync.PlayerID = LocalPlayerID.Value;
            SendPacket(readyForSync);
            readyForSync.Recycle();
        }

        public void ContinueConnecting(Viewport screenSize)
        {
            if (Connected)
                return;

            if(!InitiatedConnection)
            {
                Connect();
                InitiatedConnection = true;
            }

            HandleIncomingMessages(Client);

            Connected = SharedState != null;

            if (Connected)
            {
                LocalState = new LocalGameState(new Camera(new PointD2D(0, 0), new RectangleD2D(screenSize.Width, screenSize.Height), 8), LocalPlayerID.Value);
                SharedLogic = new SharedGameLogic();

                var imPool = Context.GetPoolFromPacketType(typeof(IssueMessageOrder));
                var imOrder = imPool.GetGamePacketFromPool() as IssueMessageOrder;
                imOrder.Message = $"Player {LocalPlayerID} has connected.";
                LocalState.Orders.Add(imOrder);

                var cnPool = Context.GetPoolFromPacketType(typeof(ChangeNameOrder));
                var cnOrder = cnPool.GetGamePacketFromPool() as ChangeNameOrder;
                cnOrder.PlayerID = LocalPlayerID.Value;
                cnOrder.NewName = DesiredName;
                LocalState.Orders.Add(cnOrder);

                imOrder = imPool.GetGamePacketFromPool() as IssueMessageOrder;
                imOrder.Message = $"Player {LocalPlayerID} has identified himself as {DesiredName}";
                LocalState.Orders.Add(imOrder);

            }
        }

        [PacketHandler(typeof(SharedGameStateDownloadPacket))]
        public void OnSharedGameStateDownloadPacket(SharedGameStateDownloadPacket packet)
        {
            SharedState = packet.SharedState;
            LocalPlayerID = packet.LocalPlayerID;
            
            SharedState.World.LoadingDone(SharedState);

            var readyForSync = (ReadyForSyncPacket)Context.GetPoolFromPacketType(typeof(ReadyForSyncPacket)).GetGamePacketFromPool();
            readyForSync.PlayerID = packet.LocalPlayerID;
            SendPacket(readyForSync);
            readyForSync.Recycle();
            Console.WriteLine($"Recieved OnSharedGameStateDownloadPacket (SharedState == null = {SharedState == null}; num players = {SharedState.Players.Count}; my id = {LocalPlayerID}");
        }
    }
}
