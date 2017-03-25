using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a one-line text field.
    /// </summary>
    public class TextField : IResizableComponent
    {
        /// <summary>
        /// If this requires high priority z right now
        /// </summary>
        public bool HighPriorityZ { get { return false; } }

        /// <summary>
        /// How long the caret is visible for during a blink cycle.
        /// </summary>
        private const int CARET_VISIBLE_TIME_MS = 500;

        /// <summary>
        /// How long the caret is hidden for during a blink cycle.
        /// </summary>
        private const int CARET_HIDDEN_TIME_MS = 750;

        private const int TEXT_X_OFFSET = 15;

        /// <summary>
        /// How long should a key press be ignored for after the initial press before
        /// repeating the press?
        /// </summary>
        private const int KEY_IGNORE_TIME_FIRST = 1000;

        /// <summary>
        /// How long should a key press be ignored for AFTER it's been held down since
        /// the initial ignore time?
        /// </summary>
        private const int KEY_IGNORE_TIME_SECOND = 48;

        /// <summary>
        /// Errors inside of the error sound cooldown period will be ignored and still
        /// reset the error sound cooldown.
        /// </summary>
        private const int ERROR_SOUND_COOLDOWN = 200;

        protected Rectangle _Location;

        /// <summary>
        /// The location of this text field. The width and height
        /// must match the underlying texture.
        /// </summary>
        public Rectangle Location
        {
            get
            {
                return _Location;
            }

            set
            {
                _TextPositionVec = null;
                _Location = value;
            }
        }

        /// <summary>
        /// The size of this text field
        /// </summary>
        public Point Size
        {
            get
            {
                return Location.Size;
            }
        }

        public Point Center
        {
            get
            {
                return Location.Center;
            }

            set
            {
                _Location.X = value.X - Size.X / 2;
                _Location.Y = value.Y - Size.Y / 2;
                _TextPositionVec = null;
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

        protected bool _Disabled;

        /// <summary>
        /// Gets or sets if this text field is disabled at the moment.
        /// </summary>
        public bool Disabled
        {
            get
            {
                return _Disabled;
            }

            set
            {
                _Disabled = value;
            }
        }
        /// <summary>
        /// Where the top-left of the text is positioned. Must be nulled whenever
        /// something changes that would effect this.
        /// </summary>
        protected Vector2? _TextPositionVec;

        protected string _Text;

        protected bool TextChangedTriggersSuppressed;

        /// <summary>
        /// The current text of the text field
        /// </summary>
        public string Text
        {
            get
            {
                return _Text;
            }

            set
            {
                if((ReferenceEquals(_Text, null) != ReferenceEquals(value, null)) || !value.Equals(_Text))
                {
                    _Text = value;

                    if (!TextChangedTriggersSuppressed)
                    {
                        TextChangedTriggersSuppressed = true;
                        TextChanged?.Invoke(this, EventArgs.Empty);
                        TextChangedTriggersSuppressed = false;
                    }
                }
            }
        }

        /// <summary>
        /// If the text field is currently focused
        /// </summary>
        public bool Focused;
        
        /// <summary>
        /// The name of the font
        /// </summary>
        public string FontName;

        /// <summary>
        /// The color of text
        /// </summary>
        public Color TextColor;

        /// <summary>
        /// The name of the sound effect that is played when a regular character
        /// is added to the text of the textfield due to typing while the text field
        /// is focused. 
        /// </summary>
        public string TypeSFXName;

        /// <summary>
        /// The name of the sound effect that is played when a key is pressed while the
        /// text field is focused that is not valid. I.e. backspace on an empty text field,
        /// or a regular key when the text field is full.
        /// </summary>
        public string InvalidKeySFXName;

        /// <summary>
        /// The maximum allowed number of characters
        /// </summary>
        public int MaxLength;

        /// <summary>
        /// If the caret is currently visible.
        /// </summary>
        protected bool CaretVisible;

        /// <summary>
        /// Time until CaretVisible should be toggled, in milliseconds.
        /// </summary>
        protected int TimeUntilCaretToggleMS;

        /// <summary>
        /// The sound is really obnoxious so we don't play it too often.
        /// </summary>
        protected int ErrorSoundCooldown;
        
        /// <summary>
        /// The background texture for the text field. 
        /// </summary>
        protected Texture2D BackgroundTexture;

        /// <summary>
        /// The backgruond texture for the text field while disabled
        /// </summary>
        protected Texture2D DisabledBackgroundTexture;

        /// <summary>
        /// The background texture while focused
        /// </summary>
        protected Texture2D FocusedBackgroundTexture;

        protected Dictionary<Keys, int> KeysToIgnoreTime;
        protected Keys[] KeysPressedLastUpdate;

        public event EventHandler Disposing;
        public event EventHandler CaretToggled;
        public event EventHandler FocusGained;
        public event EventHandler FocusLost;
        public event EventHandler TextChanged;
        public event EventHandler EnterPressed;

        public TextField(Rectangle location, string text, string font, Color textColor,
            string typeSFXName, string invalidKeySFXName, int maxLength, bool disabled = false)
        {
            _Location = location;
            Text = text;
            FontName = font;
            TextColor = textColor;
            TypeSFXName = typeSFXName;
            InvalidKeySFXName = invalidKeySFXName;
            MaxLength = maxLength;
            Disabled = disabled;

            Focused = false;
            CaretVisible = false;
            TimeUntilCaretToggleMS = 0;
            _TextPositionVec = null;

            KeysPressedLastUpdate = new Keys[0];
            KeysToIgnoreTime = new Dictionary<Keys, int>();
        }

        public void Resize(Point size)
        {
            BackgroundTexture?.Dispose();
            BackgroundTexture = null;
            FocusedBackgroundTexture?.Dispose();
            FocusedBackgroundTexture = null;
            DisabledBackgroundTexture?.Dispose();
            DisabledBackgroundTexture = null;

            Location = new Rectangle(
                Center.X - size.X / 2, 
                Center.Y - size.Y / 2,
                size.X, 
                size.Y
                );
        }
        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (!Disabled && !Focused && BackgroundTexture == null)
                InitBackgroundTexture(content, graphics, graphicsDevice, spriteBatch);
            if (!Disabled && Focused && FocusedBackgroundTexture == null)
                InitFocusedBackgroundTexture(content, graphics, graphicsDevice, spriteBatch);
            if (Disabled && DisabledBackgroundTexture == null)
                InitDisabledBackgroundTexture(content, graphics, graphicsDevice, spriteBatch);

            if (Disabled)
                spriteBatch.Draw(DisabledBackgroundTexture, destinationRectangle: Location);
            else if (Focused)
                spriteBatch.Draw(FocusedBackgroundTexture, destinationRectangle: Location);
            else
                spriteBatch.Draw(BackgroundTexture, destinationRectangle: Location);

            var font = content.Load<SpriteFont>(FontName);
            var textToDraw = Text;
            if (CaretVisible)
                textToDraw += "|";

            if(!_TextPositionVec.HasValue)
            {
                var textHeight = font.MeasureString("Ag|").Y;
                
                _TextPositionVec = new Vector2(Location.Left + TEXT_X_OFFSET, Location.Top + (Location.Height / 2) - (textHeight / 2)); 
            }

            spriteBatch.DrawString(font, textToDraw, _TextPositionVec.Value, TextColor);
        }

        protected virtual void InitBackgroundTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            BackgroundTexture = RoundedRectUtils.CreateRoundedRect(content, graphics, graphicsDevice, Size.X, Size.Y, Color.White, new Color(198, 198, 198), new Color(236, 236, 236), 5, 2, 1);
        }
        
        protected virtual void InitFocusedBackgroundTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            FocusedBackgroundTexture = RoundedRectUtils.CreateRoundedRect(content, graphics, graphicsDevice, Size.X, Size.Y, Color.White, new Color(198, 198, 198), new Color(155, 155, 255), 5, 2, 1);
        }

        protected virtual void InitDisabledBackgroundTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            DisabledBackgroundTexture = RoundedRectUtils.CreateRoundedRect(content, graphics, graphicsDevice, Size.X, Size.Y, new Color(105, 105, 105), new Color(169, 169, 169), new Color(55, 55, 55), 5, 2, 1);
        }

        protected virtual void PlayErrorSound(ContentManager content)
        {
            if(ErrorSoundCooldown <= 0)
                content.Load<SoundEffect>(InvalidKeySFXName).Play();

            ErrorSoundCooldown = ERROR_SOUND_COOLDOWN;
        }

        protected virtual void PlayTapSound(ContentManager content)
        {
            content.Load<SoundEffect>(TypeSFXName).Play();
        }
        
        protected virtual void HandleKeyPress(ContentManager content, Keys key, bool shiftDown)
        {
            if(key == Keys.Delete || key == Keys.Back)
            {
                if(Text.Length == 0)
                {
                    PlayErrorSound(content);
                }else
                {
                    PlayTapSound(content);
                    Text = Text.Substring(0, Text.Length - 1);
                }
                return;
            }

            if(key == Keys.Enter)
            {
                EnterPressed?.Invoke(this, EventArgs.Empty);
                return;
            }

            var keyChar = GetCharRepresentationOfKey(key, shiftDown);

            if(!keyChar.HasValue)
            {
                PlayErrorSound(content);
            }else
            {
                if(Text.Length == MaxLength)
                {
                    PlayErrorSound(content);
                    return;
                }

                var newText = Text + keyChar;
                var font = content.Load<SpriteFont>(FontName);

                if(font.MeasureString(newText).X > Size.X - TEXT_X_OFFSET * 2)
                {
                    PlayErrorSound(content);
                    return;
                }


                PlayTapSound(content);
                Text = Text + keyChar;
            }
        }

        public void Update(ContentManager content, int deltaMS)
        {
            if (Disabled)
                Focused = false;
            ErrorSoundCooldown -= deltaMS;

            if(Focused)
            {
                TimeUntilCaretToggleMS -= deltaMS;

                if(TimeUntilCaretToggleMS < 0)
                {
                    if(CaretVisible)
                    {
                        CaretVisible = false;
                        TimeUntilCaretToggleMS = CARET_HIDDEN_TIME_MS;
                    }else
                    {
                        CaretVisible = true;
                        TimeUntilCaretToggleMS = CARET_VISIBLE_TIME_MS;
                    }
                    CaretToggled?.Invoke(this, EventArgs.Empty);
                }

                var keys = KeysToIgnoreTime.Keys;
                var keysCopied = new List<Keys>();
                keysCopied.AddRange(keys);
                foreach(var key in keysCopied)
                {
                    var val = KeysToIgnoreTime[key];
                    if(val > 0)
                        KeysToIgnoreTime[key] = val - deltaMS;
                }
            }
        }



        /// <summary>
        /// Gets the character representation of the key, or null if it does not
        /// convert into an alphanumeric character.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="shiftDown">If shift is down</param>
        /// <returns></returns>
        protected char? GetCharRepresentationOfKey(Keys key, bool shiftDown)
        {
            switch (key)
            {
                case Keys.A:
                    return shiftDown ? 'A' : 'a';
                case Keys.B:
                    return shiftDown ? 'B' : 'b';
                case Keys.C:
                    return shiftDown ? 'C' : 'c';
                case Keys.D:
                    return shiftDown ? 'D' : 'd';
                case Keys.E:
                    return shiftDown ? 'E' : 'e';
                case Keys.F:
                    return shiftDown ? 'F' : 'f';
                case Keys.G:
                    return shiftDown ? 'G' : 'g';
                case Keys.H:
                    return shiftDown ? 'H' : 'h';
                case Keys.I:
                    return shiftDown ? 'I' : 'i';
                case Keys.J:
                    return shiftDown ? 'J' : 'j';
                case Keys.K:
                    return shiftDown ? 'K' : 'k';
                case Keys.L:
                    return shiftDown ? 'L' : 'l';
                case Keys.M:
                    return shiftDown ? 'M' : 'm';
                case Keys.N:
                    return shiftDown ? 'N' : 'n';
                case Keys.O:
                    return shiftDown ? 'O' : 'o';
                case Keys.P:
                    return shiftDown ? 'P' : 'p';
                case Keys.Q:
                    return shiftDown ? 'Q' : 'q';
                case Keys.R:
                    return shiftDown ? 'R' : 'r';
                case Keys.S:
                    return shiftDown ? 'S' : 's';
                case Keys.T:
                    return shiftDown ? 'T' : 't';
                case Keys.U:
                    return shiftDown ? 'U' : 'u';
                case Keys.V:
                    return shiftDown ? 'V' : 'v';
                case Keys.W:
                    return shiftDown ? 'W' : 'w';
                case Keys.X:
                    return shiftDown ? 'X' : 'x';
                case Keys.Y:
                    return shiftDown ? 'Y' : 'y';
                case Keys.Z:
                    return shiftDown ? 'Z' : 'z';
                case Keys.Space:
                    return ' ';
                case Keys.OemPeriod:
                    return shiftDown ? '>' : '.';
                case Keys.OemComma:
                    return shiftDown ? '<' : ',';
                case Keys.OemSemicolon:
                    return shiftDown ? ':' : ';';
                case Keys.OemQuestion:
                    return shiftDown ? '?' : '/';
                case Keys.OemQuotes:
                    return shiftDown ? '"' : '\'';
                case Keys.OemOpenBrackets:
                    return shiftDown ? '{' : '[';
                case Keys.OemCloseBrackets:
                    return shiftDown ? '}' : ']';
                case Keys.OemPlus:
                    return shiftDown ? '+' : '=';
                case Keys.D1:
                    return shiftDown ? '!' : '1';
                case Keys.D2:
                    return shiftDown ? '@' : '2';
                case Keys.D3:
                    return shiftDown ? '#' : '3';
                case Keys.D4:
                    return shiftDown ? '$' : '4';
                case Keys.D5:
                    return shiftDown ? '%' : '5';
                case Keys.D6:
                    return shiftDown ? '^' : '6';
                case Keys.D7:
                    return shiftDown ? '&' : '7';
                case Keys.D8:
                    return shiftDown ? '*' : '8';
                case Keys.D9:
                    return shiftDown ? '(' : '9';
                case Keys.D0:
                    return shiftDown ? ')' : '0';
                case Keys.NumPad1:
                    return '1';
                case Keys.NumPad2:
                    return '2';
                case Keys.NumPad3:
                    return '3';
                case Keys.NumPad4:
                    return '4';
                case Keys.NumPad5:
                    return '5';
                case Keys.NumPad6:
                    return '6';
                case Keys.NumPad7:
                    return '7';
                case Keys.NumPad8:
                    return '8';
                case Keys.NumPad9:
                    return '9';
                case Keys.NumPad0:
                    return '0';
                case Keys.Subtract:
                    return '-';
                case Keys.Add:
                    return '+';
                case Keys.Divide:
                    return '/';
                case Keys.Multiply:
                    return '*';
            }

            Console.WriteLine($"Weird key {key}");
            return null;
        }

        /// <summary>
        /// If not handled and mouse was clicked somewere than update field's focus state.
        /// If field was clicked hovered over than set handled to true.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="last">The previous mouse state.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="handled">If the mouse has been handled.</param>
        /// <param name="scrollHandled">If the scroll wheel has been handled.</param>
        public void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            var mousePos = current.Position;
            var mouseDown = current.LeftButton == ButtonState.Pressed;

            var newHover = !handled && Location.Contains(mousePos);
            var newFocus = !Disabled && (mouseDown ? newHover : Focused);

            if (newFocus != Focused)
            {
                if (newFocus)
                {
                    Focused = true;
                    CaretVisible = true;
                    TimeUntilCaretToggleMS = CARET_VISIBLE_TIME_MS;
                    FocusGained?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Focused = false;
                    CaretVisible = false;
                    TimeUntilCaretToggleMS = 0;
                    FocusLost?.Invoke(this, EventArgs.Empty);
                }
            }

            handled = handled || newHover;
        }

        /// <summary>
        /// If keyboard was not handled and this field is focused than handle all key presses and set handled to true.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="last">The previous keyboard state.</param>
        /// <param name="keyboardState">The current keyboard state.</param>
        /// <param name="handled">If the keyboard has been handled.</param>
        public void HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState keyboardState, ref bool handled)
        {
            if (!Focused || handled)
                return;

            var pressedKeys = keyboardState.GetPressedKeys();

            bool shiftDown = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

            foreach (var key in KeysPressedLastUpdate)
            {
                var pressedNow = pressedKeys.Any((k) => key == k);

                if (!pressedNow)
                {
                    KeysToIgnoreTime.Remove(key);
                }
            }

            foreach (var key in pressedKeys)
            {
                if (key == Keys.LeftShift || key == Keys.RightShift)
                    continue;

                var pressedLast = KeysPressedLastUpdate.Any((k) => k == key);
                bool ignore = pressedLast;
                if (ignore)
                {
                    int ignoreTime = KeysToIgnoreTime[key];
                    ignore = ignoreTime > 0;

                    if (!ignore)
                    {
                        KeysToIgnoreTime[key] = KEY_IGNORE_TIME_SECOND;
                    }
                    else
                    {
                        KeysToIgnoreTime[key] = ignoreTime;
                    }
                }
                else
                {
                    KeysToIgnoreTime.Add(key, KEY_IGNORE_TIME_FIRST);
                }

                if (!ignore)
                {
                    HandleKeyPress(content, key, shiftDown);
                }
            }

            KeysPressedLastUpdate = pressedKeys;

            handled = true;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
        }

        public void Dispose()
        {
            Disposing?.Invoke(this, EventArgs.Empty);

            BackgroundTexture?.Dispose();
            BackgroundTexture = null;
            FocusedBackgroundTexture?.Dispose();
            FocusedBackgroundTexture = null;
            DisabledBackgroundTexture?.Dispose();
            DisabledBackgroundTexture = null;
        }
    }
}
