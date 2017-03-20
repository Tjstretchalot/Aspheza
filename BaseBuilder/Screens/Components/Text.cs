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
    /// A screen component that just draws some text with a specified font and color.
    /// </summary>
    public class Text : IScreenComponent
    {
        /// <summary>
        /// If this requires high priority z right now
        /// </summary>
        public bool HighPriorityZ { get { return false; } }
    
        protected Point _Center;
        public Point Center
        {
            get
            {
                return _Center;
            }
            set
            {
                _TopLeft = null;
                _Center = value;
            }
        }

        protected Vector2? _TopLeft;
        public Vector2 TopLeft
        {
            get
            {
                if(!_TopLeft.HasValue)
                {
                    _TopLeft = new Vector2(Center.X - Size.X / 2, Center.Y - Size.Y / 2); 
                }

                return _TopLeft.Value;
            }
        }

        protected Point? _Size;
        public Point Size {
            get
            {
                if (!_Size.HasValue)
                {
                    var sizeVec = Font.MeasureString(Content);
                    _Size = new Point((int)sizeVec.X, (int)sizeVec.Y);
                }

                return _Size.Value;
            }
        }

        protected string _Content;
        public string Content
        {
            get
            {
                return _Content;
            }

            set
            {
                _TopLeft = null;
                _Size = null;
                _Content = value;
            }
        }

        protected SpriteFont _Font;
        public SpriteFont Font
        {
            get
            {
                return _Font;
            }

            set
            {
                _Size = null;
                _TopLeft = null;
                _Font = value;
            }
        }

        public Color TextColor;

        protected bool Cache;
        protected RenderTarget2D CacheTarget;


        /// <summary>
        /// Triggered when disposing
        /// </summary>
        public event EventHandler Disposing;

        public Text(Point center, string text, SpriteFont font, Color textColor, bool cache = false)
        {
            _Center = center;
            Content = text;
            _Font = font;
            TextColor = textColor;

            Cache = cache;
        }


        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            if(Cache)
            {
                bool redraw = false;
                if(CacheTarget == null || CacheTarget.Width != Size.X || CacheTarget.Height != Size.Y)
                {
                    CacheTarget?.Dispose();

                    CacheTarget = new RenderTarget2D(graphicsDevice, Size.X, Size.Y);
                    redraw = true;
                }

                redraw = redraw || CacheTarget.IsContentLost;

                if(redraw)
                {
                    graphicsDevice.SetRenderTarget(CacheTarget);

                    graphicsDevice.Clear(new Color(0, 0, 0, 0));

                    var spriteBatch = new SpriteBatch(graphicsDevice);
                    spriteBatch.Begin();

                    spriteBatch.DrawString(Font, Content, new Vector2(0, 0), TextColor);

                    spriteBatch.End();

                    graphicsDevice.SetRenderTarget(null);

                    spriteBatch.Dispose();
                }
            }
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (!Cache || CacheTarget == null || CacheTarget.IsContentLost)
            {
                spriteBatch.DrawString(Font, Content, TopLeft, TextColor);
            }
            else
            {
                spriteBatch.Draw(CacheTarget, new Rectangle((int)TopLeft.X, (int)TopLeft.Y, Size.X, Size.Y), TextColor);
            }
        }

        public void Update(ContentManager content, int deltaMS)
        {
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="last">The previous mouse state.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="handled">If the mouse has been handled.</param>
        /// <param name="scrollHandled">If the scroll wheel has been handled.</param>
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

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

            CacheTarget?.Dispose();
            CacheTarget = null;
        }
    }
}
