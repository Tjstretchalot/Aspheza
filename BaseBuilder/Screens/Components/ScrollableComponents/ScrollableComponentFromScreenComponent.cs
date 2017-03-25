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
using BaseBuilder.Screens.Components;

namespace BaseBuilder.Screens.Components.ScrollableComponents
{
    /// <summary>
    /// Describes a task item component that is simply a screen component
    /// laid out with no spacing at the suggested x.
    /// </summary>
    public class ScrollableComponentFromScreenComponent <T1> : IScrollableComponent where T1:IScreenComponent
    {
        public bool Disposed { get; protected set; }
        public bool Hidden { get; set; }
        public T1 Component;

        public ScrollableComponentFromScreenComponent(T1 component)
        {
            Component = component;
        }

        public void DrawHighPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (Component.HighPriorityZ)
                Component.Draw(content, graphics, graphicsDevice, spriteBatch);
        }

        public void DrawLowPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (!Component.HighPriorityZ)
                Component.Draw(content, graphics, graphicsDevice, spriteBatch);
        }

        public int GetRequiredWidth(RenderContext context)
        {
            return Component.Size.X;
        }

        public int GetRequiredHeight(RenderContext context)
        {
            return Component.Size.Y;
        }

        public void HandleKeyboardStateHighPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            if (Component.HighPriorityZ)
                Component.HandleKeyboardState(content, last, current, ref handled);
        }

        public void HandleKeyboardStateLowPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            if (!Component.HighPriorityZ)
                Component.HandleKeyboardState(content, last, current, ref handled);
        }

        public void HandleMouseStateHighPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (Component.HighPriorityZ)
                Component.HandleMouseState(content, last, current, ref handled, ref scrollHandled);
        }

        public void HandleMouseStateLowPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (!Component.HighPriorityZ)
                Component.HandleMouseState(content, last, current, ref handled, ref scrollHandled);
        }

        public void Layout(RenderContext context, int suggestedCenterX, int width, ref int height)
        {
            Component.Center = new Point(suggestedCenterX, height + Component.Size.Y / 2);
            height += Component.Size.Y;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            Component.PreDraw(content, graphics, graphicsDevice);
        }

        public void UpdateHighPriority(ContentManager content, int deltaMS)
        {
            if (Component.HighPriorityZ)
                Component.Update(content, deltaMS);
        }

        public void UpdateLowPriority(ContentManager content, int deltaMS)
        {
            if (!Component.HighPriorityZ)
                Component.Update(content, deltaMS);
        }

        public void Dispose()
        {
            Component?.Dispose();
            Component = default(T1);

            Disposed = true;
        }

    }
}
