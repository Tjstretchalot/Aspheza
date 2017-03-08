using System;
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

        public TaskMenuOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, LocalGameState localState, NetContext netContext, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Taskable = taskable;

            LiveOverlay = new LiveTaskOverlayComponent(content, graphics, graphicsDevice, spriteBatch, taskable);
            InspectOverlay = null;
            AddOverlay = null;

            
            LiveScrollableOverlay = new ScrollableComponentWrapper(content, graphics, graphicsDevice, spriteBatch, LiveOverlay, new PointI2D(50, 50), new PointI2D(200, 400), 6);
            
            LiveOverlay.TaskSelected += (sender, args) =>
            {
                InspectOverlay = new InspectTaskOverlayComponent(content, graphics, graphicsDevice, spriteBatch, LiveOverlay.Selected);
                InspectScrollableOverlay = new ScrollableComponentWrapper(content, graphics, graphicsDevice, spriteBatch, InspectOverlay, new PointI2D(255, 50), new PointI2D(200, 400), 6);

                InspectOverlay.AddPressed += (sender2, args2) =>
                {
                    AddOverlay = new AddTaskOverlayComponent(content, graphics, graphicsDevice, spriteBatch);
                    AddScrollableOverlay = new ScrollableComponentWrapper(content, graphics, graphicsDevice, spriteBatch, AddOverlay, new PointI2D(360, 50), new PointI2D(200, 400), 7);
                    AddOverlay.TaskSelected += (sender3, args3) =>
                    {
                        AddScrollableOverlay.Dispose();

                        AddOverlay = null;
                        AddScrollableOverlay = null;
                    };

                    AddOverlay.RedrawRequired += (sender3, args3) =>
                    {
                        AddScrollableOverlay?.Invalidate();
                    };
                };

                InspectOverlay.RedrawRequired += (sender2, args2) =>
                {
                    InspectScrollableOverlay?.Invalidate();
                };

                InspectOverlay.DeletePressed += (sender2, args2) =>
                {
                    var toDelete = LiveOverlay.Selected;
                    if(toDelete.Parent == null)
                    {
                        if(ReferenceEquals(toDelete.Task, Taskable.CurrentTask))
                        {
                            var newQueue = new Queue<IEntityTask>();
                            var replQueue = new List<IEntityTask>();

                            newQueue.Enqueue(Taskable.CurrentTask);

                            while(Taskable.TaskQueue.Count > 0)
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
                            
                            while(Taskable.TaskQueue.Count > 0)
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
                };
            };

            LiveOverlay.TaskUnselected += (sender, args) =>
            {
                DisposeInspect();
            };

            LiveOverlay.RedrawRequired += (sender, args) =>
            {
                LiveScrollableOverlay.Invalidate();
            };
            
        }

        void DisposeInspect()
        {
            LiveOverlay.Selected = null;
            InspectScrollableOverlay.Dispose();

            InspectOverlay = null;
            InspectScrollableOverlay = null;

            if (AddOverlay != null)
            {
                AddScrollableOverlay.Dispose();

                AddOverlay = null;
                AddScrollableOverlay = null;
            }
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
