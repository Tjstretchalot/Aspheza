using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class MoveTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"A move task is a leaf task that moves the entity
via an algorithm modified for any-size entities. 
The destination is where the top-left of the
entities collision box will match the top-left of
the tile.

To see grid locations, turn on the debug overlay
and the hovered grid location will be shown in 
the upper-left corner of the screen.";

        protected PointI2D OriginalDestination;

        protected Text DestinationXLabel;
        protected TextField DestinationXField;
        protected Text DestinationYLabel;
        protected TextField DestinationYField;

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public MoveTaskItem(EntityMoveTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Savable = true;
            Expandable = false;
            Expanded = false;
            TaskName = "Move";
            OriginalDestination = task.Destination;
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public MoveTaskItem()
        {
            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Savable = true;
            Expandable = false;
            Expanded = false;
            TaskName = "Move";
            OriginalDestination = new PointI2D(0, 0);
        }

        protected override void InitializeThings(RenderContext renderContext)
        {
            base.InitializeThings(renderContext);

            if (DestinationXLabel == null)
                DestinationXLabel = new Text(new Point(0, 0), "X", renderContext.DefaultFont, Color.Black);

            if (DestinationXField == null)
            {
                DestinationXField = UIUtils.CreateTextField(new Point(0, 0), new Point(65, 30));
                DestinationXField.Text = OriginalDestination.X.ToString();

                DestinationXField.FocusGained += (sender, args) => OnInspectRedrawRequired();
                DestinationXField.FocusLost += (sender, args) => OnInspectRedrawRequired();
                DestinationXField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                DestinationXField.TextChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (DestinationYLabel == null)
                DestinationYLabel = new Text(new Point(0, 0), "Y", renderContext.DefaultFont, Color.Black);

            if(DestinationYField == null)
            {
                DestinationYField = UIUtils.CreateTextField(new Point(0, 0), new Point(65, 30));
                DestinationYField.Text = OriginalDestination.Y.ToString();

                DestinationYField.FocusGained += (sender, args) => OnInspectRedrawRequired();
                DestinationYField.FocusLost += (sender, args) => OnInspectRedrawRequired();
                DestinationYField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                DestinationYField.TextChanged += (sender, args) => OnInspectRedrawRequired();
            }
        }
        
        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            var reqSpace = DestinationXField.Size.X + 5 + DestinationYField.Size.X;
            var sidePadding = (width - reqSpace) / 2;

            height += Math.Max(DestinationXLabel.Size.Y, DestinationYLabel.Size.Y) + 3;

            DestinationXField.Center = new Point(sidePadding + DestinationXField.Size.X / 2, height + DestinationXField.Size.Y / 2);
            DestinationYField.Center = new Point(width - sidePadding - DestinationYField.Size.X / 2, height + DestinationYField.Size.Y / 2);

            DestinationXLabel.Center = new Point(DestinationXField.Center.X - DestinationXField.Size.X / 2 + DestinationXLabel.Size.X / 2, DestinationXField.Center.Y - DestinationXField.Size.Y / 2 - DestinationXLabel.Size.Y / 2 - 3);
            DestinationYLabel.Center = new Point(DestinationYField.Center.X - DestinationYField.Size.X / 2 + DestinationYLabel.Size.X / 2, DestinationYField.Center.Y - DestinationYField.Size.Y / 2 - DestinationYLabel.Size.Y / 2 - 3);
            
            height += Math.Max(DestinationXField.Size.Y, DestinationYField.Size.Y);
            height += 10;
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if(DestinationXField == null || DestinationXField.Text.Length == 0 || DestinationYField.Text.Length == 0)
            {
                return new EntityMoveTask(taskable as MobileEntity, OriginalDestination);
            }

            var x = int.Parse(DestinationXField.Text);
            var y = int.Parse(DestinationYField.Text);

            return new EntityMoveTask(taskable as MobileEntity, new PointI2D(x, y));
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            base.PreDrawInspect(context, x, y);

            DestinationXField?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            DestinationYField?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            DestinationXLabel?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            DestinationYLabel?.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            base.DrawInspect(context, x, y);

            DestinationXField?.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            DestinationYField?.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            DestinationXLabel?.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            DestinationYLabel?.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        protected override void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            base.HandleInspectComponentsMouseState(last, current, ref handled, ref scrollHandled);

            DestinationXField?.HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
            DestinationYField?.HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            bool handled = base.HandleInspectKeyboardState(sharedGameState, localGameState, netContext, last, current);
            
            DestinationXField?.HandleKeyboardState(Content, last, current, ref handled);
            DestinationYField?.HandleKeyboardState(Content, last, current, ref handled);

            return handled;
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);

            DestinationXField?.Update(Content, timeMS);
            DestinationYField?.Update(Content, timeMS);
        }

        public override bool CanBeAssignedTo(ITaskable taskable)
        {
            return typeof(MobileEntity).IsAssignableFrom(taskable.GetType());
        }

        public override bool IsValid()
        {
            if (DestinationXField == null)
                return true;

            return DestinationXField.Text.Length > 0 && DestinationYField.Text.Length > 0;
        }

        public override void DisposeInspect()
        {
            base.DisposeInspect();

            DestinationXLabel?.Dispose();
            DestinationXLabel = null;

            DestinationXField?.Dispose();
            DestinationXField = null;

            DestinationYLabel?.Dispose();
            DestinationYLabel = null;

            DestinationYField?.Dispose();
            DestinationYField = null;
        }
    }
}
