using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a simple button that can be used in a screen.
    /// </summary>
    public class Button : IScreenComponent
    {
        private string _Text;
        /// <summary>
        /// The text on this button.
        /// </summary>
        public string Text
        {
            get
            {
                return _Text;
            }

            set
            {
                _TextDestinationVec = null;
                _Text = value;
            }
        }

        internal Rectangle? _Location;

        public Rectangle Location
        {
            get
            {
                if(!_Location.HasValue)
                {
                    var sourceRect = GetSourceRect(Pressed, Hovered);

                    _Location = new Rectangle(
                        Center.X - (sourceRect.Width / 2),
                        Center.Y - (sourceRect.Height / 2),
                        sourceRect.Width,
                        sourceRect.Height
                    );
                }

                return _Location.Value;
            }
        }

        /// <summary>
        /// The name of the font used.
        /// </summary>
        public string FontName;

        /// <summary>
        /// The color for the text when unhovered and unpressed
        /// </summary>
        public Color UnhoveredUnpressedTextColor;

        /// <summary>
        /// The color for the text when hovered and unpressed
        /// </summary>
        public Color HoveredUnpressedTextColor;

        /// <summary>
        /// The color for the text when hovered and pressed
        /// </summary>
        public Color HoveredPressedTextColor;

        protected Point _Center;

        /// <summary>
        /// The center of this button.
        /// </summary>
        public Point Center
        {
            get
            {
                return _Center;
            }

            set
            {
                _Location = null;
                _Center = value;
            }
        }

        /// <summary>
        /// The size of this button right now.
        /// </summary>
        public Point Size
        {
            get
            {
                return Location.Size;
            }
        }

        /// <summary>
        /// If this button is pressed or not.
        /// </summary>
        public bool Pressed;

        /// <summary>
        /// If the mouse is hovering over this button.
        /// </summary>
        public bool Hovered;

        /// <summary>
        /// The name of the sprite that contains the graphics for this button when
        /// unhovered and unpressed
        /// </summary>
        public string UnhoveredUnpressedButtonSpriteName;

        /// <summary>
        /// The name of the sprite that contains the graphics for this button when
        /// hovered and unpressed
        /// </summary>
        public string HoveredUnpressedButtonSpriteName;

        /// <summary>
        /// The name of the sprite that contains the graphics for this button when
        /// hovered and pressed
        /// </summary>
        public string HoveredPressedButtonSpriteName;

        /// <summary>
        /// Where in the sprite for this button should be displayed when
        /// the mouse is not hovering over this button and the button is
        /// not pressed.
        /// </summary>
        public Rectangle UnhoveredUnpressedSourceRect;

        /// <summary>
        /// Where in the sprite for this button should be displayed when
        /// the mouse is not hovering over this button and the button is not
        /// pressed.
        /// </summary>
        public Rectangle HoveredUnpressedSourceRect;

        /// <summary>
        /// Where in the sprite for this button should be displayed when
        /// the mouse is hovering over this button and the button is 
        /// pressed.
        /// </summary>
        public Rectangle HoveredPressedSourceRect;

        /// <summary>
        /// What sound effect should be played when the mouse begins to 
        /// hover over this button.
        /// </summary>
        public string MouseEnterSFXName;

        /// <summary>
        /// What sound effect should be played when the mouse stops 
        /// hovering over this button.
        /// </summary>
        public string MouseLeaveSFXName;

        /// <summary>
        /// What sound effect should be played when the button changes
        /// from unpressed to pressed.
        /// </summary>
        public string PressedSFXName;

        /// <summary>
        /// What sound effect should be played when the button changes
        /// from pressed to unpressed
        /// </summary>
        public string UnpressedSFXName;
        
        /// <summary>
        /// Called when this button changes from pressed to unpressed while hovered.
        /// This is when you should treat the button as clicked.
        /// </summary>
        public event EventHandler OnPressReleased;

        /// <summary>
        /// Triggered when hover changes
        /// </summary>
        public event EventHandler OnHoveredChanged;

        /// <summary>
        /// Triggered when pressed changes
        /// </summary>
        public event EventHandler OnPressedChanged;
        
        internal Vector2? _TextDestinationVec;

        public Button(string text, string fontName, Color unhoveredUnpressedTextColor, Color hoveredUnpressedTextColor,
            Color hoveredPressedTextColor, Point center, string spriteNameUnhoveredUnpressed,
            string spriteNameHoveredUnpressed, string spriteNameHoveredPressed, Rectangle unhoveredUnpressedSource, 
            Rectangle hoveredUnpressedSource, Rectangle hoveredPressedSource, string mouseEnterSFX, string mouseLeaveSFX, 
            string pressedSFX, string unpressedSFX)
        {
            Text = text;
            FontName = fontName;
            UnhoveredUnpressedTextColor = unhoveredUnpressedTextColor;
            HoveredUnpressedTextColor = hoveredUnpressedTextColor;
            HoveredPressedTextColor = hoveredPressedTextColor;
            _Center = center;
            UnhoveredUnpressedButtonSpriteName = spriteNameUnhoveredUnpressed;
            HoveredUnpressedButtonSpriteName = spriteNameHoveredUnpressed;
            HoveredPressedButtonSpriteName = spriteNameHoveredPressed;
            UnhoveredUnpressedSourceRect = unhoveredUnpressedSource;
            HoveredUnpressedSourceRect = hoveredUnpressedSource;
            HoveredPressedSourceRect = hoveredPressedSource;
            MouseEnterSFXName = mouseEnterSFX;
            MouseLeaveSFXName = mouseLeaveSFX;
            PressedSFXName = pressedSFX;
            UnpressedSFXName = unpressedSFX;

            _Location = null;
            _TextDestinationVec = null;
        }

        protected Color GetTextColor(bool pressed, bool hovered)
        {
            if (pressed)
            {
                if (hovered)
                    return HoveredPressedTextColor;
                return UnhoveredUnpressedTextColor;
            }
            else
            {
                if (hovered)
                    return HoveredUnpressedTextColor;
                return UnhoveredUnpressedTextColor;
            }
        }

        protected Texture2D GetSourceTexture(ContentManager content, bool pressed, bool hovered)
        {
            if (pressed)
            {
                if (hovered)
                    return content.Load<Texture2D>(HoveredPressedButtonSpriteName);
                return content.Load<Texture2D>(UnhoveredUnpressedButtonSpriteName);
            }
            else
            {
                if (hovered)
                    return content.Load<Texture2D>(HoveredUnpressedButtonSpriteName);
                return content.Load<Texture2D>(UnhoveredUnpressedButtonSpriteName);
            }
        }

        protected Rectangle GetSourceRect(bool pressed, bool hovered)
        {
            if(pressed)
            {
                if (hovered)
                    return HoveredPressedSourceRect;
                return UnhoveredUnpressedSourceRect;
            }else
            {
                if (hovered)
                    return HoveredUnpressedSourceRect;
                return UnhoveredUnpressedSourceRect;
            }
        }

        /// <summary>
        /// Determines if the specified point is inside of this 
        /// button.
        /// </summary>
        /// <param name="pX">X coordinate.</param>
        /// <param name="pY">Y coordinate.</param>
        /// <returns></returns>
        public bool ContainsPoint(int pX, int pY)
        {
            return Location.Contains(pX, pY);
        }

        /// <summary>
        /// Updates the button.
        /// </summary>
        /// <param name="content">The content (for audio)</param>
        /// <param name="deltaMS">Time in milliseconds since the last call to update</param>
        public void Update(ContentManager content, int deltaMS)
        {
        }

        public bool HandleMouseState(ContentManager content, MouseState last, MouseState mouse)
        {
            var newHovered = ContainsPoint(mouse.Position.X, mouse.Position.Y);
            var newPressed = (newHovered && mouse.LeftButton == ButtonState.Pressed);

            var pressedChanged = Pressed != newPressed;
            var hoveredChanged = Hovered != newHovered;

            if (newHovered && !Hovered)
            {
                // Mouse entered
                content.Load<SoundEffect>(MouseEnterSFXName).Play();

            }
            else if (!newHovered && Hovered)
            {
                // Mouse left
                content.Load<SoundEffect>(MouseLeaveSFXName).Play();
            }

            if (newPressed && !Pressed)
            {
                // Mouse pressed
                content.Load<SoundEffect>(PressedSFXName).Play();
            }
            else if (!newPressed && Pressed)
            {
                // Mouse unpressed
                content.Load<SoundEffect>(UnpressedSFXName).Play();

                if (newHovered && Hovered)
                    OnPressReleased?.Invoke(this, EventArgs.Empty);
            }

            if (pressedChanged)
                OnPressedChanged?.Invoke(this, EventArgs.Empty);
            if (hoveredChanged)
                OnHoveredChanged?.Invoke(this, EventArgs.Empty);

            Pressed = newPressed;
            Hovered = newHovered;

            if (pressedChanged || hoveredChanged)
                _Location = null;

            return Hovered;
        }

        public bool HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current)
        {
            return false;
        }

        /// <summary>
        /// Draws this button onto the screen
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            var sourceText = GetSourceTexture(content, Pressed, Hovered);
            var sourceRect = GetSourceRect(Pressed, Hovered);
            var textColor = GetTextColor(Pressed, Hovered);
            
            if (Text != null)
            {
                var font = content.Load<SpriteFont>(FontName);
                if (!_TextDestinationVec.HasValue)
                {
                    var textSize = font.MeasureString(Text);

                    _TextDestinationVec = new Vector2(
                        (int)(Center.X - textSize.X / 2),
                        (int)(Center.Y - textSize.Y / 2)
                        );
                }

                spriteBatch.Draw(sourceText, Location, sourceRect, Color.White);
                spriteBatch.DrawString(font, Text, _TextDestinationVec.Value, textColor);
            }
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
        }

        public void Dispose()
        {
        }
    }
}
