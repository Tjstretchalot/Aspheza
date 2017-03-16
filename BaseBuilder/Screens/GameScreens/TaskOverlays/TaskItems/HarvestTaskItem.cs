﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems;
using BaseBuilder.Screens.Components;

using static BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems.ComplexTaskItemUtils;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public class HarvestTaskItem : ComplexTaskItem
    {
        protected enum TargetType
        {
            ByID,
            ByPosition,
            ByRelativePosition
        }

        const string _InspectDescription = @"Harvests something, such as a tree, farm, or production
building. Can harvest by entity id, position, or relative
position. ";

        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<TargetType>>> TargetTypeBox;

        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> TargetByID_Field;

        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> TargetByPosition_XField;
        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> TargetByPosition_YField;

        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> TargetByRelativePosition_DXField;
        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> TargetByRelativePosition_DYField;

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public HarvestTaskItem(EntityHarvestTask task) : this()
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
        }

        /// <summary>
        /// Creates a default version of this task item
        /// </summary>
        public HarvestTaskItem()
        {
            TargetTypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<TargetType>>>(null);
            TargetByID_Field = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);
            TargetByPosition_XField = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);
            TargetByPosition_YField = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);
            TargetByRelativePosition_DXField = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);
            TargetByRelativePosition_DYField = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);

            Children = new List<ITaskItem>();
            InspectDescription = _InspectDescription;
            Savable = true;
            Expandable = false;
            Expanded = false;
            TaskName = "Harvest";
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return new EntityHarvestTask();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return false;
        }

        protected override void InitializeComponent(RenderContext context)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredWidth, 5);

            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var box = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By ID", TargetType.ByID),
                Tuple.Create("By Position", TargetType.ByPosition),
                Tuple.Create("By Relative Position", TargetType.ByRelativePosition));

            var wrapped = Wrap(box);
            TargetTypeBox.SetTarget(wrapped);

            layout.Children.Add(Label(context, "Targeting Type", wrapped));
            InitializeTargetByID(context, redraw, redrawAndReload, layout, box);
            InitializeTargetByPosition(context, redraw, redrawAndReload, layout, box);
            InitializeTargetByRelativePosition(context, redraw, redrawAndReload, layout, box);
            InspectComponent = layout;
        }

        protected void InitializeTargetByID(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            TaskItemComponentAsLayoutManager main, ComboBox<TargetType> targetBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var idField = CreateTextField(context, redraw, redrawAndReload);
            idField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            var wrapped = Wrap(idField);
            TargetByID_Field.SetTarget(wrapped);
            layout.Children.Add(Label(context, "ID", wrapped));

            SetupComboBoxHiddenToggle(targetBox, TargetType.ByID, layout);

            layout.Hidden = true;
            main.Children.Add(layout);
        }

        protected void InitializeTargetByPosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, 
            TaskItemComponentAsLayoutManager main, ComboBox<TargetType> targetBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var xyFields = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var xField = CreateTextField(context, redraw, redrawAndReload);
            xField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            var wrapped = Wrap(xField);
            TargetByPosition_XField.SetTarget(wrapped);
            xyFields.Children.Add(Label(context, "X", wrapped));

            var yField = CreateTextField(context, redraw, redrawAndReload);
            yField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            wrapped = Wrap(yField);
            TargetByPosition_YField.SetTarget(wrapped);
            xyFields.Children.Add(Label(context, "Y", wrapped));

            layout.Children.Add(xyFields);

            SetupComboBoxHiddenToggle(targetBox, TargetType.ByPosition, layout);

            layout.Hidden = true;
            main.Children.Add(layout);
        }

        protected void InitializeTargetByRelativePosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            TaskItemComponentAsLayoutManager main, ComboBox<TargetType> targetBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);


            var dxdyFields = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var dxField = CreateTextField(context, redraw, redrawAndReload);
            dxField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            var wrapped = Wrap(dxField);
            TargetByRelativePosition_DXField.SetTarget(wrapped);
            dxdyFields.Children.Add(Label(context, "Delta X", wrapped));

            var dyField = CreateTextField(context, redraw, redrawAndReload);
            dyField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            wrapped = Wrap(dyField);
            TargetByRelativePosition_DYField.SetTarget(wrapped);
            dxdyFields.Children.Add(Label(context, "Delta Y", wrapped));

            layout.Children.Add(dxdyFields);

            SetupComboBoxHiddenToggle(targetBox, TargetType.ByRelativePosition, layout);

            layout.Hidden = true;
            main.Children.Add(layout);
        }

        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
            height += 50;
        }

        protected override void LoadFromTask()
        {
            var harvestTask = Task as EntityHarvestTask;

            if (harvestTask.HarvestedTargeter != null)
            {
                var targetTypeBox = Unwrap(TargetTypeBox);
                if(harvestTask.HarvestedTargeter.GetType() == typeof(TransferTargetByID))
                {
                    var targeter = harvestTask.HarvestedTargeter as TransferTargetByID;

                    targetTypeBox.Selected = targetTypeBox.GetComboItemByValue(TargetType.ByID);

                    Unwrap(TargetByID_Field).Text = targeter.TargetID.ToString();
                }else if(harvestTask.HarvestedTargeter.GetType() == typeof(TransferTargetByPosition))
                {
                    var targeter = harvestTask.HarvestedTargeter as TransferTargetByPosition;

                    targetTypeBox.Selected = targetTypeBox.GetComboItemByValue(TargetType.ByPosition);

                    Unwrap(TargetByPosition_XField).Text = targeter.Position.X.ToString();
                    Unwrap(TargetByPosition_YField).Text = targeter.Position.Y.ToString();
                }else if(harvestTask.HarvestedTargeter.GetType() == typeof(TransferTargetByRelativePosition))
                {
                    var targeter = harvestTask.HarvestedTargeter as TransferTargetByRelativePosition;

                    targetTypeBox.Selected = targetTypeBox.GetComboItemByValue(TargetType.ByRelativePosition);

                    Unwrap(TargetByRelativePosition_DXField).Text = ((int)targeter.Offset.DeltaX).ToString();
                    Unwrap(TargetByRelativePosition_DYField).Text = ((int)targeter.Offset.DeltaY).ToString();
                }else
                {
                    throw new InvalidProgramException();
                }
            }
        }
    }
}
