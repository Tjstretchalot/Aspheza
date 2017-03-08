using BaseBuilder.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// An item on the combo box
    /// </summary>
    public class ComboBoxItem
    {
        /// <summary>
        /// The minimum size for this combo box
        /// </summary>
        public Point MinSize;

        /// <summary>
        /// The size of this combo box item. Should be set by the combo box.
        /// </summary>
        public Point Size;

        /// <summary>
        /// If this combo box item is hovered on
        /// </summary>
        public bool Hovered;

        /// <summary>
        /// If we have a render target, this is it
        /// </summary>
        protected RenderTarget2D RenderTarget;

        /// <summary>
        /// Where we drew ourselves last draw
        /// </summary>
        protected Rectangle DrawRect;

        /// <summary>
        /// The font that is use
        /// </summary>
        protected SpriteFont Font;

        /// <summary>
        /// The name of the font that I use
        /// </summary>
        protected string FontName;

        /// <summary>
        /// The text that is on this combo box item
        /// </summary>
        protected string Text;

        protected Texture2D BackgroundTexture;

        /// <summary>
        /// Initialize this combo box item with a particular font and text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="text">Text</param>
        public ComboBoxItem(SpriteFont font, string text)
        {
            Font = font;
            Text = text;
            DrawRect = new Rectangle(0, 0, 1, 1);
        }

        /// <summary>
        /// Initialize this combo box item with a particular font name and text
        /// </summary>
        /// <param name="font">The name of the font</param>
        /// <param name="text">The text</param>
        public ComboBoxItem(string font, string text)
        {
            FontName = font;
            Text = text;
            DrawRect = new Rectangle(0, 0, 1, 1);
        }
        

        /// <summary>
        /// Calculates the minimum size for this combo box
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="graphics">The graphics</param>
        /// <param name="graphicsDevice">The graphcis device</param>
        public void Initialize(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            if (Font == null && FontName != null)
                Font = content.Load<SpriteFont>(FontName);

            var minSizeVec = Font.MeasureString(Text);
            MinSize = new Point((int)minSizeVec.X, (int)minSizeVec.Y);

            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { new Color(Color.Black, 0.8f) });
        }

        public void SkippingDraw()
        {
            Hovered = false;
            DrawRect = new Rectangle(-1, -1, 1, 1);
        }

        /// <summary>
        /// This needs to be called before the first call to render or minimum size functions and when the scroll bar changes.
        /// </summary>
        /// <param name="content">The content manager</param>
        /// <param name="graphics">The graphics</param>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="itemsVisibleRect">The rectangle where visible combo box items are</param>
        /// <param name="myYOffset">The y offset for the top of this combo box relative to the visible items rect</param>
        /// <param name="scrollYOffset">The y offset caused by the scroll bar</param>
        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, Rectangle itemsVisibleRect, int myYOffset, int scrollYOffset)
        {
            // there are 4 cases that could be happening.

            // Case 1: We're not on the screen (our bottom is above the visible rect or our top is below the visible rect)
            // Case 2: We're about to leave the screen (our top is above the visible rect but our bottom is inside the visible rect)
            // Case 3: We're about to enter the screen (our top is inside the visible rect but our bottom is below the visible rect)
            // Case 4: We're totally in the screen

            // In case 1 and 4 we don't need a render target, but we do need one in case 2 and 3.

            // No matter what, we don't need to create a new render target if we already have one

            if(RenderTarget != null && RenderTarget.IsContentLost)
            {
                RenderTarget.Dispose();
                RenderTarget = null;
            }

            if (RenderTarget != null)
                return;

            var ourTop = itemsVisibleRect.Top + myYOffset + scrollYOffset;
            var ourBottom = ourTop + Size.Y;
            
            // Render targets actually look a little different than directly rendering on the 
            // screen. So we're going to just always draw from render targets. To see the effect,
            // uncomment these early returns and scroll a large textbox, the parts that came from
            // offscreen BEFORE you hover them will stay as render targets, whereas ones that have
            // never touched the edge of the screen / have been hovered on since then will be 
            // drawn directly.
            /*
            if (ourBottom <= itemsVisibleRect.Top)
                return; // Our bottom is above the visible top

            if (ourTop >= itemsVisibleRect.Bottom)
                return; // Our top is below the visible bottom

            if (ourTop >= itemsVisibleRect.Top && ourBottom <= itemsVisibleRect.Bottom)
                return; // our top is below the visible top and our bottom is above the visible bottom
            */
            

            RenderTarget = new RenderTarget2D(graphicsDevice, Size.X, Size.Y);

            graphicsDevice.SetRenderTarget(RenderTarget);

            graphicsDevice.Clear(Color.White);
            var spriteBatch = new SpriteBatch(graphicsDevice);

            spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

            DrawImpl(content, graphics, graphicsDevice, spriteBatch, 0, 0);

            spriteBatch.End();

            spriteBatch.Dispose();

            graphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Draw this combo box item. The parameters must be the same as those to the last PreDraw
        /// </summary>
        /// <param name="content"></param>
        /// <param name="graphics"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="itemsVisibleRect"></param>
        /// <param name="myYOffset"></param>
        /// <param name="scrollYOffset"></param>
        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle itemsVisibleRect, int myYOffset, int scrollYOffset)
        {
            if (RenderTarget != null && RenderTarget.IsContentLost)
            {
                RenderTarget.Dispose();
                RenderTarget = null;
            }

            if (itemsVisibleRect.Top + myYOffset + scrollYOffset + Size.Y <= itemsVisibleRect.Top)
            {
                SkippingDraw();
                return; // Our bottom is above the visible top
            }

            if (itemsVisibleRect.Top + myYOffset + scrollYOffset >= itemsVisibleRect.Bottom)
            {
                SkippingDraw();
                return; // Our top is below the visible bottom
            }

            // Always use a render target if it is available
            if (RenderTarget != null)
            {
                var sourceRect = new Rectangle(0, 0, Size.X, Size.Y);
                var destRect = new Rectangle(itemsVisibleRect.Left, itemsVisibleRect.Top + myYOffset + scrollYOffset, Size.X, Size.Y);

                if(destRect.Top < itemsVisibleRect.Top)
                {
                    // our top is above the visible top

                    var overflowAmt = itemsVisibleRect.Top - destRect.Top;

                    sourceRect.Y += overflowAmt;
                    sourceRect.Height -= overflowAmt;

                    destRect.Y += overflowAmt;
                    destRect.Height -= overflowAmt;
                }

                if(destRect.Bottom > itemsVisibleRect.Bottom)
                {
                    // our bottom is below the visible bottom

                    var overflowAmt = destRect.Bottom - itemsVisibleRect.Bottom;

                    sourceRect.Height -= overflowAmt;
                    destRect.Height -= overflowAmt;
                }

                DrawRect.X = destRect.X;
                DrawRect.Y = destRect.Y;
                DrawRect.Width = destRect.Width;
                DrawRect.Height = destRect.Height;

                spriteBatch.Draw(RenderTarget, destRect, sourceRect, Color.White);
                return;
            }

            // Otherwise just draw the whole thing
            DrawImpl(content, graphics, graphicsDevice, spriteBatch, itemsVisibleRect.Left, itemsVisibleRect.Top + myYOffset + scrollYOffset);
        }

        /// <summary>
        /// Draws this combo box directly onto the spritebatch, where the real top-left is at x and the real top-right is at y and the 
        /// real width and height is Size.
        /// </summary>
        /// <param name="content">The content manager</param>
        /// <param name="graphics">The graphcis</param>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="spriteBatch">The sprite batch</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void DrawImpl(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int x, int y)
        {
            DrawRect.X = x;
            DrawRect.Y = y;
            DrawRect.Width = Size.X;
            DrawRect.Height = Size.Y;

            spriteBatch.Draw(BackgroundTexture, new Rectangle(x, y, Size.X, Size.Y), Color.White);

            spriteBatch.DrawString(Font, Text, new Vector2(x + Size.X / 2 - MinSize.X / 2, y + Size.Y / 2 - MinSize.Y / 2), Hovered ? Color.White : Color.LightGray);
        }

        public void HandleMouseState(MouseState last, MouseState current, ref bool handled)
        {
            var newHovered = !handled && DrawRect.Contains(current.Position);

            if(newHovered != Hovered)
            {
                if(RenderTarget != null)
                {
                    RenderTarget.Dispose();
                    RenderTarget = null;
                }
            }

            Hovered = newHovered;
            handled = handled || Hovered;
        }

        public void HandleKeyboardState(KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void Update(int elapsedMS)
        {

        }

        public void Dispose()
        {
            RenderTarget.Dispose();
            RenderTarget = null;
        }
    }
}
