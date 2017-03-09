using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Context;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// A simple implementation of a game component to reduce boilerplate
    /// </summary>
    public abstract class MyGameComponent : IMyGameComponent
    {
        public virtual PointI2D ScreenLocation { get; set; }
        public virtual PointI2D Size { get; protected set; }
        public virtual int Z { get; protected set; }

        protected ContentManager Content;
        protected GraphicsDeviceManager Graphics;
        protected GraphicsDevice GraphicsDevice;
        protected SpriteBatch SpriteBatch;

        protected MyGameComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Content = content;
            Graphics = graphics;
            GraphicsDevice = graphicsDevice;
            SpriteBatch = spriteBatch;
        }

        protected void Init(PointI2D screenLoc, PointI2D size, int z)
        {
            ScreenLocation = screenLoc;
            Size = size;
            Z = z;
        }

        public virtual void PreDraw(RenderContext renderContext)
        {
        }

        public abstract void Draw(RenderContext context);

        public virtual bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            return false;
        }

        public virtual void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool handledScroll)
        {
        }

        public virtual void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
