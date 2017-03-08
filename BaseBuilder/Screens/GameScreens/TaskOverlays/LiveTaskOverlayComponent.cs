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
        /// <summary>
        /// Called when no task is selected then a task is selected
        /// </summary>
        public event EventHandler TaskSelected;

        /// <summary>
        /// Called whenever a task is selected then no task is selected.
        /// </summary>
        public event EventHandler TaskUnselected;

        /// <summary>
        /// Called when a task is selected and then a different task is selecetd
        /// </summary>
        public event EventHandler TaskSelectionChanged;

        /// <summary>
        /// Called when a new task should be added
        /// </summary>
        public event EventHandler TaskAddPressed;

        /// <summary>
        /// Called when the start button is pressed
        /// </summary>
        public event EventHandler StartPressed;

        /// <summary>
        /// Called when the stop button is pressed
        /// </summary>
        public event EventHandler StopPressed;

        /// <summary>
        /// Called when a task is expanded
        /// </summary>
        public event EventHandler TaskExpanded;

        /// <summary>
        /// Called when a task is minimized
        /// </summary>
        public event EventHandler TaskMinimized;

        /// <summary>
        /// Called whenever this component will need to be redrawn
        /// </summary>
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
        protected ICollection<ITaskItem> TaskItems;
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

            RedrawRequired += (sender, args) => { RecalculateSize(); };
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

                y += (int)(strSize.Y + 2);
            }
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            bool foundSideHover = false;
            foreach(var pair in ExpandOrMinimizeIconLocationsToTaskItems.KVPs)
            {
                var rect = pair.Item1;
                var item = pair.Item2;

                if(rect.Contains(current.Position))
                {
                    foundSideHover = true;
                    if(SideHovered != item)
                    {
                        SideHovered = item;
                        RedrawRequired?.Invoke(null, EventArgs.Empty);
                    }

                    if(last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
                    {
                        if (item.Expanded)
                        {
                            item.Expanded = false;
                            TaskMinimized?.Invoke(null, EventArgs.Empty);
                        }
                        else
                        {
                            item.Expanded = true;
                            TaskExpanded?.Invoke(null, EventArgs.Empty);
                        }

                        RedrawRequired?.Invoke(null, EventArgs.Empty);
                    }

                    break;
                }
            }

            if(!foundSideHover && SideHovered != null)
            {
                SideHovered = null;
                RedrawRequired?.Invoke(null, EventArgs.Empty);
            }

            if (foundSideHover)
                return true;

            var foundHover = false;
            foreach(var kvp in SelectLocationsToTaskItems.KVPs)
            {
                var rect = kvp.Item1;
                var item = kvp.Item2;

                if(rect.Contains(current.Position))
                {
                    foundHover = true;
                    if (Hovered != item)
                    {
                        Hovered = item;
                        RedrawRequired?.Invoke(null, EventArgs.Empty);
                    }
                    break;
                }
            }

            if(!foundHover && Hovered != null)
            {
                Hovered = null;

                if(last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
                {
                    Selected = null;
                    TaskUnselected?.Invoke(null, EventArgs.Empty);
                }
            }else if(foundHover)
            {
                if (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
                {
                    if(Selected == null)
                    {
                        Selected = Hovered;
                        TaskSelected?.Invoke(null, EventArgs.Empty);
                    }else if(Selected != Hovered)
                    {
                        Selected = Hovered;
                        TaskSelectionChanged?.Invoke(null, EventArgs.Empty);
                    }
                }
            }

            return true;
        }

        protected void CreateTaskItemsFromTaskable()
        {
            var queue = Taskable.TaskQueue.ToArray();
            var taskItems = new LinkedList<ITaskItem>();
            
            if (Taskable.CurrentTask != null)
                taskItems.AddLast(TaskItemIdentifier.Init(Taskable.CurrentTask));
            for (int i = 0; i < queue.Length; i++)
            {
                taskItems.AddLast(TaskItemIdentifier.Init(queue[i]));
            }

            TaskItems = taskItems;
        }

        void CreateTaskItemsFromTaskableAndRedraw(object sender, EventArgs args)
        {
            CreateTaskItemsFromTaskable();
            RedrawRequired?.Invoke(null, EventArgs.Empty);
        }
        /// <summary>
        /// Sets this live task overlay component to listen for any changes on the taskable.
        /// </summary>
        protected void ListenForTaskEvents()
        {
            Taskable.TaskFinished += CreateTaskItemsFromTaskableAndRedraw;
            Taskable.TasksCancelled += CreateTaskItemsFromTaskableAndRedraw;
            Taskable.TasksReplaced += CreateTaskItemsFromTaskableAndRedraw;
            Taskable.TaskStarted += CreateTaskItemsFromTaskableAndRedraw;
        }

        /// <summary>
        /// Recalculates the size of this component.
        /// </summary>
        protected void RecalculateSize()
        {
            var font = Content.Load<SpriteFont>("Bitter-Regular");

            int height = 5;

            int widestStringX = 0;

            var stack = new LinkedList<ITaskItem>();

            foreach(var item in TaskItems)
            {
                stack.AddLast(item);
            }

            while(stack.Count > 0)
            {
                var item = stack.First.Value;
                stack.RemoveFirst();

                var strSize = font.MeasureString(item.TaskName);

                widestStringX = (int)Math.Max(widestStringX, strSize.X);
                height += (int)(strSize.Y + 2);
            }

            

            height += 5;
            int width = 5 + 11 + widestStringX + 5; // 5px padding + 8 px expand/minimize + 3px padding + string + 5 px padding
             // TODO buttons
            Size = new PointI2D(width, height);
        }
    }
}