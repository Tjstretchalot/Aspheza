using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;
using BaseBuilder.Engine.World.Tiles;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Engine.Networking.Packets
{
    /// <summary>
    /// This packet describes the tiles in a world, but not the entities
    /// or environments. The entities and environments would be sent 
    /// seperately.
    /// </summary>
    public class WorldDownloadPacket : GamePacket
    {
        public int WorldWidth;
        public int WorldHeight;
        public List<Tile> World;

        public WorldDownloadPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void LoadFrom(UpdateContext context, NetPeer peer, NetIncomingMessage message)
        {
            WorldWidth = message.ReadInt32();
            WorldHeight = message.ReadInt32();

            if (World != null)
                World.Clear();
            else
                World = new List<Tile>(WorldWidth * WorldHeight);

            for(var y = 0; y < WorldHeight; y++)
            {
                for(var x = 0; x < WorldWidth; x++)
                {
                    var pos = new PointI2D(x, y);
                    var tileType = message.ReadInt16();

                    var tile = TileIdentifier.InitTile(TileIdentifier.GetTypeOfID(tileType), pos);
                    World.Add(tile);
                }
            }
        }

        public override void SaveTo(UpdateContext context, NetPeer peer, NetOutgoingMessage message)
        {
            message.Write(WorldWidth);
            message.Write(WorldHeight);

            var counter = 0;
            for(var y = 0; y < WorldHeight; y++)
            {
                for(var x = 0; x < WorldWidth; x++)
                {
                    var tile = World[counter++];

                    message.Write(TileIdentifier.GetIDOfTile(tile.GetType()));
                }
            }
        }

        public override void Clear()
        {
            WorldWidth = 0;
            WorldHeight = 0;
            World = null;
        }
    }
}
