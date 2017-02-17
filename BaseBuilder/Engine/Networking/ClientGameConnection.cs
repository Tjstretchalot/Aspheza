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

namespace BaseBuilder.Engine.Networking
{
    public class ClientGameConnection : GameConnection
    {
        protected NetClient Client;

        protected IPEndPoint Host;

        public ClientGameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic, IPEndPoint host) : base(localState, sharedState, sharedLogic)
        {
            Host = host;
        }

        public void Connect()
        {
            var cfg = new NetPeerConfiguration("basebuilder");

            Client = new NetClient(cfg);
            Client.Connect(Host);
        }

        public override void SendPacket(IGamePacket packet)
        {
            SendPacket(packet, Client, NetDeliveryMethod.ReliableOrdered);
        }

        public override void ConsiderGameUpdate()
        {

        }
    }
}
