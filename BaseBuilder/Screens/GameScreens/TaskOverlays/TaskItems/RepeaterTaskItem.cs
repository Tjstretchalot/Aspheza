﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class RepeaterTaskItem : SimpleTaskItem
    {
        const string _InspectDescription = @"A repeater repeats one child. A repeater can either
repeat forever or repeat a certain number of times.";

        protected CheckBox RepeatForeverCheckbox;
        protected Text RepeatForeverLabel;

        protected TextField TimesTextField;
        protected Text TimesLabel;

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public RepeaterTaskItem(EntityRepeaterTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>(1);
            if (task.Child != null)
            {
                var child = TaskItemIdentifier.Init(task.Child);
                child.Parent = this;
                Children.Add(child);
            }

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Repeat";
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public RepeaterTaskItem()
        {
            Children = new List<ITaskItem>(1);

            InspectDescription = _InspectDescription;
            Expandable = true;
            Expanded = false;
            TaskName = "Repeat";
        }

        protected override void InitializeThings(RenderContext renderContext)
        {
            base.InitializeThings(renderContext);

            if(RepeatForeverCheckbox == null)
            {
                RepeatForeverCheckbox = new CheckBox(new Point(0, 0));

                RepeatForeverCheckbox.PushedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if(RepeatForeverLabel == null)
            {
                RepeatForeverLabel = new Text(new Point(0, 0), "Repeat Forever?", renderContext.DefaultFont, Color.Black);
            }

            if(TimesTextField == null)
            {
                TimesTextField = UIUtils.CreateTextField(new Point(0, 0), new Point(100, 30));

                TimesTextField.TextChanged += (sender, args) => OnInspectRedrawRequired();
                TimesTextField.CaretToggled += (sender, args) => OnInspectRedrawRequired();
                TimesTextField.FocusGained += (sender, args) => OnInspectRedrawRequired();
                TimesTextField.FocusLost += (sender, args) => OnInspectRedrawRequired();
            }

            if(TimesLabel == null)
            {
                TimesLabel = new Text(new Point(0, 0), "Times", renderContext.DefaultFont, Color.Black);
            }
        }

        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {

            var trueWidth = RepeatForeverCheckbox.Size.X + 3 + RepeatForeverLabel.Size.X;
            var shiftToCenter = width / 2 - trueWidth / 2;

            RepeatForeverCheckbox.Center = new Point(shiftToCenter + RepeatForeverCheckbox.Size.X/2, height + RepeatForeverCheckbox.Size.Y / 2);
            height += RepeatForeverCheckbox.Size.Y + 3;

            RepeatForeverLabel.Center = new Point(RepeatForeverCheckbox.Center.X + RepeatForeverCheckbox.Size.X / 2 + 3 + RepeatForeverLabel.Size.X / 2, RepeatForeverCheckbox.Center.Y);
            
            height += TimesLabel.Size.Y + 2;
            TimesTextField.Center = new Point(width / 2, height + TimesTextField.Size.Y / 2);
            height += TimesTextField.Size.Y + 3;

            TimesLabel.Center = new Point(TimesTextField.Center.X - TimesTextField.Size.X/2 + TimesLabel.Size.X / 2, TimesTextField.Center.Y - TimesTextField.Size.Y / 2 - TimesLabel.Size.Y / 2 - 2);

            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            base.PreDrawInspect(context, x, y);

            RepeatForeverLabel.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            RepeatForeverCheckbox.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            TimesLabel.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            TimesTextField.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            if(ButtonShiftLast.X != x || ButtonShiftLast.Y != y)
            {
                RepeatForeverCheckbox.Center = new Point(RepeatForeverCheckbox.Center.X - ButtonShiftLast.X + x, RepeatForeverCheckbox.Center.Y - ButtonShiftLast.Y + y);
                TimesTextField.Center = new Point(TimesTextField.Center.X - ButtonShiftLast.X + x, TimesTextField.Center.Y - ButtonShiftLast.Y + y);
            }

            base.DrawInspect(context, x, y);

            RepeatForeverLabel.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            RepeatForeverCheckbox.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            TimesLabel.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            TimesTextField.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
        }

        public override bool HandleInspectMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            bool handled = base.HandleInspectMouseState(sharedGameState, localGameState, netContext, last, current);

            RepeatForeverCheckbox.HandleMouseState(Content, last, current, ref handled);
            TimesTextField.HandleMouseState(Content, last, current, ref handled);

            return handled;
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            bool handled = base.HandleInspectKeyboardState(sharedGameState, localGameState, netContext, last, current);

            TimesTextField.HandleKeyboardState(Content, last, current, ref handled);

            return handled;
        }
        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);

            RepeatForeverCheckbox.Update(Content, timeMS);
            TimesTextField.Update(Content, timeMS);
        }

        public override IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if (Children.Count == 0)
                return new EntityRepeaterTask(null, "none");

            var childTask = Children[0].CreateEntityTask(sharedState, localState, netContext);

            return new EntityRepeaterTask(childTask, childTask.GetType().Name);
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 1 && Children[0].IsValid(sharedState, localState, netContext);
        }
    }
}
