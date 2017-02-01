using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Tiles
{
    public class StoneTile : SpriteTile
    {
        public StoneTile(PointD2D position, RectangleD2D collisionMesh) : base(position, collisionMesh, "tile_stone")
        {
        }
    }
}
