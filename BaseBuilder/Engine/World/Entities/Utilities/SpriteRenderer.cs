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

        public SpriteRenderer(string spriteName)
        {
            SpriteName = spriteName;
        }

        public void Render(RenderContext context, int x, int y, int w, int h, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            context.SpriteBatch.Draw(texture,
                new Rectangle(
                    (int)(x), (int)(y),
                    (int)(w * context.Camera.Zoom), (int)(h * context.Camera.Zoom)
                    ), overlay);
        }
    }
}
