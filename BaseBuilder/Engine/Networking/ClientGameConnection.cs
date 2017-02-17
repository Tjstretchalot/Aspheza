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

namespace BaseBuilder.Engine.Networking
{
    public class ClientGameConnection : GameConnection
    {
        public bool Connected { get; protected set; }

        private bool InitiatedConnection;

        protected NetClient Client;

        protected IPEndPoint Host;

        protected int? LocalPlayerID;

        public ClientGameConnection(IPEndPoint host) : base(null, null, null)
        {
            Host = host;
            Connected = false;
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


        }

        public void ContinueConnecting(Viewport screenSize)
        {
            if(!InitiatedConnection)
            {
                Connect();
                InitiatedConnection = true;
            }

            HandleIncomingMessages(Client);

            Connected = SharedState != null;

            if(Connected)
            {
                LocalState = new LocalGameState(new Camera(new PointD2D(0, 0), new RectangleD2D(screenSize.Width, screenSize.Height), 8), LocalPlayerID.Value);
                SharedLogic = new SharedGameLogic();
            }
        }

        [PacketHandler(typeof(SharedGameStateDownloadPacket))]
        public void OnSharedGameStateDownloadPacket(SharedGameStateDownloadPacket packet)
        {
            SharedState = packet.SharedState;
            LocalPlayerID = packet.LocalPlayerID;
        }
    }
}
