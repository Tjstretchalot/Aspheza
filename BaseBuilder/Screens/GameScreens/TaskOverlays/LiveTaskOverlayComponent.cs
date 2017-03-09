using System;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using BaseBuilder.Engine.State;
using System.Collections.Generic;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Microsoft.Xna.Framework.Audio;

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

        /// <summary>
        /// The button for adding a new task 
        /// </summary>
        public Button AddButton;
        
        /// <summary>
        /// The button for toggling if an entity is completing his tasks
        /// </summary>
        public Button PauseResumeButton;

        protected bool Valid;

        protected ITaskable Taskable;
        public ICollection<ITaskItem> TaskItems;
        protected BiDictionary<Rectangle, ITaskItem> ExpandOrMinimizeIconLocationsToTaskItems;
        
        protected BiDictionary<Rectangle, ITaskItem> SelectLocationsToTaskItems;

        protected Texture2D BackgroundTexture;
        protected Texture2D IconsTexture;
        protected Rectangle ExpandSourceRect;
        protected Rectangle MinimizeSourceRect;

        public LiveTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SharedGameState sharedState, LocalGameState localState, NetContext netContext, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Taskable = taskable;
            
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 1);

            ExpandOrMinimizeIconLocationsToTaskItems = new BiDictionary<Rectangle, ITaskItem>();
            SelectLocationsToTaskItems = new BiDictionary<Rectangle, ITaskItem>();
            
            ListenForTaskEvents();

            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { Color.Gray });

            IconsTexture = content.Load<Texture2D>("icons");
            ExpandSourceRect = new Rectangle(0, 0, 8, 8);
            MinimizeSourceRect = new Rectangle(9, 0, 8, 8);

            RedrawRequired += (sender, args) => { RecalculateSize(); };

            AddButton = UIUtils.CreateButton(new Point(0, 0), "Add Task", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
            PauseResumeButton = UIUtils.CreateButton(new Point(0, 0), Taskable.Paused ? "Resume" : "Stop", UIUtils.ButtonColor.Yellow, UIUtils.ButtonSize.Medium);

            AddButton.HoveredChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            AddButton.PressedChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            PauseResumeButton.HoveredChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            PauseResumeButton.PressedChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);

            PauseResumeButton.PressReleased += (sender, args) =>
            {
                if(Taskable.Paused && !Valid)
                {
                    Content.Load<SoundEffect>("UI/TextAreaError").Play();
                    return;
                }

                if (Taskable.Paused)
                    PauseResumeButton.Text = "Stop";
                else
                    PauseResumeButton.Text = "Resume";

                var order = netContext.GetPoolFromPacketType(typeof(TogglePausedTasksOrder)).GetGamePacketFromPool() as TogglePausedTasksOrder;
                order.Entity = Taskable as Entity;
                localState.Orders.Add(order);
                RedrawRequired?.Invoke(null, EventArgs.Empty);
            };

            AddButton.PressReleased += (sender, args) =>
            {
                TaskAddPressed?.Invoke(this, args);
            };

            CreateTaskItemsFromTaskable();
        }
        
        public override void Draw(RenderContext context)
        {
            context.SpriteBatch.Draw(BackgroundTexture, new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y), Color.White);

            if(!Valid)
            {
                context.SpriteBatch.Draw(IconsTexture, new Rectangle(ScreenLocation.X + Size.X - 15, ScreenLocation.Y + 6, 9, 15), new Rectangle(18, 0, 9, 15), Color.White);
            }

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


                var str = task.TaskName;
                var strSize = context.DefaultFont.MeasureString(str);
                Rectangle rect;
                if (ExpandOrMinimizeIconLocationsToTaskItems.TryGetValue(task, out rect))
                {
                    rect.X = ScreenLocation.X + xPadding;
                    rect.Y = y + (int)(strSize.Y / 2 - 4);
                }
                else
                {
                    rect = new Rectangle(ScreenLocation.X + xPadding, y + (int)(strSize.Y / 2 - 4), 8, 8);
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

                y += context.DefaultFont.LineSpacing + 2;
            }

            AddButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            PauseResumeButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (handled)
                return;

            AddButton.HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
            PauseResumeButton.HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
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
                        Content.Load<SoundEffect>("UI/switch13").Play();
                        RecalculateSize();
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
            {
                handled = true;
                return;
            }

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

            if (Selected != null && !foundHover && last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                Selected = null;
                TaskUnselected?.Invoke(null, EventArgs.Empty);
            }

            handled = true;
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            var wasValid = Valid;
            Valid = true;
            foreach(var item in TaskItems)
            {
                if(!item.IsValid(sharedGameState, localGameState, netContext))
                {
                    Valid = false;
                    break;
                }
            }

            if (Valid != wasValid)
                RedrawRequired?.Invoke(null, EventArgs.Empty);
        }

        protected void CreateTaskItemsFromTaskable()
        {
            if (Selected != null)
            {
                TaskUnselected?.Invoke(null, EventArgs.Empty);
                Selected = null;
            }
            var queue = Taskable.TaskQueue.ToArray();
            var taskItems = new LinkedList<ITaskItem>();
            
            if (Taskable.CurrentTask != null)
                taskItems.AddLast(TaskItemIdentifier.Init(Taskable.CurrentTask));
            for (int i = 0; i < Taskable.TaskQueue.Count; i++)
            {
                taskItems.AddLast(TaskItemIdentifier.Init(queue[i]));
            }

            TaskItems = taskItems;

            RecalculateSize();
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
            Taskable.TaskQueued += CreateTaskItemsFromTaskableAndRedraw;
        }

        /// <summary>
        /// Recalculates the size of this component.
        /// </summary>
        protected void RecalculateSize()
        {
            ExpandOrMinimizeIconLocationsToTaskItems.Clear();
            SelectLocationsToTaskItems.Clear();
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

                if(item.Expandable && item.Expanded)
                {
                    foreach(var child in item.Children)
                    {
                        stack.AddFirst(child);
                    }
                }

                var strSize = font.MeasureString(item.TaskName);

                widestStringX = (int)Math.Max(widestStringX, strSize.X);
                height += (int)(font.LineSpacing + 2);
            }

            var widestThing = Math.Max(widestStringX, PauseResumeButton.Size.X);
            widestThing = Math.Max(widestThing, AddButton.Size.X);
            int width = 5 + 11 + widestThing + 5; // 5px padding + 8 px expand/minimize + 3px padding + string + 5 px padding
            
            height += 5;
            PauseResumeButton.Center = new Point(width / 2, height + PauseResumeButton.Size.Y / 2);
            height += PauseResumeButton.Size.Y + 3;
            AddButton.Center = new Point(width / 2, height + AddButton.Size.Y / 2);
            height += AddButton.Size.Y;

            height += 5;

            Size = new PointI2D(width, height);
        }
    }
}