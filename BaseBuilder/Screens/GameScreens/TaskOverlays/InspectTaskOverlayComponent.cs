using System;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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
        private bool NeedRefreshTaskItem;

        public InspectTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ITaskItem taskItem) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            TaskItem = taskItem;

            TaskItem.InspectAddPressed += (sender, args) => AddPressed?.Invoke(this, args);
            TaskItem.InspectDeletePressed += (sender, args) => DeletePressed?.Invoke(this, args);
            TaskItem.InspectRedrawRequired += (sender, args) => RedrawRequired?.Invoke(this, args);
            
            RenderContext tmp = new RenderContext();
            tmp.Content = content;
            tmp.Graphics = graphics;
            tmp.GraphicsDevice = graphicsDevice;
            tmp.SpriteBatch = spriteBatch;
            tmp.DefaultFont = Content.Load<SpriteFont>("Bitter-Regular");
            TaskItem.LoadedOrChanged(tmp);

            Size = TaskItem.InspectSize;
        }


        public override void Draw(RenderContext context)
        {
            if(NeedRefreshTaskItem)
            {
                TaskItem.LoadedOrChanged(context);
                NeedRefreshTaskItem = false;

                Size = TaskItem.InspectSize;
            }

            TaskItem.DrawInspect(context, ScreenLocation.X, ScreenLocation.Y);
        }

        public override void Dispose()
        {
            base.Dispose();

            TaskItem.DisposeInspect();
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            return TaskItem.HandleInspectMouseState(sharedGameState, localGameState, netContext, last, current);
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