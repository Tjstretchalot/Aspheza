using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.Components.ScrollableComponents
{
    /// <summary>
    /// Describes a task item which is acting as a layout manager
    /// </summary>
    public abstract class ScrollableComponentAsLayoutManager : IScrollableComponent
    {
        public bool Disposed { get; protected set; }
        public bool Hidden { get; set; }

        public List<IScrollableComponent> Children;

        public ScrollableComponentAsLayoutManager()
        {
            Children = new List<IScrollableComponent>();
        }


        public virtual void DrawHighPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.DrawHighPriority(content, graphics, graphicsDevice, spriteBatch);
            }
        }

        public virtual void DrawLowPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.DrawLowPriority(content, graphics, graphicsDevice, spriteBatch);
            }
        }

        public abstract int GetRequiredWidth(RenderContext context);
        public abstract int GetRequiredHeight(RenderContext context);

        public virtual void HandleKeyboardStateHighPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.HandleKeyboardStateHighPriority(content, last, current, ref handled);
            }
        }

        public virtual void HandleKeyboardStateLowPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.HandleKeyboardStateLowPriority(content, last, current, ref handled);
            }
        }

        public virtual void HandleMouseStateHighPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.HandleMouseStateHighPriority(content, last, current, ref handled, ref scrollHandled);
            }
        }

        public virtual void HandleMouseStateLowPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.HandleMouseStateLowPriority(content, last, current, ref handled, ref scrollHandled);
            }
        }

        public abstract void Layout(RenderContext context, int suggestedCenterX, int width, ref int height);

        public virtual void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.PreDraw(content, graphics, graphicsDevice);
            }
        }

        public virtual void UpdateHighPriority(ContentManager content, int deltaMS)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.UpdateHighPriority(content, deltaMS);
            }
        }

        public virtual void UpdateLowPriority(ContentManager content, int deltaMS)
        {
            foreach(var child in Children)
            {
                if (!child.Hidden)
                    child.UpdateLowPriority(content, deltaMS);
            }
        }

        public virtual void Dispose()
        {
            Disposed = true;
            foreach(var child in Children)
            {
                child.Dispose();
            }

            Children = null;
        }
    }
}
