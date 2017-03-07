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

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public abstract class SimpleTaskItem : TaskItem
    {
        protected string InspectDescription;
        
        protected Button DeleteButton;
        protected Button SetChildButton;

        protected PointI2D ButtonShiftLast;

        public override event EventHandler InspectAddPressed;
        public override event EventHandler InspectDeletePressed;
        public override event EventHandler InspectRedrawRequired;

        protected ContentManager Content;

        protected Texture2D BackgroundTexture;
        protected Texture2D LineTexture;

        protected virtual void InitializeThings(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext)
        {
            Content = renderContext.Content;

            if (DeleteButton == null)
            {
                DeleteButton = CreateButton(new Point(0, 0), "Delete", ButtonColor.Yellow, ButtonSize.Medium);
                DeleteButton.OnHoveredChanged += (sender, args) => InspectRedrawRequired?.Invoke(this, EventArgs.Empty);
                DeleteButton.OnPressedChanged += (sender, args) => InspectRedrawRequired?.Invoke(this, EventArgs.Empty);
                DeleteButton.OnPressReleased += (sender, args) => InspectDeletePressed?.Invoke(this, args);
            }

            if (Expandable && SetChildButton == null)
            {
                SetChildButton = CreateButton(new Point(0, 0), "Set Child", ButtonColor.Blue, ButtonSize.Medium);
                SetChildButton.OnHoveredChanged += (sender, args) => InspectRedrawRequired?.Invoke(this, EventArgs.Empty);
                SetChildButton.OnPressedChanged += (sender, args) => InspectRedrawRequired?.Invoke(this, EventArgs.Empty);
                SetChildButton.OnPressReleased += (sender, args) => InspectAddPressed?.Invoke(this, args);
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

        protected virtual int CalculateWidth(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext)
        {
            var descriptionSize = renderContext.DefaultFont.MeasureString(InspectDescription);
            return (int)(5 + Math.Max(Math.Max(descriptionSize.X, DeleteButton.Size.X), SetChildButton.Size.X) + 5);
        }

        protected virtual int CalculateHeightPreButtons(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext)
        {
            var descriptionSize = renderContext.DefaultFont.MeasureString(InspectDescription);
            var height = (int)(5 + descriptionSize.Y); // 5 px padding + description

            height += 8; // 3 px padding, 2px line, 3px padding

            return height;
        }

        protected virtual void CalculateHeightPostButtonsAndInitButtons(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext, ref int height, int width)
        {
            if (Expandable)
            {
                SetChildButton.Center = new Point((width / 2) - (SetChildButton.Size.X / 2), height + (SetChildButton.Size.Y / 2));
                height += SetChildButton.Size.Y + 3;
            }

            DeleteButton.Center = new Point((width / 2) - (DeleteButton.Size.X / 2), height + (DeleteButton.Size.Y / 2));
            height += DeleteButton.Size.Y;

            height += 5; // padding
        }

        public override void LoadedOrChanged(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext)
        {
            InitializeThings(sharedState, localState, netContext, renderContext);

            var width = CalculateWidth(sharedState, localState, netContext, renderContext);

            var height = CalculateHeightPreButtons(sharedState, localState, netContext, renderContext);

            CalculateHeightPostButtonsAndInitButtons(sharedState, localState, netContext, renderContext, ref height, width);

            InspectSize = new PointI2D(width, height);
            ButtonShiftLast = new PointI2D(0, 0);
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

            context.SpriteBatch.Draw(BackgroundTexture, new Rectangle(x, y, InspectSize.X, InspectSize.Y), Color.White);

            var currY = y + 5;
            var descSize = context.DefaultFont.MeasureString(InspectDescription);

            context.SpriteBatch.DrawString(context.DefaultFont, InspectDescription, new Vector2(5, currY), Color.White);

            currY += (int)descSize.Y + 3;

            context.SpriteBatch.Draw(LineTexture, new Rectangle(x, currY, InspectSize.X, 2), Color.White);

            currY += 2 + 3;

            if(Expandable)
                SetChildButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);

            DeleteButton.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            if(Expandable)
                SetChildButton.Update(Content, timeMS);

            DeleteButton.Update(Content, timeMS);
        }

        public override void DisposeInspect()
        {
            if (BackgroundTexture != null)
            {
                BackgroundTexture.Dispose();
                BackgroundTexture = null;
            }

            if (LineTexture != null)
            {
                LineTexture.Dispose();
                LineTexture = null;
            }
        }
    }
}
