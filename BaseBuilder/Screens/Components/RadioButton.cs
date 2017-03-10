using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a radio button. A radio button needs to be part of a group 
    /// if you want to get the real radio button effect.
    /// </summary>
    public class RadioButton : IScreenComponent
    {
        /// <summary>
        /// If this requires high priority z right now
        /// </summary>
        public bool HighPriorityZ { get { return false; } }

        protected bool _Pushed;

        /// <summary>
        /// If this radio button is pushed. Setting this
        /// triggers a pushed change event.
        /// </summary>
        public bool Pushed
        {
            get
            {
                return _Pushed;
            }

            set
            {
                if (_Pushed != value)
                {
                    if(_Pushed)
                    {
                        Size = PushedSourceRect.Size;
                        PushSoundEffect?.Play();
                    }else
                    {
                        Size = UnpushedSourceRect.Size;
                        UnpushSoundEffect?.Play();
                    }

                    _Pushed = value;

                    PushedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected bool _Hovered;
        /// <summary>
        /// If this radio button is currently hovered on. Setting 
        /// this triggers a hovered changed event.
        /// </summary>
        public bool Hovered
        {
            get
            {
                return _Hovered;
            }

            set
            {
                if (_Hovered != value)
                    HoveredChanged?.Invoke(this, EventArgs.Empty);
                _Hovered = value;
            }
        }

        /// <summary>
        /// Triggered whenver Pushed changed
        /// </summary>
        public event EventHandler PushedChanged;

        /// <summary>
        /// Triggered whenever Hovered changed.
        /// </summary>
        public event EventHandler HoveredChanged;

        /// <summary>
        /// The last location that this radio button was drawn. Can
        /// be set to offscreen if the radio button was not drawn to
        /// ensure the hovered/pushed works correctly.
        /// </summary>
        public Rectangle DrawRect;

        /// <summary>
        /// The name of the unpushed texture, or null if none
        /// is given. Can be used to load from content if unpushed
        /// texture is null.
        /// </summary>
        protected string UnpushedTextureName;

        /// <summary>
        /// The name of the pushed texture, or null if 
        /// none is given. Can be used to load from content
        /// if pushed texture is null.
        /// </summary>
        protected string PushedTextureName;

        /// <summary>
        /// The unpushed texture
        /// </summary>
        protected Texture2D UnpushedTexture;

        /// <summary>
        /// The pushed texture
        /// </summary>
        protected Texture2D PushedTexture;

        /// <summary>
        /// The unpushed source rect inside the UnpushedTexture
        /// </summary>
        protected Rectangle UnpushedSourceRect;

        /// <summary>
        /// The pushed source rect inside the PushedTexture
        /// </summary>
        protected Rectangle PushedSourceRect;

        /// <summary>
        /// The sound effect that should be played when pushed
        /// </summary>
        protected SoundEffect PushSoundEffect;

        /// <summary>
        /// The sound effect that should be played when unpushed
        /// </summary>
        protected SoundEffect UnpushSoundEffect;

        /// <summary>
        /// The name of the push sound effect
        /// </summary>
        protected string PushSoundEffectName;

        /// <summary>
        /// The name of the unpush sound effect
        /// </summary>
        protected string UnpushSoundEffectName;

        /// <summary>
        /// The center of this radio button
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// The size of this radio button. Not guarranteed to be accurate 
        /// until the first call to draw.
        /// </summary>
        public Point Size { get; protected set; }

        /// <summary>
        /// Initializes a new radio button with the specified textures and source rects.
        /// </summary>
        /// <param name="pushed">The pushed texture</param>
        /// <param name="unpushed">The unpushed texture</param>
        /// <param name="pushedSource">The source rect for the pushed texture</param>
        /// <param name="unpushedSource">The source rect for the unpushed texture</param>
        /// <param name="center">Where the center of this radio button should be</param>
        public RadioButton(Texture2D pushed, Texture2D unpushed, Rectangle pushedSource, Rectangle unpushedSource, SoundEffect pushSFX, SoundEffect unpushSFX, Point center)
        {
            PushedTexture = pushed;
            UnpushedTexture = unpushed;
            PushedSourceRect = pushedSource;
            UnpushedSourceRect = unpushedSource;
            PushSoundEffect = pushSFX;
            UnpushSoundEffect = unpushSFX;


            Center = center;
            Size = unpushedSource.Size;

            DrawRect = new Rectangle(-1, -1, 1, 1);
            _Hovered = false;
            _Pushed = false;
        }

        /// <summary>
        /// Initializes a radio button with textures that it will load from content on its
        /// first draw
        /// </summary>
        /// <param name="pushedTextureName">The name of the pushed texture</param>
        /// <param name="unpushedTextureName">The name of the unpushed texture</param>
        /// <param name="pushedSource">The source rect for the pushed texture</param>
        /// <param name="unpushedSource">The source rect for the unpushed texture</param>
        /// <param name="center">The center of the radio button</param>
        public RadioButton(string pushedTextureName, string unpushedTextureName, Rectangle pushedSource, Rectangle unpushedSource, string pushSFXName, string unpushSFXName, Point center)
        {
            PushedTextureName = pushedTextureName;
            UnpushedTextureName = unpushedTextureName;
            PushedSourceRect = pushedSource;
            UnpushedSourceRect = unpushedSource;
            PushSoundEffectName = pushSFXName;
            UnpushSoundEffectName = unpushSFXName;

            Center = center;
            Size = unpushedSource.Size;

            DrawRect = new Rectangle(-1, -1, 1, 1);
            _Hovered = false;
            _Pushed = false;
        }

        /// <summary>
        /// Initializes the radio button with default graphics
        /// </summary>
        /// <param name="center">Center of the radio button</param>
        public RadioButton(Point center)
        {
            PushedTextureName = "UI/blueSheet";
            UnpushedTextureName = "UI/greySheet";
            PushedSourceRect = new Rectangle(386, 210, 36, 36);
            UnpushedSourceRect = new Rectangle(185, 469, 36, 36);
            PushSoundEffectName = "UI/switch7";
            UnpushSoundEffectName = "UI/switch8";

            Center = center;
            Size = UnpushedSourceRect.Size;

            DrawRect = new Rectangle(-1, -1, 1, 1);
            _Hovered = false;
            _Pushed = false;
        }

        public void Update(ContentManager content, int deltaMS)
        {
            if (PushSoundEffect == null && PushSoundEffectName != null)
                PushSoundEffect = content.Load<SoundEffect>(PushSoundEffectName);

            if (UnpushSoundEffect == null && UnpushSoundEffectName != null)
                UnpushSoundEffect = content.Load<SoundEffect>(UnpushSoundEffectName);
        }

        /// <summary>
        /// If mouse not handled and radiobutton click set pushed and handled to true.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="last">The previous mouse state.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="handled">If the mouse has been handled.</param>
        /// <param name="scrollHandled">If the scroll wheel has been handled.</param>
        public virtual void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            var newHovered = !handled && DrawRect.Contains(current.Position);
            var justPressed = newHovered && (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released);

            Hovered = newHovered;
            Pushed = justPressed ? true : Pushed;
            handled = handled || newHovered;
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
            if (PushedTexture == null && PushedTextureName != null)
                PushedTexture = content.Load<Texture2D>(PushedTextureName);
            if (UnpushedTexture == null && UnpushedTextureName != null)
                UnpushedTexture = content.Load<Texture2D>(UnpushedTextureName);
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            DrawRect.X = Center.X - Size.X / 2;
            DrawRect.Y = Center.Y - Size.Y / 2;
            DrawRect.Width = Size.X;
            DrawRect.Height = Size.Y;

            if (Pushed)
                spriteBatch.Draw(PushedTexture, DrawRect, PushedSourceRect, Color.White);
            else
                spriteBatch.Draw(UnpushedTexture, DrawRect, UnpushedSourceRect, Color.White);
        }

        public void Dispose()
        {
        }
    }
}
