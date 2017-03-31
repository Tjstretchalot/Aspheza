using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components.ScrollableComponents;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Screens.Components;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// Corresponds with EntityAidTask
    /// </summary>
    public class AidTaskItem : ComplexTaskItem
    {
        protected enum TargetMethod
        {
            ByID,
            ByPosition,
            ByRelativePosition
        }

        protected const string _InspectDescription = @"An aid task allows a worker to assist something, 
without directly affecting the worker. Most 
commonly this is used to build things.

This will return failure if it cannot find an 
aidable target using the conditions specified.
If it can find the target and it is sometimes
aidable but is not currently in need of aid,
this returns success. Otherwise, this returns
running.";

        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<TargetMethod>>> TargeterBox;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByID_Field;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByPosition_XField;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByPosition_YField;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByRelPosition_DXField;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByRelPosition_DYField;

        public AidTaskItem()
        {
            Children = new List<ITaskItem>();
            TaskName = "Aid";
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            Savable = true;

            TargeterBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<TargetMethod>>>(null);
            TargetByID_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByPosition_XField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByPosition_YField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByRelPosition_DXField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByRelPosition_DYField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
        }

        public AidTaskItem(EntityAidTask task) : this()
        {
            Task = task;
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var id = ((Thing)taskable).ID;
            ScrollableComponentFromScreenComponent<ComboBox<TargetMethod>> targeterBoxWrapped;
            if (!TryGetWrapped(TargeterBox, out targeterBoxWrapped))
                return Task == null ? new EntityAidTask(id, null) : new EntityAidTask(id, ((EntityAidTask)Task).AidingTargeter);

            var targeterBox = targeterBoxWrapped.Component;

            if (targeterBox.Selected == null)
                return new EntityAidTask(id, null);

            TextField field;
            int parsed1, parsed2;
            switch(targeterBox.Selected.Value)
            {
                case TargetMethod.ByID:
                    field = Unwrap(MakeStrong(TargetByID_Field));
                    if (!int.TryParse(field.Text, out parsed1))
                        return new EntityAidTask(id, new TransferTargetByID(0));

                    return new EntityAidTask(id, new TransferTargetByID(parsed1));
                case TargetMethod.ByPosition:
                    field = Unwrap(MakeStrong(TargetByPosition_XField));
                    if (!int.TryParse(field.Text, out parsed1))
                        return new EntityAidTask(id, new TransferTargetByPosition(new PointI2D(0, 0)));
                    field = Unwrap(MakeStrong(TargetByPosition_YField));
                    if (!int.TryParse(field.Text, out parsed2))
                        return new EntityAidTask(id, new TransferTargetByPosition(new PointI2D(parsed1, 0)));

                    return new EntityAidTask(id, new TransferTargetByPosition(new PointI2D(parsed1, parsed2)));
                case TargetMethod.ByRelativePosition:
                    field = Unwrap(MakeStrong(TargetByRelPosition_DXField));
                    if (!int.TryParse(field.Text, out parsed1))
                        return new EntityAidTask(id, new TransferTargetByRelativePosition(new VectorD2D(1, 0)));
                    field = Unwrap(MakeStrong(TargetByRelPosition_DYField));
                    if (!int.TryParse(field.Text, out parsed2))
                        return new EntityAidTask(id, new TransferTargetByRelativePosition(new VectorD2D(parsed1, parsed1 == 0 ? 1 : 0)));

                    return new EntityAidTask(id, new TransferTargetByRelativePosition(new VectorD2D(parsed1, (parsed1 == 0 && parsed2 == 0) ? 1 : parsed2)));
                default:
                    throw new InvalidProgramException("Shouldn't get here");
            }
        }

        public override bool IsValid()
        {
            ScrollableComponentFromScreenComponent<ComboBox<TargetMethod>> targeterBoxWrapped;
            if (!TryGetWrapped(TargeterBox, out targeterBoxWrapped))
                return Task == null ? false : Task.IsValid();

            var targeterBox = targeterBoxWrapped.Component;

            if (targeterBox.Selected == null)
                return false;

            int parsed;
            TextField field;
            switch(targeterBox.Selected.Value)
            {
                case TargetMethod.ByID:
                    field = Unwrap(MakeStrong(TargetByID_Field));
                    if (!int.TryParse(field.Text, out parsed))
                        return false;
                    break;
                case TargetMethod.ByPosition:
                    field = Unwrap(MakeStrong(TargetByPosition_XField));
                    if (!int.TryParse(field.Text, out parsed))
                        return false;
                    field = Unwrap(MakeStrong(TargetByPosition_YField));
                    if (!int.TryParse(field.Text, out parsed))
                        return false;
                    break;
                case TargetMethod.ByRelativePosition:
                    field = Unwrap(MakeStrong(TargetByRelPosition_DXField));
                    if (!int.TryParse(field.Text, out parsed))
                        return false;
                    field = Unwrap(MakeStrong(TargetByRelPosition_DYField));
                    if (!int.TryParse(field.Text, out parsed))
                        return false;
                    break;
            }

            return true;
        }

        protected override IScrollableComponent InitializeComponent(RenderContext context)
        {
            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var result = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredWidth, 7);

            var targeterBox = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By ID", TargetMethod.ByID),
                Tuple.Create("By Position", TargetMethod.ByPosition),
                Tuple.Create("By Relative Position", TargetMethod.ByRelativePosition));
            var wrappedBox = Wrap(targeterBox);
            TargeterBox.SetTarget(wrappedBox);

            result.Children.Add(Label(context, "Targeting Method", wrappedBox));

            AddByIDTargeter(context, redraw, redrawAndReload, result, targeterBox);
            AddByPositionTargeter(context, redraw, redrawAndReload, result, targeterBox);
            AddByRelativePositionTargeter(context, redraw, redrawAndReload, result, targeterBox);

            return result;
        }

        protected void AddByIDTargeter(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetMethod> targeterBox)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 7);

            var idField = CreateTextField(context, redraw, redrawAndReload);
            idField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            var fieldWrapped = Wrap(idField);
            TargetByID_Field.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "ID", fieldWrapped));

            SetupComboBoxHiddenToggle(targeterBox, TargetMethod.ByID, layout);
            parent.Children.Add(layout);
        }

        protected void AddByPositionTargeter(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetMethod> targeterBox)
        {
            var layout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var xField = CreateTextField(context, redraw, redrawAndReload, 90);
            xField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            var fieldWrapped = Wrap(xField);
            TargetByPosition_XField.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "X", fieldWrapped));

            var yField = CreateTextField(context, redraw, redrawAndReload, 90);
            fieldWrapped = Wrap(yField);
            TargetByPosition_YField.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "Y", fieldWrapped));

            SetupComboBoxHiddenToggle(targeterBox, TargetMethod.ByPosition, layout);
            parent.Children.Add(layout);
        }

        protected void AddByRelativePositionTargeter(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetMethod> targeterBox)
        {
            var layout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var xField = CreateTextField(context, redraw, redrawAndReload, 90);
            xField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            var fieldWrapped = Wrap(xField);
            TargetByRelPosition_DXField.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "Delta X", fieldWrapped));

            var yField = CreateTextField(context, redraw, redrawAndReload, 90);
            yField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            fieldWrapped = Wrap(yField);
            TargetByRelPosition_DYField.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "Delta Y", fieldWrapped));

            SetupComboBoxHiddenToggle(targeterBox, TargetMethod.ByRelativePosition, layout);
            parent.Children.Add(layout);
        }

        protected override void LoadFromTask(RenderContext context)
        {
            if (Task == null)
                return;

            var task = Task as EntityAidTask;

            if (task.AidingTargeter == null)
                return;

            var targeterBox = Unwrap(MakeStrong(TargeterBox));
            var targeterType = task.AidingTargeter.GetType();
            if(typeof(TransferTargetByID).IsAssignableFrom(targeterType))
            {
                var targeter = (TransferTargetByID)task.AidingTargeter;
                targeterBox.Selected = targeterBox.GetComboItemByValue(TargetMethod.ByID);

                Unwrap(MakeStrong(TargetByID_Field)).Text = targeter.TargetID.ToString();
            }else if(typeof(TransferTargetByPosition).IsAssignableFrom(targeterType))
            {
                var targeter = (TransferTargetByPosition)task.AidingTargeter;
                targeterBox.Selected = targeterBox.GetComboItemByValue(TargetMethod.ByPosition);

                Unwrap(MakeStrong(TargetByPosition_XField)).Text = targeter.Position.X.ToString();
                Unwrap(MakeStrong(TargetByPosition_YField)).Text = targeter.Position.Y.ToString();
            }else if(typeof(TransferTargetByRelativePosition).IsAssignableFrom(targeterType))
            {
                var targeter = (TransferTargetByRelativePosition)task.AidingTargeter;
                targeterBox.Selected = targeterBox.GetComboItemByValue(TargetMethod.ByRelativePosition);

                Unwrap(MakeStrong(TargetByRelPosition_DXField)).Text = ((int)targeter.Offset.DeltaX).ToString();
                Unwrap(MakeStrong(TargetByRelPosition_DYField)).Text = ((int)targeter.Offset.DeltaY).ToString();
            }else
            {
                throw new InvalidProgramException($"Shouldn't get here! Unknown targeter type {targeterType.FullName}");
            }
        }

        public override bool CanBeAssignedTo(ITaskable taskable)
        {
            if (!base.CanBeAssignedTo(taskable))
                return false;

            return (taskable as MobileEntity) != null;
        }
    }
}
