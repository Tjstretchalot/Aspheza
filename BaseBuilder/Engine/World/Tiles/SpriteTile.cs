using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Engine.World.Tiles
{
    public class SpriteTile : Tile
    {
        protected string SpriteName;

        public SpriteTile(string spriteName)
        {
            SpriteName = spriteName;
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            context.SpriteBatch.Draw(texture,
                new Rectangle(
                    (int)(screenTopLeft.X + CollisionMesh.Left), (int)(screenTopLeft.Y + CollisionMesh.Top), 
                    (int)(CollisionMesh.Width), (int)(CollisionMesh.Height)
                    ), Color.White);
        }
    }
}
