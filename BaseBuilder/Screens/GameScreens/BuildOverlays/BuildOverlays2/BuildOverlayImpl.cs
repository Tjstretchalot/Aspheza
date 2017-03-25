using System;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.Components.ScrollableComponents;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Screens.GComponents.ScrollableComponents;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.Math2D.Double;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays.BuildOverlays2
{
    public class BuildOverlayImpl : MyGameComponent
    {
        public event EventHandler RedrawRequired;
        public event EventHandler SelectionChanged;
        
        protected List<BuildOverlayMenuItem2> Items;
        public UnbuiltImmobileEntity Selected { get; protected set; }
        public BuildOverlayMenuItem2 SelectedItem { get; protected set; }
        protected BuildOverlayMenuItem2 ToSelect;

        public bool Disposed;
        public bool Reload;
        public IScrollableComponent Component;
        public Texture2D BackgroundTexture;

        public BuildOverlayImpl() : base(null, null, null, null)
        {
            Init(new PointI2D(0, 0), new PointI2D(250, 1), 1);
            Items = new List<BuildOverlayMenuItem2>
            {
                new BakeryItem2(),
            };
        }

        public void InitComponent(RenderContext context)
        {
            BackgroundTexture = new Texture2D(context.GraphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { Color.Gray });

            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredWidth, 7);
            layout.Children.Add(CreatePadding(1, 5));
            layout.Children.Add(Wrap(CreateText(context, "Build Menu", true)));

            EventHandler redraw = (sender, args) => OnRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnRedrawRequired();
            };
            foreach(var item in Items)
            {
                layout.Children.Add(item.BuildComponent(context, this, redraw, redrawAndReload));
            }
            layout.Children.Add(CreatePadding(1, 5));

            Component = layout;
            RecalculateSize(context);
        }

        public void RecalculateSize(RenderContext context)
        {
            var width = Math.Max(Size.X, Component.GetRequiredWidth(context));
            var height = 0;
            CalculateHeight(context, width, ref height);
            Size = new PointI2D(width, height);
        }

        public void CalculateHeight(RenderContext context, int width, ref int height)
        {
            Component.Layout(context, 0, width, ref height);
        }

        public override void PreDraw(RenderContext renderContext)
        {
            Content = renderContext.Content;
            if (Disposed)
                return;
            if (Component == null)
            {
                InitComponent(renderContext);
                Reload = false;
                OnRedrawRequired();
            }
            if (Reload)
            {
                RecalculateSize(renderContext);
                Reload = false;
            }
            Component.PreDraw(renderContext.Content, renderContext.Graphics, renderContext.GraphicsDevice);
        }

        public override void Draw(RenderContext context)
        {
            if (Component == null)
                return;
            context.SpriteBatch.Draw(BackgroundTexture, new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y), Color.White);
            Component.DrawLowPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            Component.DrawHighPriority(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {   
            if (Component == null)
                return;
            if(ToSelect != null)
            {
                SelectedItem = ToSelect;
                Selected = SelectedItem.CreateUnbuiltImmobileEntity(sharedGameState);
                ToSelect = null;
                OnSelectionChanged();
            }
            Component.UpdateHighPriority(Content, timeMS);
            Component.UpdateLowPriority(Content, timeMS);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            bool handled = false;
            Component?.HandleKeyboardStateHighPriority(Content, last, current, ref handled);
            Component?.HandleKeyboardStateLowPriority(Content, last, current, ref handled);
            return handled;
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool handledScroll)
        {
            Component?.HandleMouseStateHighPriority(Content, last, current, ref handled, ref handledScroll);
            Component?.HandleMouseStateLowPriority(Content, last, current, ref handled, ref handledScroll);
        }

        public void TryBuildEntity(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, PointD2D placeLocation, UnbuiltImmobileEntity buildingToPlace)
        {
            SelectedItem.TryBuildEntity(sharedGameState, localGameState, netContext, placeLocation, buildingToPlace);
        }

        public void SetSelectedItem(BuildOverlayMenuItem2 item)
        {
            ToSelect = item;
        }

        public void ResetSelected()
        {
            if (Selected == null)
                return;

            Selected = null;
            SelectedItem = null;
            OnSelectionChanged();
        }

        protected virtual void OnRedrawRequired()
        {
            RedrawRequired?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();
            Component?.Dispose();
            Component = null;
            BackgroundTexture?.Dispose();
            BackgroundTexture = null;
            Disposed = true;
        }

    }
}
