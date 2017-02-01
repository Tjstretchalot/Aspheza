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

namespace BaseBuilder.Engine.World.Tiles
{
    public class SpriteTile : Tile
    {
        protected string SpriteName;

        public SpriteTile(PointD2D position, RectangleD2D collisionMesh, string spriteName) : base(position, collisionMesh)
        {
            SpriteName = spriteName;
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            context.SpriteBatch.Draw(texture,
                new Rectangle(
                    (int)(screenTopLeft.X + CollisionMesh.Left * CameraZoom.SCREEN_OVER_WORLD), (int)(screenTopLeft.Y + CollisionMesh.Top * CameraZoom.SCREEN_OVER_WORLD), 
                    (int)(CollisionMesh.Width * CameraZoom.SCREEN_OVER_WORLD), (int)(CollisionMesh.Height * CameraZoom.SCREEN_OVER_WORLD)
                    ), Color.White);
        }
    }
}
