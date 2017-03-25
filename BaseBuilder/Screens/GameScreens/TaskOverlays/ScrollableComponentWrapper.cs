using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// This class wraps a component in order to make it scrollable. The wrapped component
    /// must respect its size and position.
    /// </summary>
    public class ScrollableComponentWrapper : MyGameComponent
    {
        public int ScrollOffsetY { get; set; }

        protected int MaxHeightBeforeScrolling;

        protected MyGameComponent WrappedComponent;
        protected RenderTarget2D CurrentRender;
        protected Rectangle SourceRect;
        protected Rectangle DrawRect;

        protected bool RedrawRequested;

        public ScrollableComponentWrapper(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MyGameComponent wrapped, PointI2D screenLoc, PointI2D size, int z) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            MaxHeightBeforeScrolling = size.Y;
            size = new PointI2D(Math.Max(size.X, wrapped.Size.X), Math.Min(size.Y, wrapped.Size.Y));
            Init(screenLoc, size, z);

            WrappedComponent = wrapped;
            SourceRect = new Rectangle();
            DrawRect = new Rectangle();

            RedrawRequested = false;
        }

        public override void PreDraw(RenderContext context)
        {
            if (CurrentRender != null)
            {
                if (!RedrawRequested && !CurrentRender.IsContentLost && CurrentRender.Width == WrappedComponent.Size.X && CurrentRender.Height >= WrappedComponent.Size.Y)
                    return;
            }

            RedrawRequested = false;
            WrappedComponent.PreDraw(context);

            // A lot of components recalculate there size in predraw, so we need to ensure scroll offset isn't invalid now
            ScrollOffsetY = Math.Max(ScrollOffsetY, -(WrappedComponent.Size.Y - Size.Y));

            // And make sure we still need to scroll
            if (Math.Min(Size.Y, WrappedComponent.Size.Y) < MaxHeightBeforeScrolling && Size.Y != WrappedComponent.Size.Y)
            {
                Size.Y = Math.Min(MaxHeightBeforeScrolling, WrappedComponent.Size.Y);
                ScrollOffsetY = 0;
            }

            // We don't recreate render targets unless absolutely 100% necessary, it's very expensive. So instead we never downsize
            // the render target and rely on the source rect to correct it
            if(CurrentRender != null && (CurrentRender.Width != WrappedComponent.Size.X || CurrentRender.Height < WrappedComponent.Size.Y))
            {
                CurrentRender?.Dispose();
                CurrentRender = null;
            }

            if(CurrentRender == null)
                CurrentRender = new RenderTarget2D(context.GraphicsDevice, WrappedComponent.Size.X, WrappedComponent.Size.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            var myContext = new RenderContext();
            myContext.Content = context.Content;
            myContext.DefaultFont = context.DefaultFont;
            myContext.Graphics = context.Graphics;

            myContext.GraphicsDevice = context.GraphicsDevice;

            myContext.GraphicsDevice.SetRenderTarget(CurrentRender);

            myContext.GraphicsDevice.Clear(Color.CornflowerBlue);

            myContext.SpriteBatch = new SpriteBatch(context.GraphicsDevice);
            myContext.SpriteBatch.Begin();

            WrappedComponent.ScreenLocation = new PointI2D(0, 0);
            WrappedComponent.Draw(myContext);

            myContext.SpriteBatch.End();

            myContext.GraphicsDevice.SetRenderTarget(null);

            myContext.SpriteBatch.Dispose();
            
        }

        public override void Draw(RenderContext context)
        {
            if(CurrentRender != null && !CurrentRender.IsContentLost)
            {
                SourceRect.X = 0;
                SourceRect.Y = -ScrollOffsetY;
                SourceRect.Width = WrappedComponent.Size.X;
                SourceRect.Height = Math.Min(Size.Y, WrappedComponent.Size.Y);

                if (Size.X > WrappedComponent.Size.X)
                    DrawRect.X = ScreenLocation.X + Size.X / 2 - WrappedComponent.Size.X / 2;
                else
                    DrawRect.X = ScreenLocation.X;

                if (Size.Y > WrappedComponent.Size.Y)
                    DrawRect.Y = ScreenLocation.Y + Size.Y / 2 - WrappedComponent.Size.Y / 2;
                else
                    DrawRect.Y = ScreenLocation.Y;

                DrawRect.Width = SourceRect.Width;
                DrawRect.Height = SourceRect.Height;

                context.SpriteBatch.Draw(CurrentRender, DrawRect, SourceRect, Color.White);
            }
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            WrappedComponent.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            return WrappedComponent.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame);
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (handled)
                return;

            if (DrawRect == null || !DrawRect.Contains(current.Position))
                return;

            var componentMouseLast = new MouseState(last.X - DrawRect.X, last.Y - DrawRect.Y - ScrollOffsetY, last.ScrollWheelValue, last.LeftButton, last.MiddleButton, last.RightButton, last.XButton1, last.XButton2);
            var componentMouseCurrent = new MouseState(current.X - DrawRect.X, current.Y - DrawRect.Y - ScrollOffsetY, current.ScrollWheelValue, current.LeftButton, current.MiddleButton, current.RightButton, current.XButton1, current.XButton2);

            WrappedComponent.HandleMouseState(sharedGameState, localGameState, netContext, componentMouseLast, componentMouseCurrent, ref handled, ref scrollHandled);
            
            if (!scrollHandled && last.ScrollWheelValue != current.ScrollWheelValue)
            {
                scrollHandled = true;
                // Scrolling was requested

                var deltaScroll = (int)Math.Round((current.ScrollWheelValue - last.ScrollWheelValue) * 0.07);
                var desiredNewScrollY = ScrollOffsetY + deltaScroll;

                // Can't scroll things to cause them to go down visually
                desiredNewScrollY = Math.Min(desiredNewScrollY, 0);

                // Can't scroll past the bottom
                desiredNewScrollY = Math.Max(desiredNewScrollY, -(WrappedComponent.Size.Y - Size.Y));

                ScrollOffsetY = desiredNewScrollY;
            }

            handled = true;
        }

        /// <summary>
        /// Must be called whenever the wrapped component needs to be redrawn.
        /// </summary>
        public void Invalidate()
        {
            RedrawRequested = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            CurrentRender?.Dispose();
            CurrentRender = null;
            WrappedComponent.Dispose();
        }
    }
}
