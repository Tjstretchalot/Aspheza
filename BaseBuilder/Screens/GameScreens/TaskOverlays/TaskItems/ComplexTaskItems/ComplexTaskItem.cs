using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// A complex task item is set up to allow for hiding/unhiding
    /// components and laying them out automatically. The subclass
    /// should hold weak references to components that it needs to
    /// access.
    /// </summary>
    public abstract class ComplexTaskItem : SimpleTaskItem
    {
        protected ITaskItemComponent InspectComponent;
        protected bool LoadedFromTask;

        protected ComplexTaskItem() : base()
        {
        }

        /// <summary>
        /// Initializes the Component
        /// </summary>
        /// <param name="context">The render context</param>
        protected abstract void InitializeComponent(RenderContext context);

        protected abstract void LoadFromTask();

        protected override void InitializeThings(RenderContext renderContext)
        {
            base.InitializeThings(renderContext);
            InitializeComponent(renderContext);

            if (!LoadedFromTask && Task != null)
            {
                LoadedFromTask = true;
                LoadFromTask();
            }
        }

        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            height += 8;
            InspectComponent.Layout(renderContext, 0, width, ref height);
            height += 8;

            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
        }

        protected override int CalculateWidth(RenderContext renderContext)
        {
            return Math.Max(base.CalculateWidth(renderContext), InspectComponent.GetRequiredWidth(renderContext));
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            base.PreDrawInspect(context, x, y);
            InspectComponent.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            base.DrawInspect(context, x, y);
            InspectComponent.DrawLowPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            InspectComponent.DrawHighPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        protected override void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            InspectComponent.HandleMouseStateHighPriority(Content, last, current, ref handled, ref scrollHandled);
            base.HandleInspectComponentsMouseState(last, current, ref handled, ref scrollHandled);
            InspectComponent?.HandleMouseStateLowPriority(Content, last, current, ref handled, ref scrollHandled);
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            bool handled = false;

            InspectComponent.HandleKeyboardStateHighPriority(Content, last, current, ref handled);
            InspectComponent.HandleKeyboardStateLowPriority(Content, last, current, ref handled);

            return handled;
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            InspectComponent.UpdateHighPriority(Content, timeMS);
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);
            InspectComponent.UpdateLowPriorirty(Content, timeMS);
        }

        public override void DisposeInspect()
        {
            InspectComponent?.Dispose();
            InspectComponent = null;
        }
    }
}
