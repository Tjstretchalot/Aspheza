using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Engine.World.Tiles
{
    public class DirtTile : SpriteTile
    {
        private static short ID = 1;
        private static Rectangle sourceRect;

        static DirtTile()
        {
            sourceRect = new Rectangle(0, 0, 32, 32);
            TileIdentifier.Register(typeof(DirtTile), ID);
        }

        public DirtTile(PointI2D position, RectangleD2D collisionMesh) : base(position, collisionMesh, "TileSet", sourceRect)
        {
        }


        protected bool GrassAtRelative(UpdateContext context, int rx, int ry)
        {
            var tile = GetTileFromRelative(context, rx, ry);
            if (tile == null)
                return true;

            return GetType().IsAssignableFrom(tile.GetType());
        }
    }
}
