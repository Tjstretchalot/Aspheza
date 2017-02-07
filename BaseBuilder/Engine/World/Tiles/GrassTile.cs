using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Tiles
{
    public class GrassTile : SpriteTile
    {
        private static short ID = 2;
        private static Rectangle sourceRect;

        static GrassTile()
        {
            sourceRect = new Rectangle(0, 32, 32, 32);
            TileIdentifier.Register(typeof(GrassTile), ID);
        }
        
        public GrassTile(PointI2D position, RectangleD2D collisionMesh) : base(position, collisionMesh, "TileSet", sourceRect)
        {
        }
    }
}
