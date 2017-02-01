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
        private static Rectangle sourceRectAllDirt;
        private static Rectangle sourceRectDirtUpGrassDown;
        private static Rectangle sourceRectDirtDownGrassUp;
        private static Rectangle sourceRectDirtRightGrassLeft;
        private static Rectangle sourceRectDirtLeftGrassRight;
        private static Rectangle sourceRectDirtUpRightGrassDownLeft;
        private static Rectangle sourceRectDirtUpLeftGrassDownRight;
        private static Rectangle sourceRectDirtDownRightGrassUpLeft;
        private static Rectangle sourceRectDirtDownLeftGrassUpRight;

        static DirtTile()
        {
            sourceRectAllDirt = new Rectangle(0, 0, 32, 32);
            sourceRectDirtUpGrassDown = new Rectangle(64, 0, 32, 32);
            sourceRectDirtDownGrassUp = new Rectangle(64 + 32, 0, 32, 32);
            sourceRectDirtRightGrassLeft = new Rectangle(64 + 32 * 2, 0, 32, 32);
            sourceRectDirtLeftGrassRight = new Rectangle(64 + 32 * 3, 0, 32, 32);
            sourceRectDirtUpRightGrassDownLeft = new Rectangle(64 + 32 * 4, 0, 32, 32);
            sourceRectDirtUpLeftGrassDownRight = new Rectangle(64 + 32 * 5, 0, 32, 32);
            sourceRectDirtDownRightGrassUpLeft = new Rectangle(64 + 32 * 6, 0, 32, 32);
            sourceRectDirtDownLeftGrassUpRight = new Rectangle(64 + 32 * 7, 0, 32, 32);
        }

        public DirtTile(PointI2D position, RectangleD2D collisionMesh) : base(position, collisionMesh, "TileSet", sourceRectAllDirt)
        {
        }

        public override void Loaded(UpdateContext context)
        {
            base.Loaded(context);

            
        }
    }
}
