using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseBuilder.Screens.Components.UIUtils;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public abstract class SimpleTaskItem : TaskItem
    {
        protected string InspectDescription;
        protected bool Savable;
        
        protected Button DeleteButton;
        protected Button SetChildButton;
        protected Button SaveButton;

        protected PointI2D ButtonShiftLast;

        public override event EventHandler InspectAddPressed;
        public override event EventHandler InspectDeletePressed;
        public override event EventHandler InspectRedrawRequired;
        public override event EventHandler InspectSaveRequired;

        protected ContentManager Content;

        protected Texture2D BackgroundTexture;
        protected Texture2D LineTexture;

        protected Rectangle BackgroundRect;

        protected bool Reload;

        protected virtual void InitializeThings(RenderContext renderContext)
        {
            BackgroundRect = new Rectangle(0, 0, 1, 1);

            Content = renderContext.Content;

            if (DeleteButton == null)
            {
                DeleteButton = CreateButton(new Point(0, 0), "Delete", ButtonColor.Yellow, ButtonSize.Medium);
                DeleteButton.HoveredChanged += (sender, args) =>
                {
                    OnInspectRedrawRequired();
                };
                DeleteButton.PressedChanged += (sender, args) => OnInspectRedrawRequired();
                DeleteButton.PressReleased += (sender, args) => OnInspectDeletePressed();
            }

            if (Expandable && SetChildButton == null)
            {
                SetChildButton = CreateButton(new Point(0, 0), "Set Child", ButtonColor.Blue, ButtonSize.Medium);
                SetChildButton.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                SetChildButton.PressedChanged += (sender, args) => OnInspectRedrawRequired();
                SetChildButton.PressReleased += (sender, args) => OnInspectAddPressed();
            }
            
            if(Savable && SaveButton == null)
            {
                SaveButton = CreateButton(new Point(0, 0), "Save", ButtonColor.Blue, ButtonSize.Medium);
                SaveButton.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                SaveButton.PressedChanged += (sender, args) => OnInspectRedrawRequired();
                SaveButton.PressReleased += (sender, args) => OnInspectSaveRequired();
            }

            if (BackgroundTexture == null)
            {
                BackgroundTexture = new Texture2D(renderContext.GraphicsDevice, 1, 1);
                BackgroundTexture.SetData(new[] { Color.Gray });
            }

            if (LineTexture == null)
            {
                LineTexture = new Texture2D(renderContext.GraphicsDevice, 1, 2);
                LineTexture.SetData(new[] { Color.DarkGray, Color.LightGray });
            }
        }

        protected virtual int CalculateWidth(RenderContext renderContext)
        {
            var descriptionSize = renderContext.DefaultFont.MeasureString(InspectDescription);
            var width = Math.Max(descriptionSize.X, DeleteButton.Size.X);

            if (Savable)
                width = Math.Max(width, SaveButton.Size.X);
            if (Expandable)
                width = Math.Max(width, SetChildButton.Size.X);

            return (int)(5 + width + 5);
        }

        protected virtual int CalculateHeightPreButtons(RenderContext renderContext)
        {
            var descriptionSize = renderContext.DefaultFont.MeasureString(InspectDescription);
            var height = (int)(5 + descriptionSize.Y); // 5 px padding + description

            height += 8; // 3 px padding, 2px line, 3px padding

            return height;
        }

        protected virtual void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            if(Savable)
            {
                SaveButton.Center = new Point(width / 2, height + (SaveButton.Size.Y / 2));
                height += SaveButton.Size.Y + 3;
            }
            if (Expandable)
            {
                SetChildButton.Center = new Point(width / 2, height + (SetChildButton.Size.Y / 2));
                height += SetChildButton.Size.Y + 3;
            }

            DeleteButton.Center = new Point(width / 2, height + (DeleteButton.Size.Y / 2));
            height += DeleteButton.Size.Y;

            height += 5; // padding
        }

        public override void LoadedOrChanged(RenderContext renderContext)
        {
            InitializeThings(renderContext);

            var width = CalculateWidth(renderContext);

            var height = CalculateHeightPreButtons(renderContext);

            CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);

            InspectSize = new PointI2D(width, height);
            ButtonShiftLast = new PointI2D(0, 0);
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            if(Reload)
            {
                LoadedOrChanged(context);
                Reload = false;
            }

            DeleteButton?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            SetChildButton?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            SaveButton?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            if (ButtonShiftLast.X != x || ButtonShiftLast.Y != y)
            {
                if (Expandable)
                {
                    SetChildButton.Center = new Point(SetChildButton.Center.X - ButtonShiftLast.X + x, SetChildButton.Center.Y - ButtonShiftLast.Y + y);
                }
                DeleteButton.Center = new Point(DeleteButton.Center.X - ButtonShiftLast.X + x, DeleteButton.Center.Y - ButtonShiftLast.Y + y);

                ButtonShiftLast = new PointI2D(x, y);
            }

            BackgroundRect.X = x;
            BackgroundRect.Y = y;
            BackgroundRect.Width = InspectSize.X;
            BackgroundRect.Height = InspectSize.Y;
            context.SpriteBatch.Draw(BackgroundTexture, BackgroundRect, Color.White);

            var currY = y + 5;
            var descSize = context.DefaultFont.MeasureString(InspectDescription);

            context.SpriteBatch.DrawString(context.DefaultFont, InspectDescription, new Vector2(5, currY), Color.White);

            currY += (int)descSize.Y + 3;

            context.SpriteBatch.Draw(LineTexture, new Rectangle(x, currY, InspectSize.X, 2), Color.White);

            currY += 2 + 3;

            if (Savable)
                SaveButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            if(Expandable)
                SetChildButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);

            DeleteButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            if (Savable)
                SaveButton.Update(Content, timeMS);
            if(Expandable)
                SetChildButton.Update(Content, timeMS);

            DeleteButton.Update(Content, timeMS);
        }

        public override bool HandleInspectMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            bool handled = false;
            HandleInspectComponentsMouseState(last, current, ref handled);
            handled = handled || BackgroundRect.Contains(current.Position);
            return handled;
        }

        protected virtual void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled)
        {
            SaveButton?.HandleMouseState(Content, last, current, ref handled);
            SetChildButton?.HandleMouseState(Content, last, current, ref handled);
            DeleteButton.HandleMouseState(Content, last, current, ref handled);
        }
        public override void DisposeInspect()
        {
            SaveButton?.Dispose();
            SaveButton = null;

            SetChildButton?.Dispose();
            SetChildButton = null;

            DeleteButton?.Dispose();
            DeleteButton = null;

            BackgroundTexture?.Dispose();
            BackgroundTexture = null;

            LineTexture?.Dispose();
            LineTexture = null;
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
