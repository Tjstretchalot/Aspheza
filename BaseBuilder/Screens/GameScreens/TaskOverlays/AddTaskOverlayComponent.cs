using System;
using System.Linq;
using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.World.Entities.EntityTasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays
{
    /// <summary>
    /// This overlay allows the user to select a task by name.
    /// </summary>
    public class AddTaskOverlayComponent : MyGameComponent
    {
        public event EventHandler TaskSelected;
        public event EventHandler RedrawRequired;

        public ITaskItem Selected;
        public ITaskItem Hovered;

        protected List<ITaskItem> Options;
        protected BiDictionary<ITaskItem, Rectangle> OptionsAndTheirLocations;

        protected Rectangle BackgroundRect;
        protected Texture2D BackgroundTexture;

        public AddTaskOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ITaskable taskable) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Options = new List<ITaskItem>
            {
                new ConditionTaskItem(),
                new FailerTaskItem(),
                new HarvestTaskItem(),
                new InverterTaskItem(),
                new MineGoldTaskItem(),
                new MoveTaskItem(),
                new RepeaterTaskItem(),
                new RepeatUntilFailTaskItem(),
                new SelectorTaskItem(),
                new SequenceTaskItem(),
                new SucceederTaskItem(),
                new TransferItemTaskItem(),
            }.Where((opt) => opt.CanBeAssignedTo(taskable)).ToList();
            

            OptionsAndTheirLocations = new BiDictionary<ITaskItem, Rectangle>();

            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { Color.Gray });

            BackgroundRect = new Rectangle(-1, -1, 1, 1);
        }

        public void CalculateSize(RenderContext context)
        {
            var widest = 0;
            var height = 5;

            foreach (var item in Options)
            {
                var str = item.TaskName;
                var strSize = context.DefaultFont.MeasureString(str);

                Rectangle rect;
                if(OptionsAndTheirLocations.TryGetByFirst(item, out rect))
                {
                    rect.X = 5;
                    rect.Y = height;
                    rect.Width = (int)strSize.X;
                    rect.Height = (int)strSize.Y;
                }else
                {
                    OptionsAndTheirLocations[item] = new Rectangle(5, height, (int)strSize.X, (int)strSize.Y);
                }

                widest = (int) Math.Max(widest, strSize.X);
                height += (int)(context.DefaultFont.LineSpacing * 1.5);
            }

            height += 5;

            Size = new PointI2D(5 + widest + 5, height);
        }

        public override void PreDraw(RenderContext renderContext)
        {
            if (Size == null)
                CalculateSize(renderContext);
        }
        public override void Draw(RenderContext context)
        {
            BackgroundRect.X = ScreenLocation.X;
            BackgroundRect.Y = ScreenLocation.Y;
            BackgroundRect.Width = Size.X;
            BackgroundRect.Height = Size.Y;

            context.SpriteBatch.Draw(BackgroundTexture, BackgroundRect, Color.White);

            foreach (var item in OptionsAndTheirLocations.GetFirstKeys())
            {
                var rect = OptionsAndTheirLocations[item];

                context.SpriteBatch.DrawString(context.DefaultFont, item.TaskName, new Vector2(rect.X, rect.Y), ReferenceEquals(item, Hovered) ? Color.White : Color.LightGray);
            }
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            var found = false;

            if (Hovered != null && OptionsAndTheirLocations[Hovered].Contains(current.Position))
                found = true;

            if (!found)
            {
                foreach (var item in OptionsAndTheirLocations.GetFirstKeys())
                {
                    if (ReferenceEquals(item, Hovered))
                        continue;

                    var rect = OptionsAndTheirLocations[item];

                    if (rect.Contains(current.Position))
                    {
                        Hovered = item;
                        found = true;
                        RedrawRequired?.Invoke(null, EventArgs.Empty);
                        break;
                    }
                }
            }

            if(!found && Hovered != null)
            {
                Hovered = null;
                RedrawRequired?.Invoke(null, EventArgs.Empty);
            }

            if(last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                if (Selected != Hovered)
                {
                    Selected = Hovered;
                    TaskSelected?.Invoke(this, EventArgs.Empty);
                }
            }

            handled = found || BackgroundRect.Contains(current.Position);
        }
        public override void Dispose()
        {
            base.Dispose();

            BackgroundTexture?.Dispose();
            BackgroundTexture = null;
        }
    }
}