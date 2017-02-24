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
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// Draws hover text on the screen as appropriate
    /// </summary>
    public class HoverTextComponent : MyGameComponent
    {
        private Point HoverTextMouseLoc;
        private string HoverText;

        private Texture2D BackgroundTexture;

        public HoverTextComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { new Color(Color.Black, 0.8f) });
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 1);
        }
        
        public override void Draw(RenderContext context)
        {
            if (HoverText == null)
                return;

            var screenSize = context.GraphicsDevice.Viewport;
            var expandRight = HoverTextMouseLoc.X < (screenSize.Width / 2);
            var expandDown = HoverTextMouseLoc.Y < (screenSize.Height / 2);

            var font = context.DefaultFont;

            var textSize = font.MeasureString(HoverText);
            var leftOffset = expandRight ? 16 : -textSize.X - 5;
            var topOffset = expandDown ? 5 : -textSize.Y - 5;


            var displayRect = new Rectangle((int)(HoverTextMouseLoc.X + leftOffset - 5), (int)(HoverTextMouseLoc.Y + topOffset - 5), (int)(textSize.X + 10), (int)(textSize.Y + 10));

            SpriteBatch.Draw(BackgroundTexture, destinationRectangle: displayRect);
            SpriteBatch.DrawString(font, HoverText, new Vector2(displayRect.X + 5, displayRect.Y + 5), Color.White);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext context, int timeMS)
        {
            HoverText = null;
            if (localGameState.HoveredEntity != null)
            {
                var ent = localGameState.HoveredEntity;

                if (ent.HoverText != null)
                {
                    HoverText = ent.HoverText;
                    var mouseState = Mouse.GetState();
                    HoverTextMouseLoc = mouseState.Position;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            BackgroundTexture.Dispose();
            BackgroundTexture = null;
        }
    }
}
