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

namespace BaseBuilder.Engine.Networking
{
    public class ServerGameConnection : GameConnection
    {
        protected LocalGameState LocalState;
        protected SharedGameState SharedState;
        protected SharedGameLogic SharedLogic;

        protected NetServer Server;

        protected int Port;

        public ServerGameConnection(LocalGameState localState, SharedGameState sharedState, SharedGameLogic sharedLogic, int port)
        {
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

        public override IEnumerable<IGamePacket> ReadIncomingPackets(UpdateContext context)
        {
            return ReadIncomingPackets(context, Server);
        }

        public override void SendPacket(UpdateContext context, IGamePacket packet)
        {
            SendPacket(context, packet, Server, NetDeliveryMethod.ReliableOrdered);
        }

        public override void ConsiderGameUpdate()
        {
            
        }
    }
}
