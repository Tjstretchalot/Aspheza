using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// Draws hover text on the screen as appropriate
    /// </summary>
    public class HoverTextComponent : MyGameComponent
    {
        private PointD2D HoverTextMouseLoc;
        private string HoverText;

        private Texture2D BackgroundTexture;

        public HoverTextComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { new Color(Color.Black, 0.8f) });
        }
        
        public override void Draw(RenderContext context)
        {
            if (HoverText == null)
                return;

            var screenSize = context.GraphicsDevice.Viewport;
            var expandRight = HoverTextMouseLoc.X < (screenSize.Width / 2);
            var expandDown = HoverTextMouseLoc.Y < (screenSize.Height / 2);

            var font = context.DebugFont;

            var textSize = font.MeasureString(HoverText);
            var leftOffset = expandRight ? 0 : -textSize.X;
            var topOffset = expandDown ? 0 : -textSize.Y;


            var displayRect = new Rectangle((int)(HoverTextMouseLoc.X + leftOffset - 5), (int)(HoverTextMouseLoc.Y + topOffset - 5), (int)(textSize.X + 10), (int)(textSize.Y + 10));

            SpriteBatch.Draw(BackgroundTexture, destinationRectangle: displayRect);
            SpriteBatch.DrawString(font, HoverText, new Vector2(displayRect.X + 5, displayRect.Y + 5), Color.White);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, int timeMS)
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();

            BackgroundTexture.Dispose();
            BackgroundTexture = null;
        }
    }
}
