﻿using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Outside of the Networking package, the engine can treat playing as the
    /// server and the client as identical, except when first establishing the 
    /// connection. This interface describes the core of a game connection that
    /// should be sufficient for normal gameplay.
    /// </summary>
    public interface IGameConnection
    {
        void SendPacket(UpdateContext context, IGamePacket packet);

        IEnumerable<IGamePacket> ReadIncomingPackets(UpdateContext context);
    }
}