using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Engine.World.Tiles
{
    public class SpriteTile : Tile
    {
        protected string SpriteName;
        protected Rectangle SourceRect;

        private Rectangle drawRect;

        public SpriteTile(PointI2D position, RectangleD2D collisionMesh, string spriteName, Rectangle sourceRect) : base(position, collisionMesh)
        {
            SpriteName = spriteName;
            SourceRect = sourceRect;

            drawRect = new Rectangle();
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            drawRect.X = (int)(screenTopLeft.X + CollisionMesh.Left * context.Camera.Zoom);
            drawRect.Y = (int)(screenTopLeft.Y + CollisionMesh.Top * context.Camera.Zoom);
            drawRect.Width = (int)(CollisionMesh.Width * context.Camera.Zoom);
            drawRect.Height = (int)(CollisionMesh.Height * context.Camera.Zoom);

            context.SpriteBatch.Draw(texture, sourceRectangle: SourceRect, destinationRectangle: drawRect, color: overlay);
        }
    }
}
