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
using System.Linq;

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
        /// Called when we don't have a task selected but we are clicking
        /// the grey area of this live task overlay.
        /// </summary>
        public event EventHandler FocusRequested;

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

        /// <summary>
        /// If the user is trying to drag an item right now
        /// </summary>
        protected bool Dragging;

        /// <summary>
        /// The parent of the item that is being dragged. May be null
        /// if the task has no parent.
        /// </summary>
        protected ITaskItem DraggingItemsParent;

        /// <summary>
        /// The index of the item being dragged in DraggingItemsParent's children.
        /// Items can only be dragged within their current parent, never to a 
        /// different parent.
        /// </summary>
        protected int DraggingItemIndex;

        /// <summary>
        /// Where the items index should be after the drag is complete based on the
        /// current mouse location. May be -1, which would imply the mouse isn't 
        /// hovering somewhere that makes sense. May also be the length of the 
        /// parents children array; this just means that it should be placed at
        /// the end of the array. Otherwise, assume the item at this index should be
        /// pushed up an index.
        /// 
        /// Remember to be careful; this index should be considered as the index if
        /// a NEW one is added, not the index after removing and readding.
        /// </summary>
        protected int DraggingToIndex;
        
        protected bool Valid;

        protected ITaskable Taskable;
        public IList<ITaskItem> TaskItems;
        protected BiDictionary<Rectangle, ITaskItem> ExpandOrMinimizeIconLocationsToTaskItems;
        
        protected BiDictionary<Rectangle, ITaskItem> SelectLocationsToTaskItems;

        protected Texture2D BackgroundTexture;
        protected Texture2D IconsTexture;
        protected Texture2D LineTexture;
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

            LineTexture = new Texture2D(graphicsDevice, 1, 1);
            LineTexture.SetData(new Color[] { Color.Black });
            IconsTexture = content.Load<Texture2D>("icons");
            ExpandSourceRect = new Rectangle(0, 0, 8, 8);
            MinimizeSourceRect = new Rectangle(9, 0, 8, 8);

            RedrawRequired += (sender, args) => { RecalculateSize(); };

            AddButton = UIUtils.CreateButton(new Point(0, 0), "Add Task", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
            PauseResumeButton = UIUtils.CreateButton(new Point(0, 0), Taskable.IsPaused ? "Resume" : "Stop", UIUtils.ButtonColor.Yellow, UIUtils.ButtonSize.Medium);

            AddButton.HoveredChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            AddButton.PressedChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            PauseResumeButton.HoveredChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);
            PauseResumeButton.PressedChanged += (sender, args) => RedrawRequired?.Invoke(this, EventArgs.Empty);

            PauseResumeButton.PressReleased += (sender, args) =>
            {
                if(Taskable.IsPaused && !Valid)
                {
                    Content.Load<SoundEffect>("UI/TextAreaError").Play();
                    return;
                }
                
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
            
            Taskable.PausedChanged += (sender, args) =>
            {
                if (Taskable.IsPaused)
                    PauseResumeButton.Text = "Resume";
                else
                    PauseResumeButton.Text = "Stop";

                RedrawRequired?.Invoke(null, EventArgs.Empty);
            };
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

                bool usedYShift = false;

                var str = task.TaskName;
                var strSize = context.DefaultFont.MeasureString(str);

                if (Dragging && DraggingToIndex == 0 && ReferenceEquals(task.Parent, DraggingItemsParent))
                {
                    bool isIndex0;
                    if (task.Parent == null)
                    {
                        isIndex0 = ReferenceEquals(TaskItems[0], task);
                    }
                    else
                    {
                        isIndex0 = ReferenceEquals(task.Parent.Children[0], task);
                    }

                    if (isIndex0)
                    {
                        context.SpriteBatch.Draw(LineTexture, new Rectangle(ScreenLocation.X + xPadding + 11, y, (int)strSize.X, 2), Color.White);
                        usedYShift = true;
                        y += 2;
                    }
                }

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

                if(!task.IsValid())
                {
                    var rect2 = new Rectangle(rect.X + rect.Width + 3, rect.Y, 9, 15);
                    context.SpriteBatch.Draw(IconsTexture, rect2, new Rectangle(18, 0, 9, 15), Color.White);
                }

                y += context.DefaultFont.LineSpacing;

                if (Dragging && DraggingToIndex != -1 && DraggingToIndex != DraggingItemIndex && ReferenceEquals(task.Parent, DraggingItemsParent))
                {
                    var indexInParent = -1;
                    if(task.Parent == null)
                    {
                        indexInParent = TaskItems.IndexOf(task);
                    }else
                    {
                        indexInParent = task.Parent.Children.IndexOf(task);
                    }

                    if(indexInParent == DraggingToIndex - 1)
                        context.SpriteBatch.Draw(LineTexture, new Rectangle(ScreenLocation.X + xPadding + 11, y, (int)strSize.X, 2), Color.White);
                }

                if(!usedYShift)
                    y += 2;
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
            bool buttonsHandled = handled;
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
            var bottomHalfHover = false;
            foreach(var kvp in SelectLocationsToTaskItems.KVPs)
            {
                var rect = kvp.Item1;
                var item = kvp.Item2;

                if(rect.Contains(current.Position))
                {
                    foundHover = true;
                    bottomHalfHover = rect.Center.Y < current.Position.Y;
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
                if(Dragging)
                {
                    // If we're dragging and we found a hover, we need to update the
                    // DraggingToIndex

                    var oldIndex = DraggingToIndex;
                    if (!ReferenceEquals(Hovered.Parent, DraggingItemsParent)) {
                        DraggingToIndex = -1;
                    }else
                    {
                        if(DraggingItemsParent == null)
                        {
                            DraggingToIndex = TaskItems.IndexOf(Hovered);
                        }else
                        {
                            DraggingToIndex = Hovered.Parent.Children.IndexOf(Hovered);
                        }

                        if (bottomHalfHover)
                            DraggingToIndex++;
                    }

                    if (oldIndex != DraggingToIndex)
                        RedrawRequired?.Invoke(null, EventArgs.Empty);
                }else if (current.LeftButton == ButtonState.Pressed)
                {
                    // If we have the mouse down, assume we're trying to drag 

                    Dragging = true;
                    DraggingItemsParent = Hovered.Parent;

                    if (Hovered.Parent == null)
                        DraggingItemIndex = TaskItems.IndexOf(Hovered);
                    else
                        DraggingItemIndex = Hovered.Parent.Children.IndexOf(Hovered);

                    DraggingToIndex = DraggingItemIndex;
                }

                if (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
                {
                    var selecting = !Dragging;
                    if(!selecting)
                    {
                        if(Hovered.Parent == null)
                        {
                            selecting = DraggingItemIndex == TaskItems.IndexOf(Hovered);
                        }else
                        {
                            selecting = DraggingItemIndex == Hovered.Parent.Children.IndexOf(Hovered);
                        }
                    }

                    if (selecting)
                    {
                        if (Dragging)
                        {
                            Dragging = false;
                            RedrawRequired?.Invoke(null, EventArgs.Empty);
                        }
                        if (Selected == null)
                        {
                            Selected = Hovered;
                            TaskSelected?.Invoke(null, EventArgs.Empty);
                        }
                        else if (Selected != Hovered)
                        {
                            Selected = Hovered;
                            TaskSelectionChanged?.Invoke(null, EventArgs.Empty);
                        }
                    }else if(Dragging)
                    {
                        if (DraggingToIndex != -1 && DraggingToIndex != DraggingItemIndex)
                        {
                            if (DraggingItemsParent == null)
                            {
                                var result = CloneAndSwapIndexesInList(TaskItems, DraggingItemIndex, DraggingToIndex);
                                ReplaceTasksWith(sharedGameState, localGameState, netContext, result);
                            }else
                            {
                                PerformComplicatedDrag(sharedGameState, localGameState, netContext);
                            }
                        }
                        Dragging = false;
                    }
                }
            }

            if (!foundHover && last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                if (Selected != null)
                {
                    Selected = null;
                    TaskUnselected?.Invoke(null, EventArgs.Empty);
                }else if(!buttonsHandled)
                {
                    FocusRequested?.Invoke(null, EventArgs.Empty);
                }
            }

            handled = true;
        }

        /// <summary>
        /// A complicated drag occurs when we complete a valid drag for a child whose parent is not
        /// null.
        /// </summary>
        /// <param name="sharedGameState"></param>
        /// <param name="localGameState"></param>
        /// <param name="netContext"></param>
        private void PerformComplicatedDrag(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext)
        {
            // First lets create the new copy of DraggingItemsParent with the childrens indexes swapped. Then we will
            // swap that in each of their parents where the old version was until we get to TaskItems.

            // *************************************************************************************************************************
            // Note that we do NOT have the correct parent for many items in these lists, and if we tried to replace ITaskItems
            // with this list we would 100% get errors. However, we only need to create a list good enough to call CreateTaskItems 
            // on, which purges the parent/child relationship. We keep track of parent for the item we're replacing purely 
            // out of conveinence for this swap.
            // *************************************************************************************************************************

            // Step 1: The new version of DraggingItemsParent. 
            var newParentOfDraggedItem = DraggingItemsParent.GetType().GetConstructor(new Type[] { }).Invoke(new object[] { }) as ITaskItem;
            newParentOfDraggedItem.Children = CloneAndSwapIndexesInList(DraggingItemsParent.Children, DraggingItemIndex, DraggingToIndex);
            newParentOfDraggedItem.Parent = DraggingItemsParent.Parent;

            // Step 2: We need to replace the upper-most parent of DraggingItemsParent with a new version who reflects this change.
            // To do so, we will have to create a new version of the parent of DraggingItemsParent, and his parent, and his parent, etc.

            // For each iteration of the loop, we are creating a copy of currentlyReplacing.Parent that has 
            // currentlySwapping as the child where currentlyReplacing was before.
            var currentlyReplacing = DraggingItemsParent;
            var currentlySwapping = newParentOfDraggedItem;

            while(currentlySwapping.Parent != null)
            {
                var newParentOfReplacing = currentlyReplacing.Parent.GetType().GetConstructor(new Type[] { }).Invoke(new object[] { }) as ITaskItem;
                newParentOfReplacing.Children = new List<ITaskItem>();
                
                for(int i = 0; i < currentlyReplacing.Parent.Children.Count; i++)
                {
                    if(ReferenceEquals(currentlyReplacing.Parent.Children[i], currentlyReplacing))
                    {
                        newParentOfReplacing.Children.Add(currentlySwapping);
                    }else
                    {
                        newParentOfReplacing.Children.Add(currentlyReplacing.Parent.Children[i]);
                    }
                }

                newParentOfReplacing.Parent = currentlyReplacing.Parent.Parent;

                currentlyReplacing = currentlyReplacing.Parent;
                currentlySwapping = newParentOfReplacing;
            }
            
            var result = new List<ITaskItem>();
            for(int i = 0; i < TaskItems.Count; i++)
            {
                if (ReferenceEquals(TaskItems[i], currentlyReplacing))
                    result.Add(currentlySwapping);
                else
                    result.Add(TaskItems[i]);
            }

            ReplaceTasksWith(sharedGameState, localGameState, netContext, result);
        }

        private IList<T> CloneAndSwapIndexesInList<T>(IList<T> list, int from, int to)
        {
            List<T> result = new List<T>();

            for(int i = 0; i < list.Count; i++)
            {
                if (i == from)
                    continue;

                if (i == to)
                    result.Add(list[from]);

                result.Add(list[i]);
            }

            if (to == list.Count)
                result.Add(list[from]);

            return result;
        }
        private void ReplaceTasksWith(SharedGameState shared, LocalGameState local, NetContext netContext, IList<ITaskItem> newTasks)
        {
            List<IEntityTask> tasks = new List<IEntityTask>();

            foreach(var item in newTasks)
            {
                tasks.Add(item.CreateEntityTask(Taskable, shared, local, netContext));
            }
            
            var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
            order.Entity = Taskable as Entity;
            order.NewQueue = tasks;
            local.Orders.Add(order);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            var wasValid = Valid;
            Valid = true;
            foreach(var item in TaskItems)
            {
                if(!item.IsValid())
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
            var taskItems = new List<ITaskItem>();
            
            if (Taskable.CurrentTask != null)
                taskItems.Add(TaskItemIdentifier.Init(Taskable.CurrentTask));
            for (int i = 0; i < Taskable.TaskQueue.Count; i++)
            {
                taskItems.Add(TaskItemIdentifier.Init(queue[i]));
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

        public override void Dispose()
        {
            base.Dispose();

            LineTexture?.Dispose();
            LineTexture = null;

            BackgroundTexture?.Dispose();
            BackgroundTexture = null;

            AddButton?.Dispose();
            AddButton = null;

            PauseResumeButton?.Dispose();
            PauseResumeButton = null;
        }
    }
}