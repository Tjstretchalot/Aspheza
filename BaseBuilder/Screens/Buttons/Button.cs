using BaseBuilder.Engine.Math2D;
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

namespace BaseBuilder.Screens.Buttons
{
    /// <summary>
    /// Describes a simple button that can be used in a screen.
    /// </summary>
    public class Button
    {
        /// <summary>
        /// The text on this button.
        /// </summary>
        public string Text;

        /// <summary>
        /// The name of the font used.
        /// </summary>
        public string FontName;

        /// <summary>
        /// The center of this button.
        /// </summary>
        public PointI2D CenterPoint;

        /// <summary>
        /// If this button is pressed or not.
        /// </summary>
        public bool Pressed;

        /// <summary>
        /// If the mouse is hovering over this button.
        /// </summary>
        public bool Hovered;

        /// <summary>
        /// The name of the sprite that contains the graphics for this button.
        /// </summary>
        public string ButtonSpriteName;

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
        
        protected Rectangle? _DestinationRect;
        protected Vector2? _TextDestinationVec;

        public Button(string text, string fontName, PointI2D center, string spriteName, Rectangle unhoveredUnpressedSource, 
            Rectangle hoveredUnpressedSource, Rectangle hoveredPressedSource, string mouseEnterSFX, string mouseLeaveSFX, 
            string pressedSFX, string unpressedSFX)
        {
            Text = text;
            FontName = fontName;
            CenterPoint = center;
            ButtonSpriteName = spriteName;
            UnhoveredUnpressedSourceRect = unhoveredUnpressedSource;
            HoveredUnpressedSourceRect = hoveredUnpressedSource;
            HoveredPressedSourceRect = hoveredPressedSource;
            MouseEnterSFXName = mouseEnterSFX;
            MouseLeaveSFXName = mouseLeaveSFX;
            PressedSFXName = pressedSFX;
            UnpressedSFXName = unpressedSFX;

            _DestinationRect = null;
            _TextDestinationVec = null;
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
                else
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
            if (_DestinationRect.HasValue)
                return _DestinationRect.Value.Contains(pX, pY);

            var rect = GetSourceRect(Pressed, Hovered);

            return pX >= (CenterPoint.X - rect.Width / 2)
                && pX <= (CenterPoint.X + rect.Width / 2)
                && pY >= (CenterPoint.Y - rect.Height / 2)
                && pY <= (CenterPoint.Y + rect.Height / 2);
        }

        /// <summary>
        /// Returns true if Pressed changed during this update, returns
        /// false otherwise.
        /// </summary>
        /// <returns>
        /// True if Pressed changed during this update, false otherwise.
        /// </returns>
        /// <param name="content">The content (for audio)</param>
        public bool Update(ContentManager content)
        {
            var mouse = Mouse.GetState();
            var newHovered = ContainsPoint(mouse.Position.X, mouse.Position.Y);
            var newPressed = (newHovered && mouse.LeftButton == ButtonState.Pressed);

            var pressedChanged = Pressed != newPressed;
            var hoveredChanged = Hovered != newHovered;

            if(newHovered && !Hovered)
            {
                // Mouse entered
                content.Load<SoundEffect>(MouseEnterSFXName).Play();
            }else if(!newHovered && Hovered)
            {
                // Mouse left
                content.Load<SoundEffect>(MouseLeaveSFXName).Play();
            }

            if(newPressed && !Pressed)
            {
                // Mouse pressed
                content.Load<SoundEffect>(PressedSFXName).Play();
            }else if(!newPressed && Pressed)
            {
                // Mouse unpressed
                content.Load<SoundEffect>(UnpressedSFXName).Play();
            }

            Pressed = newPressed;
            Hovered = newHovered;

            if(pressedChanged || hoveredChanged)
                _DestinationRect = null;

            return pressedChanged;
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
            var sourceText = content.Load<Texture2D>(ButtonSpriteName);
            var sourceRect = GetSourceRect(Pressed, Hovered);

            if (!_DestinationRect.HasValue) {
                _DestinationRect = new Rectangle(
                    CenterPoint.X - (sourceRect.Width / 2),
                    CenterPoint.Y - (sourceRect.Height / 2),
                    sourceRect.Width,
                    sourceRect.Height
                    );
            }

            if (Text != null)
            {
                var font = content.Load<SpriteFont>(FontName);
                if (!_TextDestinationVec.HasValue)
                {
                    var textSize = font.MeasureString(Text);

                    _TextDestinationVec = new Vector2(
                        (int)(CenterPoint.X - textSize.X / 2),
                        (int)(CenterPoint.Y - textSize.Y / 2)
                        );
                }

                spriteBatch.Draw(sourceText, _DestinationRect.Value, sourceRect, Color.White);
                spriteBatch.DrawString(font, Text, _TextDestinationVec.Value, Color.Black);
            }
        }
    }
}
