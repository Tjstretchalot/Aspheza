using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;

using static BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems.ComplexTaskItemUtils;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals.InventoryConditionals;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// This task item allows building of EntityConditionTasks. They are grouped together
    /// by type.
    /// </summary>
    public class ConditionTaskItem : ComplexTaskItem
    {
        protected const string _InspectDescription = @"A condition task is a leaf task that returns success
if a condition is met and failure otherwise. A 
condition task takes some time, typically one second.";

        protected enum ConditionType
        {
            Inventory
        }

        protected enum InventoryConditionType
        {
            HasOpenSlots
        }

        // These references need to be weak so when Dispose is called and Components is cleared the visuals
        // are destroyed correctly. If these were strong references we would need to override Dispose to set
        // them to null and still need nullchecks everywhere the weak references could have been lost AND 
        // if somehow a component was removed from components without a call to dispose we wouldn't get any
        // exceptions.

        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<ConditionType>>> ConditionTypeBox;
        
        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<InventoryConditionType>>> InventoryConditionTypeBox;

        public ConditionTaskItem()
        {
            Task = null;

            ConditionTypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<ConditionType>>>(null);
            
            InventoryConditionTypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<InventoryConditionType>>>(null);

            Children = new List<ITaskItem>();
            TaskName = "Condition";
            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            Savable = true;
        }

        public ConditionTaskItem(IEntityTask task) : this()
        {
            Task = task;

        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var visualsDisposed = false;

            TaskItemComponentFromScreenComponent<ComboBox<ConditionType>> typeBoxWrapped;
            if (!ConditionTypeBox.TryGetTarget(out typeBoxWrapped))
                visualsDisposed = true;
            else if (typeBoxWrapped.Disposed)
                visualsDisposed = true;

            if (visualsDisposed)
            {
                if(Task != null)
                {
                    var oldTask = Task as EntityConditionTask;
                    return new EntityConditionTask(((Thing)taskable).ID, oldTask.Conditional, oldTask.TotalTimeMS);
                }

                return new EntityConditionTask(((Thing)taskable).ID, null);
            }

            var result = new EntityConditionTask(((Thing)taskable).ID, null);
            var typeBox = Unwrap(typeBoxWrapped);
            if (typeBox.Selected == null)
                return result;

            switch(typeBox.Selected.Value)
            {
                case ConditionType.Inventory:
                    var invBox = Unwrap(InventoryConditionTypeBox);
                    if (invBox.Selected == null)
                        return result;

                    result.Conditional = new EntityInventoryHasOpenSlotCondition();
                    return result;
                default:
                    throw new InvalidProgramException();
            }
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var visualsDisposed = false;

            TaskItemComponentFromScreenComponent<ComboBox<ConditionType>> typeBoxWrapped;
            if (!ConditionTypeBox.TryGetTarget(out typeBoxWrapped))
                visualsDisposed = true;
            else if (typeBoxWrapped.Disposed)
                visualsDisposed = true;

            if (visualsDisposed)
            {
                if (Task == null)
                    return false;
                return Task.IsValid();
            }


            var typeBox = typeBoxWrapped.Component;
            if (typeBox.Selected == null)
                return false;

            switch(typeBox.Selected.Value)
            {
                case ConditionType.Inventory:
                    var invBox = Unwrap(InventoryConditionTypeBox);
                    if (invBox.Selected == null)
                        return false;

                    switch(invBox.Selected.Value)
                    {
                        case InventoryConditionType.HasOpenSlots:
                            break;
                        default:
                            throw new InvalidProgramException();
                    }
                    break;
                default:
                    throw new InvalidProgramException();
            }

            return true;
        }

        protected override void LoadFromTask()
        {
            var task = Task as EntityConditionTask;
            if (task.Conditional == null)
                return;

            if(task.Conditional.GetType() == typeof(EntityInventoryHasOpenSlotCondition))
            {
                var typeBox = Unwrap(ConditionTypeBox);
                typeBox.Selected = typeBox.GetComboItemByValue(ConditionType.Inventory);

                var invBox = Unwrap(InventoryConditionTypeBox);
                invBox.Selected = invBox.GetComboItemByValue(InventoryConditionType.HasOpenSlots);
            }
        }

        protected override void InitializeComponent(RenderContext context)
        {
            if (InspectComponent != null)
                return;

            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var main = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredWidth, 5);

            var typeBox = CreateComboBox(context, redraw, redrawAndReload, Tuple.Create("Inventory", ConditionType.Inventory));

            var wrapped = Wrap(typeBox);
            ConditionTypeBox.SetTarget(wrapped);
            main.Children.Add(Label(context, "Condition Type", wrapped));

            InitializeInventoryConditions(context, redraw, redrawAndReload, main, typeBox);

            InspectComponent = main;
        }

        protected virtual void InitializeInventoryConditions(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main, ComboBox<ConditionType> mainBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 3);

            var typeBox = CreateComboBox(context, redraw, redrawAndReload, Tuple.Create("Has Open Slots", InventoryConditionType.HasOpenSlots));
            var wrapped = Wrap(typeBox);
            InventoryConditionTypeBox.SetTarget(wrapped);
            layout.Children.Add(Label(context, "Inventory Condition", wrapped));

            SetupComboBoxHiddenToggle(mainBox, ConditionType.Inventory, layout);

            layout.Hidden = true;
            main.Children.Add(layout);
        }
    }
}
