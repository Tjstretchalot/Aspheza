using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// This is a component inside a complex task item.
    /// </summary>
    public interface ITaskItemComponent
    {
        bool Hidden { get; set; }

        void UpdateHighPriority(ContentManager content, int deltaMS);
        void UpdateLowPriorirty(ContentManager content, int deltaMS);
        void HandleMouseStateHighPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled);
        void HandleMouseStateLowPriority(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled);
        void HandleKeyboardStateHighPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled);
        void HandleKeyboardStateLowPriority(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled);
        void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice);
        void DrawHighPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
        void DrawLowPriority(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);

        void Layout(RenderContext context, int suggestedCenterX, int width, ref int height);
        int GetRequiredHeight(RenderContext context);
        int GetRequiredWidth(RenderContext context);

        void Dispose();
    }
}