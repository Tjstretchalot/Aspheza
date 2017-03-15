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

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// Takes up space but doesnt draw anything
    /// </summary>
    public class PaddingTaskItemComponent : ITaskItemComponent
    {
        public bool Disposed { get; protected set; }

        public bool Hidden { get; set; }

        public int Width;
        public int Height;

        public PaddingTaskItemComponent(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public void DrawHighPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
        }

        public void DrawLowPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
        }

        public int GetRequiredHeight(RenderContext context)
        {
            return Height;
        }

        public int GetRequiredWidth(RenderContext context)
        {
            return Width;
        }

        public void HandleKeyboardStateHighPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void HandleKeyboardStateLowPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void HandleMouseStateHighPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
        }

        public void HandleMouseStateLowPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
        }

        public void Layout(RenderContext context, int suggestedCenterX, int width, ref int height)
        {
            height += Height;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
        }

        public void UpdateHighPriority(ContentManager content, int deltaMS)
        {
        }

        public void UpdateLowPriorirty(ContentManager content, int deltaMS)
        {
        }
    }
}
