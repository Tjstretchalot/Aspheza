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
using BaseBuilder.Engine.State.Resources;

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
            HasOpenSlots, Count
        }

        // These references need to be weak so when Dispose is called and Components is cleared the visuals
        // are destroyed correctly. If these were strong references we would need to override Dispose to set
        // them to null and still need nullchecks everywhere the weak references could have been lost AND 
        // if somehow a component was removed from components without a call to dispose we wouldn't get any
        // exceptions.

        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<ConditionType>>> ConditionTypeBox;
        
        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<InventoryConditionType>>> InventoryConditionTypeBox;

        protected WeakReference<TaskItemComponentFromScreenComponent<TextField>> InventoryCount_Quantity;
        protected WeakReference<TaskItemComponentFromScreenComponent<RadioButton>> InventoryCount_AtMostButton;
        protected WeakReference<TaskItemComponentFromScreenComponent<RadioButton>> InventoryCount_AtLeastButton;
        protected WeakReference<TaskItemComponentFromScreenComponent<CheckBox>> InventoryCount_TypeCheck;
        protected WeakReference<TaskItemComponentFromScreenComponent<ComboBox<Material>>> InventoryCount_TypeBox;

        public ConditionTaskItem()
        {
            Task = null;

            ConditionTypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<ConditionType>>>(null);
            
            InventoryConditionTypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<InventoryConditionType>>>(null);

            InventoryCount_Quantity = new WeakReference<TaskItemComponentFromScreenComponent<TextField>>(null);
            InventoryCount_AtMostButton = new WeakReference<TaskItemComponentFromScreenComponent<RadioButton>>(null);
            InventoryCount_AtLeastButton = new WeakReference<TaskItemComponentFromScreenComponent<RadioButton>>(null);
            InventoryCount_TypeCheck = new WeakReference<TaskItemComponentFromScreenComponent<CheckBox>>(null);
            InventoryCount_TypeBox = new WeakReference<TaskItemComponentFromScreenComponent<ComboBox<Material>>>(null);

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

            int parsed;
            Material material;
            switch (typeBox.Selected.Value)
            {
                case ConditionType.Inventory:
                    var invBox = Unwrap(InventoryConditionTypeBox);
                    if (invBox.Selected == null)
                        return result;

                    switch (invBox.Selected.Value)
                    {
                        case InventoryConditionType.HasOpenSlots:
                            result.Conditional = new EntityInventoryHasOpenSlotCondition();
                            break;
                        case InventoryConditionType.Count:
                            if (!int.TryParse(Unwrap(InventoryCount_Quantity).Text, out parsed))
                                parsed = 0;

                            material = null;
                            if(Unwrap(InventoryCount_TypeCheck).Pushed)
                            {
                                var box = Unwrap(InventoryCount_TypeBox);
                                if (box.Selected != null)
                                    material = box.Selected.Value;
                            }
                            result.Conditional = new EntityInventoryCountCondition(parsed, Unwrap(InventoryCount_AtMostButton).Pushed, material);
                            break;
                    }
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

            int parsed;
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
                        case InventoryConditionType.Count:
                            if (!Unwrap(InventoryCount_AtLeastButton).Pushed && !Unwrap(InventoryCount_AtMostButton).Pushed)
                                return false;

                            var field = Unwrap(InventoryCount_Quantity);
                            if (field.Text.Length == 0 || !int.TryParse(field.Text, out parsed))
                                return false;

                            if (parsed <= 0)
                                return false;

                            if (Unwrap(InventoryCount_TypeCheck).Pushed && Unwrap(InventoryCount_TypeBox).Selected == null)
                                return false;

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
            }else if(task.Conditional.GetType() == typeof(EntityInventoryCountCondition))
            {
                var conditional = (EntityInventoryCountCondition)task.Conditional;
                var typeBox = Unwrap(ConditionTypeBox);
                typeBox.Selected = typeBox.GetComboItemByValue(ConditionType.Inventory);

                var invBox = Unwrap(InventoryConditionTypeBox);
                invBox.Selected = invBox.GetComboItemByValue(InventoryConditionType.Count);

                if (conditional.AtMost)
                    Unwrap(InventoryCount_AtMostButton).Pushed = true;
                else
                    Unwrap(InventoryCount_AtLeastButton).Pushed = true;

                Unwrap(InventoryCount_Quantity).Text = conditional.Quantity.ToString();
                
                if(conditional.Material != null)
                {
                    Unwrap(InventoryCount_TypeCheck).Pushed = true;

                    var box = Unwrap(InventoryCount_TypeBox);
                    box.Selected = box.GetComboItemByValue(conditional.Material);
                }
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

        protected void InitializeInventoryConditions(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main, ComboBox<ConditionType> mainBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 3);

            var typeBox = CreateComboBox(context, redraw, redrawAndReload, 
                Tuple.Create("Has Open Slots", InventoryConditionType.HasOpenSlots),
                Tuple.Create("Count", InventoryConditionType.Count));

            var wrapped = Wrap(typeBox);
            InventoryConditionTypeBox.SetTarget(wrapped);
            layout.Children.Add(Label(context, "Inventory Condition", wrapped));

            SetupComboBoxHiddenToggle(mainBox, ConditionType.Inventory, layout);

            layout.Hidden = true;
            main.Children.Add(layout);

            InitializeInventoryCountCondition(context, redraw, redrawAndReload, main, mainBox, layout, typeBox);
        }

        protected void InitializeInventoryCountCondition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main, ComboBox<ConditionType> mainBox, TaskItemComponentAsLayoutManager inventory, ComboBox<InventoryConditionType> inventoryBox)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);
            layout.Children.Add(CreatePadding(2, 2));

            var atMost = CreateRadioButton(context, redraw, redrawAndReload);
            var atLeast = CreateRadioButton(context, redraw, redrawAndReload);
            GroupRadioButtons(context, redraw, redrawAndReload, atMost, atLeast);

            var atMostWrapped = Wrap(atMost);
            var atLeastWrapped = Wrap(atLeast);
            InventoryCount_AtMostButton.SetTarget(atMostWrapped);
            InventoryCount_AtLeastButton.SetTarget(atLeastWrapped);

            var buttonsLayout = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
            buttonsLayout.Children.Add(Label(context, "At Most", atMostWrapped, false));
            buttonsLayout.Children.Add(Label(context, "At Least", atLeastWrapped, false));
            layout.Children.Add(buttonsLayout);

            SetupComboBoxHiddenToggle(inventoryBox, InventoryConditionType.Count, layout);

            var quantity = CreateTextField(context, redraw, redrawAndReload);
            quantity.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);

            var quantityWrapped = Wrap(quantity);
            InventoryCount_Quantity.SetTarget(quantityWrapped);
            layout.Children.Add(Label(context, "Quantity", quantityWrapped));

            layout.Children.Add(CreatePadding(2, 3));
            var typeCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var typeCheckWrapped = Wrap(typeCheck);
            InventoryCount_TypeCheck.SetTarget(typeCheckWrapped);
            layout.Children.Add(Label(context, "Specific Material", typeCheckWrapped, false));

            var typeBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var typeBoxWrapped = Wrap(typeBox);
            InventoryCount_TypeBox.SetTarget(typeBoxWrapped);
            var typeBoxAll = Label(context, "Material", typeBoxWrapped);
            typeBoxAll.Hidden = true;
            SetupRadioButtonHiddenToggle(typeCheck, typeBoxAll);
            layout.Children.Add(typeBoxAll);

            layout.Hidden = true;
            inventory.Children.Add(layout);
        }
    }
}
