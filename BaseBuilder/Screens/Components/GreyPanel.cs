using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// This class simply draws a grey panel of a certain size. The grey
    /// panel should be used as the background for UI elements, which is itself
    /// on a white background or the game background.
    /// </summary>
    public class GreyPanel : IResizableComponent
    {
        private const int SAMPLES_SQRT = 3; // this number must be odd and increases the number of operations quadratically
        private const int RADIUS = 15 * SAMPLES_SQRT;
        private const int INNER_BORDER = 4 * SAMPLES_SQRT;
        private const int OUTER_BORDER = 2 * SAMPLES_SQRT;

        /// <summary>
        /// If this requires high priority z right now
        /// </summary>
        public bool HighPriorityZ { get { return false; } }

        /// <summary>
        /// The actual location
        /// </summary>
        protected Rectangle _Location;

        /// <summary>
        /// Where the grey panel is located
        /// </summary>
        protected Rectangle Location
        {
            get
            {
                return _Location;
            }
            set
            {
                if (value.Width != _Location.Width || value.Height != _Location.Height)
                {
                    Texture?.Dispose();
                    Texture = null;
                }

                _Location = value;
            }
        }

        /// <summary>
        /// The center of this panel.
        /// </summary>
        public Point Center
        {
            get
            {
                return _Location.Center;
            }

            set
            {
                _Location.X = value.X - Size.X / 2;
                _Location.Y = value.Y - Size.Y / 2;
            }
        }

        /// <summary>
        /// The size of this panel.
        /// </summary>
        public Point Size
        {
            get
            {
                return _Location.Size;
            }
        }

        public Point MinSize
        {
            get { return Point.Zero; }
        }

        public Point MaxSize
        {
            get { return UIUtils.MaxPoint; }
        }
        
        /// <summary>
        /// The texture. 
        /// </summary>
        protected Texture2D Texture;

        /// <summary>
        /// Triggered when this panel is disposing itself
        /// </summary>
        public event EventHandler Disposing;

        /// <summary>
        /// Sets up a grey panel at the specified location
        /// </summary>
        /// <param name="location">The location of the grey panel</param>
        public GreyPanel(Rectangle location)
        {
            _Location = location;
        }

        /// <summary>
        /// Resizes this panel to the specified size.
        /// </summary>
        /// <param name="newSize">The new size of this panel</param>
        public void Resize(Point newSize)
        {
            Location = new Rectangle(Center.X - newSize.X / 2, Center.Y - newSize.Y / 2, newSize.X, newSize.Y);
        }

        /// <summary>
        /// Draws the grey panel
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (Texture == null)
                InitTexture(content, graphics, graphicsDevice, spriteBatch);

            spriteBatch.Draw(Texture, destinationRectangle: Location);
        }

        public void Update(ContentManager content, int deltaMS)
        {
        }

        protected virtual void InitTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            // Create a rounded rectangle with a center color of (238, 238, 238)
            // a 2px wide border of white (255, 255, 255) and another border of (153, 153, 153) that's also 2px wide

            Texture = RoundedRectUtils.CreateRoundedRect(content, graphics, graphicsDevice, spriteBatch, Location.Width, Location.Height,
                new Color(238, 238, 238), new Color(153, 153, 153), Color.White, 15, 4, 2);
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="content"></param>
        /// <param name="last"></param>
        /// <param name="current"></param>
        /// <param name="handled"></param>
        /// <param name="scrollHandled"></param>
        public void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="content"></param>
        /// <param name="last"></param>
        /// <param name="current"></param>
        /// <param name="handled"></param>
        public void HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}
