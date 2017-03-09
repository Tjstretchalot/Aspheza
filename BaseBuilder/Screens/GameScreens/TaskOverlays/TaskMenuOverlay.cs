﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.Utilities;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// Draws the tasks of an entity. This component is network friendly; if two people have a task menu
    /// open for the same entity, they will see each others changes.
    /// </summary>
    /// <remarks>
    /// Draws the tasks of an entity when opened. Unlike many of the overlays, the task menu overlay
    /// expects to be added for one entity then disposed when finished.
    /// </remarks>
    public class TaskMenuOverlay : MyGameComponent
    {
        /// <summary>
        /// The taskable that this task menu is regarding
        /// </summary>
        protected ITaskable Taskable;

        protected LiveTaskOverlayComponent LiveOverlay;
        protected InspectTaskOverlayComponent InspectOverlay;
        protected AddTaskOverlayComponent AddOverlay;

        protected ScrollableComponentWrapper LiveScrollableOverlay;
        protected ScrollableComponentWrapper InspectScrollableOverlay;
        protected ScrollableComponentWrapper AddScrollableOverlay;
        
        public TaskMenuOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SharedGameState sharedState, LocalGameState localState, NetContext netContext, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Taskable = taskable;

            LiveOverlay = new LiveTaskOverlayComponent(content, graphics, graphicsDevice, spriteBatch, localState, netContext, taskable);
            InspectOverlay = null;
            AddOverlay = null;

            
            LiveScrollableOverlay = new ScrollableComponentWrapper(content, graphics, graphicsDevice, spriteBatch, LiveOverlay, new PointI2D(50, 50), new PointI2D(200, 400), 6);
            
            LiveOverlay.TaskSelected += (sender, args) =>
            {
                SetupInspect(sharedState, localState, netContext);
            };

            LiveOverlay.TaskUnselected += (sender, args) =>
            {
                DisposeInspect();
                LiveOverlay.Selected = null;
            };

            LiveOverlay.TaskSelectionChanged += (sender, args) =>
            {
                DisposeInspect();
                SetupInspect(sharedState, localState, netContext);
            };

            LiveOverlay.RedrawRequired += (sender, args) =>
            {
                LiveScrollableOverlay.Invalidate();
            };

            LiveOverlay.TaskAddPressed += (sender, args) =>
            {
                if (InspectOverlay != null)
                    DisposeInspect();

                if (AddOverlay != null)
                    DisposeAdd();
                
                SetupAdd(sharedState, localState, netContext, true);
            };
        }

        void SetupInspect(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            InspectOverlay = new InspectTaskOverlayComponent(Content, Graphics, GraphicsDevice, SpriteBatch, LiveOverlay.Selected);
            var tmpC = new RenderContext();
            tmpC.Graphics = Graphics;
            tmpC.GraphicsDevice = GraphicsDevice;
            tmpC.Content = Content;
            tmpC.SpriteBatch = SpriteBatch;
            tmpC.DefaultFont = Content.Load<SpriteFont>("Bitter-Regular");
            InspectOverlay.PreDraw(tmpC);
            InspectScrollableOverlay = new ScrollableComponentWrapper(Content, Graphics, GraphicsDevice, SpriteBatch, InspectOverlay, new PointI2D(255, 50), new PointI2D(200, 400), 6);

            InspectOverlay.AddPressed += (sender2, args2) =>
            {
                SetupAdd(sharedState, localState, netContext, false);
            };

            InspectOverlay.RedrawRequired += (sender2, args2) =>
            {
                InspectScrollableOverlay?.Invalidate();
            };

            InspectOverlay.DeletePressed += (sender2, args2) =>
            {
                var toDelete = LiveOverlay.Selected;
                if (toDelete.Parent == null)
                {
                    if (ReferenceEquals(toDelete.Task, Taskable.CurrentTask))
                    {
                        var newQueue = new Queue<IEntityTask>();
                        var replQueue = new List<IEntityTask>();

                        newQueue.Enqueue(Taskable.CurrentTask);

                        while (Taskable.TaskQueue.Count > 0)
                        {
                            var tmp = Taskable.TaskQueue.Dequeue();

                            newQueue.Enqueue(tmp);
                            replQueue.Add(tmp);
                        }

                        Taskable.TaskQueue = newQueue;

                        var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
                        order.Entity = Taskable as Entity;
                        order.NewQueue = replQueue;
                        localState.Orders.Add(order);
                    }
                    else
                    {
                        var newQueue = new Queue<IEntityTask>();
                        var replQueue = new List<IEntityTask>();

                        replQueue.Add(Taskable.CurrentTask);

                        while (Taskable.TaskQueue.Count > 0)
                        {
                            var tmp = Taskable.TaskQueue.Dequeue();
                            newQueue.Enqueue(tmp);

                            if (!ReferenceEquals(toDelete.Task, tmp))
                                replQueue.Add(tmp);
                        }

                        Taskable.TaskQueue = newQueue;

                        var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
                        order.Entity = Taskable as Entity;
                        order.NewQueue = replQueue;
                        localState.Orders.Add(order);
                    }
                }

                DisposeInspect();
                LiveOverlay.Selected = null;
            };
        }

        void DisposeInspect()
        {
            InspectScrollableOverlay.Dispose();

            InspectOverlay = null;
            InspectScrollableOverlay = null;

            if (AddOverlay != null)
            {
                DisposeAdd();
            }
        }

        void SetupAdd(SharedGameState sharedState, LocalGameState localState, NetContext netContext, bool direct)
        {
            
            AddOverlay = new AddTaskOverlayComponent(Content, Graphics, GraphicsDevice, SpriteBatch);
            var tmp = new RenderContext();
            tmp.Graphics = Graphics;
            tmp.GraphicsDevice = GraphicsDevice;
            tmp.Content = Content;
            tmp.SpriteBatch = SpriteBatch;
            tmp.DefaultFont = Content.Load<SpriteFont>("Bitter-Regular");
            AddOverlay.PreDraw(tmp);
            AddScrollableOverlay = new ScrollableComponentWrapper(Content, Graphics, GraphicsDevice, SpriteBatch, AddOverlay, new PointI2D(direct ? 255 : InspectScrollableOverlay.ScreenLocation.X + InspectScrollableOverlay.Size.X, 50), new PointI2D(200, 400), 2);
            AddOverlay.TaskSelected += (sender, args) =>
            {
                var ent = Taskable as Entity;
                var task = AddOverlay.Selected;

                if (!task.IsValid(sharedState, localState, netContext) && !ent.Paused)
                {
                    LiveOverlay.PauseResumeButton.Text = "Resume";
                    LiveScrollableOverlay.Invalidate();
                    var ord = netContext.GetPoolFromPacketType(typeof(TogglePausedTasksOrder)).GetGamePacketFromPool() as TogglePausedTasksOrder;
                    ord.Entity = ent;
                    localState.Orders.Add(ord);
                }


                if (direct)
                {
                    var asEntityTask = task.CreateEntityTask(sharedState, localState, netContext);
                    var order = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
                    order.Entity = ent;
                    order.Task = asEntityTask;
                    localState.Orders.Add(order);
                }else
                {
                    var newItems = new List<ITaskItem>();
                    LiveOverlay.Selected.Children.Add(task);

                    var replQueue = new List<IEntityTask>();

                    foreach(var item in LiveOverlay.TaskItems)
                    {
                        replQueue.Add(item.CreateEntityTask(sharedState, localState, netContext));
                    }

                    var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
                    order.Entity = Taskable as Entity;
                    order.NewQueue = replQueue;
                    localState.Orders.Add(order);
                }

                DisposeAdd();
            };

            AddOverlay.RedrawRequired += (sender, args) => AddScrollableOverlay?.Invalidate();
        }

        void DisposeAdd()
        {
            AddScrollableOverlay?.Dispose();

            AddOverlay = null;
            AddScrollableOverlay = null;
        }

        public override void PreDraw(RenderContext renderContext)
        {
            if (LiveScrollableOverlay != null)
                LiveScrollableOverlay.PreDraw(renderContext);

            if (InspectScrollableOverlay != null)
                InspectScrollableOverlay.PreDraw(renderContext);

            if (AddScrollableOverlay != null)
                AddScrollableOverlay.PreDraw(renderContext);
        }

        public override void Draw(RenderContext context)
        {
            if (LiveScrollableOverlay != null)
                LiveScrollableOverlay.Draw(context);

            if (InspectScrollableOverlay != null)
                InspectScrollableOverlay.Draw(context);

            if (AddScrollableOverlay != null)
                AddScrollableOverlay.Draw(context);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            if(AddScrollableOverlay != null)
            {
                if (AddScrollableOverlay.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame))
                    return true;
            }

            if(InspectScrollableOverlay != null)
            {
                if (InspectScrollableOverlay.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame))
                    return true;
            }

            if(LiveScrollableOverlay != null)
            {
                if (LiveScrollableOverlay.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame))
                    return true;
            }

            return false;
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if(AddScrollableOverlay != null)
            {
                if (AddScrollableOverlay.HandleMouseState(sharedGameState, localGameState, netContext, last, current))
                    return true;
            }

            if(InspectScrollableOverlay != null)
            {
                if (InspectScrollableOverlay.HandleMouseState(sharedGameState, localGameState, netContext, last, current))
                    return true;
            }

            if(LiveScrollableOverlay != null)
            {
                if (LiveScrollableOverlay.HandleMouseState(sharedGameState, localGameState, netContext, last, current))
                    return true;
            }

            return false;
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            var thing = Taskable as Thing;
            if(thing != null && thing.Destroyed)
            {
                Dispose();
                localGameState.Components.Remove(this);
                return;
            }

            if (LiveScrollableOverlay != null)
                LiveScrollableOverlay.Update(sharedGameState, localGameState, netContext, timeMS);

            if (InspectScrollableOverlay != null)
                InspectScrollableOverlay.Update(sharedGameState, localGameState, netContext, timeMS);

            if (AddScrollableOverlay != null)
                AddScrollableOverlay.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        public override void Dispose()
        {
            if(LiveScrollableOverlay != null)
            {
                LiveScrollableOverlay.Dispose();
                LiveScrollableOverlay = null;
                LiveOverlay = null;
            }

            if(InspectScrollableOverlay != null)
            {
                InspectScrollableOverlay.Dispose();
                InspectScrollableOverlay = null;
                InspectOverlay = null;
            }

            if(AddScrollableOverlay != null)
            {
                AddScrollableOverlay.Dispose();
                AddScrollableOverlay = null;
                AddOverlay = null;
            }
        }
    }
}
