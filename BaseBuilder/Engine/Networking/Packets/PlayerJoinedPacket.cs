using BaseBuilder.Engine.Logic.Players;
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
    public class PlayerJoinedPacket : GamePacket
    {
        public Player NewPlayer;

        public PlayerJoinedPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            NewPlayer = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            var id = message.ReadInt32();
            var name = message.ReadString();

            NewPlayer = new Player(id, name);
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(NewPlayer.ID);
            message.Write(NewPlayer.Name);
        }
    }
}
