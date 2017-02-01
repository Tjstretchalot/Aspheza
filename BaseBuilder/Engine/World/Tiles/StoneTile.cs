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
    public class StoneTile : SpriteTile
    {
        private static Rectangle sourceRect;
        static StoneTile()
        {
            sourceRect = new Rectangle(32, 0, 32, 32);
        }

        public StoneTile(PointI2D position, RectangleD2D collisionMesh) : base(position, collisionMesh, "TileSet", sourceRect)
        {
        }
    }
}
