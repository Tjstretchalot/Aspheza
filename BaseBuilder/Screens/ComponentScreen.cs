using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens
{
    /// <summary>
    /// Describes a screen that is made up of renderable components (i.e. menus)
    /// </summary>
    public class ComponentScreen : Screen
    {
        /// <summary>
        /// The components that make up this screen, in the
        /// order that they are drawn.
        /// </summary>
        protected List<IScreenComponent> Components;

        /// <summary>
        /// If this screen has been initialized yet
        /// </summary>
        protected bool Initialized;

        protected MouseState? MouseLast;
        protected KeyboardState? KeyboardLast;
        public ComponentScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
            Components = new List<IScreenComponent>();
        }

        /// <summary>
        /// Initializes the components.
        /// </summary>
        protected virtual void Initialize()
        {
            Initialized = true;
        }

        public override void Draw()
        {
            if (!Initialized)
                return;

            graphicsDevice.Clear(Color.White);

            foreach(var component in Components)
            {
                component.PreDraw(content, graphics, graphicsDevice);
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

            foreach (var component in Components)
            {
                component.Draw(content, graphics, graphicsDevice, spriteBatch);
            }

            spriteBatch.End();
        }

        public override void Update(int deltaMS)
        {
            if(!Initialized)
                Initialize();

            foreach(var component in Components)
            {
                component.Update(content, deltaMS);
            }

            var currMouse = Mouse.GetState();
            var currKeyboard = Keyboard.GetState();
            if(MouseLast.HasValue && KeyboardLast.HasValue)
            {
                bool mouseHandled = false;
                bool keyboardHandled = false;
                foreach(var component in Components)
                {
                    if (!mouseHandled && component.HandleMouseState(content, MouseLast.Value, currMouse))
                        mouseHandled = true;
                    if (!keyboardHandled && component.HandleKeyboardState(content, KeyboardLast.Value, currKeyboard))
                        keyboardHandled = true;

                    if (mouseHandled && keyboardHandled)
                        break;
                }
            }
            MouseLast = currMouse;
            KeyboardLast = currKeyboard;
        }

        public override void Dispose()
        {
            foreach(var component in Components)
            {
                component.Dispose();
            }

            Components = null;
        }
    }
}
