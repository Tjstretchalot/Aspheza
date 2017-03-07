using System;
using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// This overlay allows the user to select a task by name.
    /// </summary>
    public class AddTaskOverlayComponent : MyGameComponent
    {
        public event EventHandler TaskSelected;
        public event EventHandler RedrawRequired;

        public ITaskItem Selected;

        public AddTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
        }

        public override void Draw(RenderContext context)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

        }
    }
}