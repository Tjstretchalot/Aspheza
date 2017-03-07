using System;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// This component allows a user to get more information about a specific task.
    /// </summary>
    /// <remarks>
    /// This class expects to be disposed when closed and respects screen location and size.
    /// </remarks>
    public class InspectTaskOverlayComponent : MyGameComponent
    {
        public event EventHandler AddPressed;
        public event EventHandler DeletePressed;
        public event EventHandler RedrawRequired;

        private ITaskItem TaskItem;

        public InspectTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ITaskItem taskItem) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            TaskItem = taskItem;
        }


        public override void Draw(RenderContext context)
        {
        }

        public override void Dispose()
        {
            base.Dispose();


        }

        /// <summary>
        /// Sets the new size of this component.
        /// </summary>
        /// <param name="newSize">The new size for this component.</param>
        public void Resize(PointI2D newSize)
        {
            Size = newSize;
        }
    }
}