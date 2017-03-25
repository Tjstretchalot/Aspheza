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
using Microsoft.Xna.Framework.Content;

using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BaseBuilder.Screens.Components.ScrollableComponents;
using BaseBuilder.Screens.GComponents.ScrollableComponents;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// A complex task item is set up to allow for hiding/unhiding
    /// components and laying them out automatically. The subclass
    /// should hold weak references to components that it needs to
    /// access.
    /// </summary>
    public abstract class ComplexTaskItem : TaskItem
    {
        protected IScrollableComponent CompleteComponent;

        public override event EventHandler InspectAddPressed;
        public override event EventHandler InspectDeletePressed;
        public override event EventHandler InspectRedrawRequired;
        public override event EventHandler InspectSaveRequired;

        protected string InspectDescription;

        protected bool Savable;
        protected bool Reload;

        protected ContentManager Content;

        protected bool LoadedFromTask;

        protected Texture2D BackgroundTexture;

        protected ComplexTaskItem() : base()
        {
            LoadedFromTask = false;
        }

        /// <summary>
        /// Initializes the Component
        /// </summary>
        /// <param name="context">The render context</param>
        protected abstract IScrollableComponent InitializeComponent(RenderContext context);

        protected abstract void LoadFromTask(RenderContext context);

        protected virtual void InitializeThings(RenderContext renderContext)
        {
            Content = renderContext.Content;
            
            if (CompleteComponent == null)
                InitializeCompleteComponent(renderContext);

            if (!LoadedFromTask && Task != null)
            {
                LoadedFromTask = true;
                LoadFromTask(renderContext);
            }
        }

        public override void LoadedOrChanged(RenderContext renderContext)
        {
            InitializeThings(renderContext);

            var width = CalculateWidth(renderContext);

            var height = 0;

            CalculateHeight(renderContext, ref height, width);

            InspectSize = new PointI2D(width, height);
        }


        protected virtual void InitializeCompleteComponent(RenderContext context)
        {
            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredWidth, 8);

            /*
             * The performance gained from caching large texts in render target is absolutely insane. 
             * To test, change the sort mode to immediate (so you can see the cost of each call), at
             * the time of writing it was between 1/2 and 7/8 of the entire draw call, dwarfing even
             * tile rendering!
             * 
             * It's possible that drawing text in immediate mode is particularly slow, so the performance
             * improvements are not quite that good, but it was still a very significant improvement.
             */
            var text = CreateText(context, InspectDescription, true);

            layout.Children.Add(CreatePadding(1, 5));
            layout.Children.Add(Wrap(text));
            var lineText = new Texture2D(context.GraphicsDevice, 1, 1);  // we're telling the component to handle disposing of this texture
            lineText.SetData(new[] { Color.DarkGray });
            layout.Children.Add(Wrap(new TextureComponent(lineText, new Rectangle(0, 0, text.Size.X, 2), true)));
            layout.Children.Add(InitializeComponent(context));

            Button button;
            if (Savable) {
                button = CreateButton(context, redraw, redrawAndReload, "Save");
                button.PressReleased += (sender, args) => OnInspectSaveRequired();

                layout.Children.Add(Wrap(button));
            }

            if(Expandable)
            {
                button = CreateButton(context, redraw, redrawAndReload, "Set Child");
                button.PressReleased += (sender, args) => OnInspectAddPressed();

                layout.Children.Add(Wrap(button));
            }

            button = CreateButton(context, redraw, redrawAndReload, "Delete", UIUtils.ButtonColor.Yellow);
            button.PressReleased += (sender, args) => OnInspectDeletePressed();

            layout.Children.Add(Wrap(button));

            CompleteComponent = layout;
        }

        protected virtual void CalculateHeight(RenderContext renderContext, ref int height, int width)
        {
            CompleteComponent.Layout(renderContext, 0, width, ref height);
            height += 5;
        }

        protected virtual int CalculateWidth(RenderContext renderContext)
        {
            return CompleteComponent.GetRequiredWidth(renderContext) + 10;
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            if(BackgroundTexture == null)
            {
                BackgroundTexture = new Texture2D(context.GraphicsDevice, 1, 1);
                BackgroundTexture.SetData(new[] { Color.Gray });
            }

            if (Reload)
            {
                LoadedOrChanged(context);
                Reload = false;
            }

            CompleteComponent.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            context.SpriteBatch.Draw(BackgroundTexture, new Rectangle(x, y, InspectSize.X, InspectSize.Y), Color.White);
            CompleteComponent.DrawLowPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            CompleteComponent.DrawHighPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }


        public override void HandleInspectMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            HandleInspectComponentsMouseState(last, current, ref handled, ref scrollHandled);
        }

        protected virtual void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            CompleteComponent.HandleMouseStateHighPriority(Content, last, current, ref handled, ref scrollHandled);
            CompleteComponent?.HandleMouseStateLowPriority(Content, last, current, ref handled, ref scrollHandled);
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            bool handled = false;

            CompleteComponent.HandleKeyboardStateHighPriority(Content, last, current, ref handled);
            CompleteComponent.HandleKeyboardStateLowPriority(Content, last, current, ref handled);

            return handled;
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            CompleteComponent.UpdateHighPriority(Content, timeMS);
            CompleteComponent.UpdateLowPriority(Content, timeMS);
        }

        public override void DisposeInspect()
        {
            BackgroundTexture?.Dispose();
            BackgroundTexture = null;
            CompleteComponent?.Dispose();
            CompleteComponent = null;

            LoadedFromTask = false;
        }

        protected virtual void OnInspectAddPressed()
        {
            InspectAddPressed?.Invoke(null, EventArgs.Empty);
        }

        protected virtual void OnInspectDeletePressed()
        {
            InspectDeletePressed?.Invoke(null, EventArgs.Empty);
        }

        protected virtual void OnInspectRedrawRequired()
        {
            InspectRedrawRequired?.Invoke(null, EventArgs.Empty);
        }

        protected virtual void OnInspectSaveRequired()
        {
            InspectSaveRequired?.Invoke(null, EventArgs.Empty);
        }
    }
}
