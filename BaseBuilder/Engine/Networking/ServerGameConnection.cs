using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using Lidgren.Network;

namespace BaseBuilder.Engine.Networking
{
    public class ServerGameConnection : GameConnection
    {
        protected NetServer Server;

        protected int Port;

        public ServerGameConnection(int port)
        {
            Port = port;
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
    }
}
