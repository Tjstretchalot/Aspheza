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

        public Text(Point center, string text, SpriteFont font, Color textColor)
        {
            _Center = center;
            Content = text;
            _Font = font;
            TextColor = textColor;
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Content, TopLeft, TextColor);
        }

        public void Update(ContentManager content, int deltaMS)
        {
        }

        public void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled)
        {
        }

        public void HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
        }

        public void Dispose()
        {
        }
    }
}
