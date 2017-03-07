using System;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BaseBuilder.Engine.Utility;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// Shows the live tasks. Supports listening for selecting a task,
    /// adding a task, starting/stopping a task, deleting a task, expanding
    /// or contracting a task, and a general listener whenever a redraw will 
    /// be required. Respects size and position.
    /// </summary>
    public class LiveTaskOverlayComponent : MyGameComponent
    {
        public event EventHandler TaskSelected;
        public event EventHandler TaskUnselected;
        public event EventHandler TaskAddPressed;
        public event EventHandler StartPressed;
        public event EventHandler StopPressed;
        public event EventHandler TaskDeletePressed;
        public event EventHandler TaskExpanded;
        public event EventHandler TaskMinimized;
        public event EventHandler RedrawRequired;

        /// <summary>
        /// What task item is selected
        /// </summary>
        public ITaskItem Selected;

        /// <summary>
        /// What task item has it's "+" thing hovered on, which
        /// should cause the whole thing to glow a little bit
        /// </summary>
        public ITaskItem SideHovered;

        /// <summary>
        /// What task item is hovered on at all? This doesn't cause anything
        /// to happen visually.
        /// </summary>
        public ITaskItem Hovered;
        
        protected ITaskable Taskable;
        protected List<ITaskItem> TaskItems;
        protected BiDictionary<Rectangle, ITaskItem> ExpandOrMinimizeIconLocationsToTaskItems;
        
        protected BiDictionary<Rectangle, ITaskItem> SelectLocationsToTaskItems;

        protected Texture2D BackgroundTexture;
        protected Texture2D IconsTexture;
        protected Rectangle ExpandSourceRect;
        protected Rectangle MinimizeSourceRect;

        public LiveTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Taskable = taskable;
            
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 1);

            ExpandOrMinimizeIconLocationsToTaskItems = new BiDictionary<Rectangle, ITaskItem>();
            SelectLocationsToTaskItems = new BiDictionary<Rectangle, ITaskItem>();

            CreateTaskItemsFromTaskable();

            ListenForTaskEvents();
            RecalculateSize();

            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { Color.Gray });

            IconsTexture = content.Load<Texture2D>("icons");
            ExpandSourceRect = new Rectangle(0, 0, 8, 8);
            MinimizeSourceRect = new Rectangle(9, 0, 8, 8);
        }

        public override void Draw(RenderContext context)
        {
            context.SpriteBatch.Draw(BackgroundTexture, new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y), Color.White);

            int y = ScreenLocation.Y + 5;
            const int xPadding = 5;
            var stack = new LinkedList<ITaskItem>();
            foreach(var task in TaskItems)
            {
                stack.AddLast(task);
            }
            
            while(stack.Count > 0)
            {
                var task = stack.First.Value;
                stack.RemoveFirst();

                Rectangle rect;
                if (ExpandOrMinimizeIconLocationsToTaskItems.TryGetValue(task, out rect))
                {
                    rect.X = ScreenLocation.X + xPadding;
                    rect.Y = y;
                }
                else
                {
                    rect = new Rectangle(ScreenLocation.X + xPadding, y, 8, 8);
                    ExpandOrMinimizeIconLocationsToTaskItems[task] = rect;
                }

                if (task.Expanded)
                {
                    context.SpriteBatch.Draw(IconsTexture, rect, MinimizeSourceRect, Color.White);
                    for (int i = task.Children.Count - 1; i >= 0; i--)
                    {
                        stack.AddFirst(task.Children[i]);
                    }
                }else if(task.Expandable)
                {
                    context.SpriteBatch.Draw(IconsTexture, rect, ExpandSourceRect, Color.White);
                }

                var str = task.TaskName;
                var strSize = context.DefaultFont.MeasureString(str);
                if(SelectLocationsToTaskItems.TryGetValue(task, out rect))
                {
                    rect.X = ScreenLocation.X + xPadding + 11;
                    rect.Y = y;
                    rect.Width = (int)strSize.X;
                    rect.Height = (int)strSize.Y;
                }else
                {
                    rect = new Rectangle(ScreenLocation.X + xPadding + 11, y, (int)strSize.X, (int)strSize.Y);
                    SelectLocationsToTaskItems[task] = rect;
                }
                context.SpriteBatch.DrawString(context.DefaultFont, task.TaskName, new Vector2(rect.X, rect.Y), Color.White);   
            }
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            return false;
        }

        protected void CreateTaskItemsFromTaskable()
        {
            TaskItems = new List<ITaskItem>();
        }

        /// <summary>
        /// Sets this live task overlay component to listen for any changes on the taskable.
        /// </summary>
        protected void ListenForTaskEvents()
        {

        }

        /// <summary>
        /// Recalculates the size of this component.
        /// </summary>
        protected void RecalculateSize()
        {
            //int y = 5;

            
        }
    }
}