﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;

namespace BaseBuilder.Engine.Networking
{
    public class ClientGameConnection : GameConnection
    {
        protected NetClient Client;

        protected IPEndPoint Host;

        public ClientGameConnection(IPEndPoint host)
        {
            Host = host;
        }

        public void Connect()
        {
            var cfg = new NetPeerConfiguration("basebuilder");

            Client = new NetClient(cfg);
            Client.Connect(Host);
        }

        public override IEnumerable<IGamePacket> ReadIncomingPackets(UpdateContext context)
        {
            return ReadIncomingPackets(context, Client);
        }

        public override void SendPacket(UpdateContext context, IGamePacket packet)
        {
            SendPacket(context, packet, Client, NetDeliveryMethod.ReliableOrdered);
        }

        public override void ConsiderGameUpdate()
        {

        }
    }
}
