using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class SpriteRenderer 
    {
        protected string SpriteName;
        public Rectangle SourceRect;

        public SpriteRenderer(string spriteName, Rectangle sourceRect)
        {
            SpriteName = spriteName;
            SourceRect = sourceRect;
        }

        public void Render(RenderContext context, int x, int y, double w, double h, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            context.SpriteBatch.Draw(texture,
                sourceRectangle: SourceRect,
                destinationRectangle: new Rectangle(
                    (int)(x), (int)(y),
                    (int)(w * context.Camera.Zoom), (int)(h * context.Camera.Zoom)
                    ), color: overlay);
        }
    }
}
