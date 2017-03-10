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
        
        public TaskMenuOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SharedGameState sharedState, LocalGameState localState, NetContext netContext, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Taskable = taskable;

            LiveOverlay = new LiveTaskOverlayComponent(content, graphics, graphicsDevice, spriteBatch, sharedState, localState, netContext, taskable);
            InspectOverlay = null;
            AddOverlay = null;

            
            LiveScrollableOverlay = new ScrollableComponentWrapper(content, graphics, graphicsDevice, spriteBatch, LiveOverlay, new PointI2D(50, 50), new PointI2D(200, 300), 6);
            
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
            InspectOverlay.ScreenLocation = new PointI2D(0, 0);
            InspectOverlay.PreDraw(tmpC);
            InspectScrollableOverlay = new ScrollableComponentWrapper(Content, Graphics, GraphicsDevice, SpriteBatch, InspectOverlay, new PointI2D(275, 50), new PointI2D(200, 300), 6);

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
                if (toDelete == null)
                    return;

                if (toDelete.Parent == null)
                {
                    LiveOverlay.TaskItems.Remove(toDelete);
                }else
                {
                    toDelete.Parent.Children.Remove(toDelete);
                }

                var replQueue = new List<IEntityTask>();

                foreach (var item in LiveOverlay.TaskItems)
                {
                    replQueue.Add(item.CreateEntityTask(Taskable, sharedState, localState, netContext));
                }

                var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
                order.Entity = Taskable as Entity;
                order.NewQueue = replQueue;
                localState.Orders.Add(order);

                DisposeInspect();
                LiveOverlay.Selected = null;
            };

            InspectOverlay.SaveRequired += (sender, args) =>
            {
                var replQueue = new List<IEntityTask>();

                foreach(var item in LiveOverlay.TaskItems)
                {
                    replQueue.Add(item.CreateEntityTask(Taskable, sharedState, localState, netContext));
                }

                var order = netContext.GetPoolFromPacketType(typeof(ReplaceTasksOrder)).GetGamePacketFromPool() as ReplaceTasksOrder;
                order.Entity = Taskable as Entity;
                order.NewQueue = replQueue;
                localState.Orders.Add(order);
            };

            if(AddOverlay != null)
            {
                DisposeAdd();
            }
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
            
            AddOverlay = new AddTaskOverlayComponent(Content, Graphics, GraphicsDevice, SpriteBatch, Taskable);
            var tmp = new RenderContext();
            tmp.Graphics = Graphics;
            tmp.GraphicsDevice = GraphicsDevice;
            tmp.Content = Content;
            tmp.SpriteBatch = SpriteBatch;
            tmp.DefaultFont = Content.Load<SpriteFont>("Bitter-Regular");
            AddOverlay.ScreenLocation = new PointI2D(0, 0);
            AddOverlay.PreDraw(tmp);
            AddScrollableOverlay = new ScrollableComponentWrapper(Content, Graphics, GraphicsDevice, SpriteBatch, AddOverlay, new PointI2D(direct ? 255 : InspectScrollableOverlay.ScreenLocation.X + InspectScrollableOverlay.Size.X, 50), new PointI2D(200, 300), 2);
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
                    var asEntityTask = task.CreateEntityTask(Taskable, sharedState, localState, netContext);
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
                        replQueue.Add(item.CreateEntityTask(Taskable, sharedState, localState, netContext));
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

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (handled)
                return;

            AddScrollableOverlay?.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);
            InspectScrollableOverlay?.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);
            LiveScrollableOverlay.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);
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
            
            LiveScrollableOverlay?.Update(sharedGameState, localGameState, netContext, timeMS);
            InspectScrollableOverlay?.Update(sharedGameState, localGameState, netContext, timeMS);
            AddScrollableOverlay?.Update(sharedGameState, localGameState, netContext, timeMS);
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
