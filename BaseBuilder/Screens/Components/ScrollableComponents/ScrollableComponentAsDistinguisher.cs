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
using BaseBuilder.Screens.Components.ScrollableComponents.Distinguishers;

namespace BaseBuilder.Screens.Components.ScrollableComponents
{
    /// <summary>
    /// Distinguishes the child of the component with a background and border
    /// </summary>
    public class ScrollableComponentAsDistinguisher : IScrollableComponent
    {
        public bool Disposed { get; set; }
        public bool Hidden { get; set; }

        public IScrollableComponent Child;
        public List<IDistinguisherComponent> Distinguishers;

        public int TopleftX;
        public int TopleftY;

        protected int Width;
        protected int Height;

        /// <summary>
        /// How much padding should be on each side 
        /// </summary>
        public int HorizontalPadding;

        /// <summary>
        /// How much padding should be on the top and bottom
        /// </summary>
        public int VerticalPadding;

        public ScrollableComponentAsDistinguisher(IScrollableComponent child, List<IDistinguisherComponent> distinguishers, int horizPadding, int vertPadding)
        {
            Child = child;
            Distinguishers = distinguishers;
            HorizontalPadding = horizPadding;
            VerticalPadding = vertPadding;
        }

        public void DrawHighPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Child.DrawHighPriority(content, graphics, graphicsDevice, spriteBatch);
        }

        public void DrawLowPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            foreach(var distinguisher in Distinguishers)
            {
                distinguisher.Draw(content, graphics, graphicsDevice, spriteBatch, TopleftX + HorizontalPadding, TopleftY + VerticalPadding, Width - HorizontalPadding * 2, Height - VerticalPadding * 2);
            }

            Child.DrawLowPriority(content, graphics, graphicsDevice, spriteBatch);
        }

        public int GetRequiredHeight(RenderContext context)
        {
            return Child.GetRequiredHeight(context) + VerticalPadding * 2;
        }

        public int GetRequiredWidth(RenderContext context)
        {
            return Child.GetRequiredWidth(context) + HorizontalPadding * 2;
        }

        public void HandleKeyboardStateHighPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            Child.HandleKeyboardStateHighPriority(content, last, current, ref handled);
        }

        public void HandleKeyboardStateLowPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
            Child.HandleKeyboardStateLowPriority(content, last, current, ref handled);
        }

        public void HandleMouseStateHighPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            Child.HandleMouseStateHighPriority(content, last, current, ref handled, ref scrollHandled);
        }

        public void HandleMouseStateLowPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            Child.HandleMouseStateLowPriority(content, last, current, ref handled, ref scrollHandled);
        }

        public void Layout(RenderContext context, int suggestedCenterX, int width, ref int height)
        {
            Width = GetRequiredWidth(context);
            Height = GetRequiredHeight(context);

            TopleftX = suggestedCenterX - (Width / 2);
            TopleftY = height;
            height += VerticalPadding;
            Child.Layout(context, suggestedCenterX, width, ref height);
            height += VerticalPadding;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            foreach(var distinguisher in Distinguishers)
            {
                distinguisher.PreDraw(content, graphics, graphicsDevice, TopleftX + HorizontalPadding, TopleftY + VerticalPadding, Width - HorizontalPadding * 2, Height - VerticalPadding * 2);
            }

            Child.PreDraw(content, graphics, graphicsDevice);
        }

        public void UpdateHighPriority(ContentManager content, int deltaMS)
        {
            Child.UpdateHighPriority(content, deltaMS);
        }

        public void UpdateLowPriority(ContentManager content, int deltaMS)
        {
            Child.UpdateLowPriority(content, deltaMS);
        }

        public void Dispose()
        {
            Disposed = true;

            foreach(var distinguisher in Distinguishers)
            {
                distinguisher.Dispose();
            }
            Distinguishers = null;

            Child.Dispose();
            Child = null;
        }
    }
}
