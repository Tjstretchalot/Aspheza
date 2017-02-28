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
    public class SpriteSheetAnimationRenderer
    {
        string SpriteSheetName;
        Rectangle SourceRect;

        public SpriteSheetAnimationRenderer(string sheetName, Rectangle sourceRect)
        {
            SpriteSheetName = sheetName;
            SourceRect = sourceRect;
        }

        public void Render(RenderContext context, int x, int y, int w, int h, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteSheetName);

            context.SpriteBatch.Draw(texture,
                sourceRectangle: SourceRect,
                destinationRectangle: new Rectangle(
                    (int)(x), (int)(y),
                    (int)(w * context.Camera.Zoom), (int)(h * context.Camera.Zoom)
                    ),
                color: overlay);
        }
    }
}
