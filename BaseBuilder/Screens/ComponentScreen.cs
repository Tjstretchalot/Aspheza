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


            foreach (var component in Components)
            {
                component.PreDraw(content, graphics, graphicsDevice);
            }

            graphicsDevice.Clear(Color.White);

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
                for(int i = Components.Count - 1; i >= 0; i--)
                {
                    var component = Components[i];
                    component.HandleMouseState(content, MouseLast.Value, currMouse, ref mouseHandled);
                    component.HandleKeyboardState(content, KeyboardLast.Value, currKeyboard, ref keyboardHandled);
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
