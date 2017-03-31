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
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.WorldObject.Entities;

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

        private List<T> ReadEntityArray<T>(NetContext context, SharedGameState gameState, NetIncomingMessage message, Action<T> action = null, bool getResult=true) where T:Entity
        {
            int size = message.ReadInt32();
            List<T> result = null;
            if (getResult)
                result = new List<T>(size);
            for (int i = 0; i < size; i++)
            {
                var typeID = message.ReadInt16();
                var entity = (T)EntityIdentifier.InitEntity(EntityIdentifier.GetTypeOfID(typeID), gameState, message);
                if(getResult)
                    result.Add(entity);

                action?.Invoke(entity);
            }
            return result;
        }

        private void WriteEntityArray<T>(List<T> arr, NetContext context, SharedGameState gameState, NetOutgoingMessage message) where T:Entity
        {
            message.Write(arr.Count);
            foreach(var ent in arr)
            {
                message.Write(EntityIdentifier.GetIDOfEntity(ent.GetType()));
                ent.Write(message);
            }
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

                    var tile = TileIdentifier.InitTile(TileIdentifier.GetTypeOfID(tileType), pos, message);
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

            ReadEntityArray<MobileEntity>(context, gameState, message, (me) => SharedState.World.AddMobileEntity(me), false);
            ReadEntityArray<ImmobileEntity>(context, gameState, message, (im) => SharedState.World.AddImmobileEntity(im), false);
            ReadEntityArray<MobileEntity>(context, gameState, message, (me) => SharedState.World.AddMobileEntityImpl(me), false);
            ReadEntityArray<ImmobileEntity>(context, gameState, message, (im) => SharedState.World.AddImmobileEntityImpl(im), false);
            ReadEntityArray<MobileEntity>(context, gameState, message, (me) => SharedState.World.RemoveMobileEntity(me), false);
            ReadEntityArray<ImmobileEntity>(context, gameState, message, (im) => SharedState.World.RemoveImmobileEntity(im), false);

            SharedState.EntityIDCounter = message.ReadInt32();

            var numRecentMessages = message.ReadInt32();
            for (var i = 0; i < numRecentMessages; i++)
            {
                var msg = message.ReadString();
                var time = message.ReadInt32();

                SharedState.RecentMessages.Add(Tuple.Create(msg, time));
            }

            SharedState.Resources = new MaterialManager(message);

            var numReserved = message.ReadInt32();

            for(var i = 0; i < numReserved; i++)
            {
                SharedState.Reserved.Add(new PointI2D(message));
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
                    tile.Write(message);
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

            WriteEntityArray(SharedState.World.MobileEntitiesQueuedForAddition, context, gameState, message);
            WriteEntityArray(SharedState.World.ImmobileEntitiesQueuedForAddition, context, gameState, message);
            WriteEntityArray(SharedState.World.MobileEntities, context, gameState, message);
            WriteEntityArray(SharedState.World.ImmobileEntities, context, gameState, message);
            WriteEntityArray(SharedState.World.MobileEntitiesQueuedForRemoval, context, gameState, message);
            WriteEntityArray(SharedState.World.ImmobileEntitiesQueuedForRemoval, context, gameState, message);

            message.Write(SharedState.EntityIDCounter);

            message.Write(SharedState.RecentMessages.Count);

            foreach(var msgTuple in SharedState.RecentMessages)
            {
                message.Write(msgTuple.Item1);
                message.Write(msgTuple.Item2);
            }

            SharedState.Resources.Write(message);

            message.Write(SharedState.Reserved.Count);

            foreach(var res in SharedState.Reserved)
            {
                res.Write(message);
            }
        }

        public override void Clear()
        {
            SharedState = null;
            LocalPlayerID = -1;
        }
    }
}
