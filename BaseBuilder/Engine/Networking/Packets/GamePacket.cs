using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.Networking.Packets
{
    public abstract class GamePacket : IGamePacket
    {
        public abstract void Clear();
        public abstract void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message);
        public abstract void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message);

        protected GamePacketPool Pool;

        protected GamePacket(GamePacketPool pool)
        {
            Pool = pool;
        }

        public void Recycle()
        {
            Pool.Recycle(this);
        }
    }
}
