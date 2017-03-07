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

        protected MyGameComponent WrappedComponent;
        protected RenderTarget2D CurrentRender;
        protected Rectangle SourceRect;
        protected Rectangle DrawRect;

        public ScrollableComponentWrapper(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, MyGameComponent wrapped, PointI2D screenLoc, PointI2D size, int z) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(screenLoc, size, z);

            WrappedComponent = wrapped;
            SourceRect = new Rectangle();
            DrawRect = new Rectangle();
        }

        public override void PreDraw(RenderContext context)
        {
            if (CurrentRender != null)
            {
                if (CurrentRender.IsContentLost)
                {
                    Invalidate();
                }
                else
                {
                    return;
                }
            }

            WrappedComponent.PreDraw(context);

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
            
        }

        public override void Draw(RenderContext context)
        {
            if(CurrentRender != null && !CurrentRender.IsContentLost)
            {
                SourceRect.X = 0;
                SourceRect.Y = -ScrollOffsetY;
                SourceRect.Width = Size.X;
                SourceRect.Height = Size.Y;

                DrawRect.X = ScreenLocation.X;
                DrawRect.Y = ScreenLocation.Y;
                DrawRect.Width = Size.X;
                DrawRect.Height = Size.Y;

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

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if (DrawRect == null || !DrawRect.Contains(current.Position))
                return false;

            var handledMouse = false;
            if (last.ScrollWheelValue != current.ScrollWheelValue)
            {
                handledMouse = true;
                // Scrolling was requested

                var deltaScroll = (int)Math.Round((current.ScrollWheelValue - last.ScrollWheelValue) * 0.07);
                var desiredNewScrollY = ScrollOffsetY + deltaScroll;

                // Can't scroll things to cause them to go down visually
                desiredNewScrollY = Math.Min(desiredNewScrollY, 0);

                // Can't scroll past the bottom
                desiredNewScrollY = Math.Max(desiredNewScrollY, -(WrappedComponent.Size.Y - Size.Y));

                ScrollOffsetY = desiredNewScrollY;
            }

            var componentMouseLast = new MouseState(last.X - DrawRect.X, last.Y - DrawRect.Y + ScrollOffsetY, last.ScrollWheelValue, last.LeftButton, last.MiddleButton, last.RightButton, last.XButton1, last.XButton2);
            var componentMouseCurrent = new MouseState(current.X - DrawRect.X, current.Y - DrawRect.Y + ScrollOffsetY, current.ScrollWheelValue, current.LeftButton, current.MiddleButton, current.RightButton, current.XButton1, current.XButton2);
            
            handledMouse = WrappedComponent.HandleMouseState(sharedGameState, localGameState, netContext, componentMouseLast, componentMouseCurrent) || handledMouse;

            return handledMouse;
        }

        /// <summary>
        /// Must be called whenever the wrapped component needs to be redrawn.
        /// </summary>
        public void Invalidate()
        {
            if(CurrentRender != null)
            {
                CurrentRender.Dispose();
                CurrentRender = null;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Invalidate();
            WrappedComponent.Dispose();
        }
    }
}
