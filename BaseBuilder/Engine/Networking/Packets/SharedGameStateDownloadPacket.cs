using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.World.Tiles;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Logic.Players;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;

namespace BaseBuilder.Engine.Networking.Packets
{
    /// <summary>
    /// This packet describes the tiles in a world, but not the entities
    /// or environments. The entities and environments would be sent 
    /// seperately.
    /// </summary>
    public class SharedGameStateDownloadPacket : GamePacket
    {
        public SharedGameState SharedState;
        public int LocalPlayerID;

        public SharedGameStateDownloadPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            LocalPlayerID = message.ReadInt32();

            var worldWidth = message.ReadInt32();
            var worldHeight = message.ReadInt32();
            var tiles = new List<Tile>(worldWidth * worldHeight);

            for(var y = 0; y < worldHeight; y++)
            {
                for(var x = 0; x < worldWidth; x++)
                {
                    var pos = new PointI2D(x, y);
                    var tileType = message.ReadInt16();

                    var tile = TileIdentifier.InitTile(TileIdentifier.GetTypeOfID(tileType), pos);
                    tiles.Add(tile);
                }
            }

            var numPlayers = message.ReadInt16();
            var players = new List<Player>();

            for(var playerNum = 0; playerNum < numPlayers; playerNum++)
            {
                var name = message.ReadString();
                var id = message.ReadInt32();

                var player = new Player(id, name);
                players.Add(player);
            }

            var gameTimeMS = message.ReadInt32();

            SharedState = new SharedGameState(new World.TileWorld(worldWidth, worldHeight, tiles), players, gameTimeMS);

            var numMobileEntities = message.ReadInt32();

            for(var i = 0; i < numMobileEntities; i++)
            {
                var entID = message.ReadInt16();

                SharedState.World.AddMobileEntity((MobileEntity)EntityIdentifier.InitEntity(EntityIdentifier.GetTypeOfID(entID), SharedState, message));
            }

            var numImmobileEntities = message.ReadInt32();
            for (var i = 0; i < numImmobileEntities; i++)
            {
                var entID = message.ReadInt16();

                SharedState.World.AddImmobileEntity((ImmobileEntity)EntityIdentifier.InitEntity(EntityIdentifier.GetTypeOfID(entID), SharedState, message));
            }

            var numRecentMessages = message.ReadInt32();
            for (var i = 0; i < numRecentMessages; i++)
            {
                var msg = message.ReadString();
                var time = message.ReadInt32();

                SharedState.RecentMessages.Add(Tuple.Create(msg, time));
            }
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(LocalPlayerID);

            message.Write(SharedState.World.TileWidth);
            message.Write(SharedState.World.TileHeight);
            
            var counter = 0;
            for(var y = 0; y < SharedState.World.TileHeight; y++)
            {
                for(var x = 0; x < SharedState.World.TileWidth; x++)
                {
                    var tile = SharedState.World.Tiles[counter++];

                    message.Write(TileIdentifier.GetIDOfTile(tile.GetType()));
                }
            }

            message.Write((short)SharedState.Players.Count);

            foreach(var player in SharedState.Players)
            {
                message.Write(player.Name);
                message.Write(player.ID);

                // we don't write orders - this packet can only be sent during the Waiting period so there are no orders
            }

            message.Write(SharedState.GameTimeMS);

            message.Write(SharedState.World.MobileEntities.Count);

            foreach(var mobile in SharedState.World.MobileEntities)
            {
                message.Write(EntityIdentifier.GetIDOfEntity(mobile.GetType()));

                mobile.Write(message);
            }


            message.Write(SharedState.World.ImmobileEntities.Count);

            foreach (var immobile in SharedState.World.ImmobileEntities)
            {
                message.Write(EntityIdentifier.GetIDOfEntity(immobile.GetType()));

                immobile.Write(message);
            }

            message.Write(SharedState.RecentMessages.Count);

            foreach(var msgTuple in SharedState.RecentMessages)
            {
                message.Write(msgTuple.Item1);
                message.Write(msgTuple.Item2);
            }
        }

        public override void Clear()
        {
            SharedState = null;
            LocalPlayerID = -1;
        }
    }
}
