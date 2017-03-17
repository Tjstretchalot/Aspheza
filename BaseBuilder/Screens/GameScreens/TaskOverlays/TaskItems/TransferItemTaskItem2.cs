using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;
using static BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems.ComplexTaskItemUtils;
using BaseBuilder.Screens.Components;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /*
     * Interface Overview:
     *   1. Choosing the transferer:
     *     Radio buttons for pickup/dropdown. Initially both radio buttons are unselected
     *   
     *   2. Choosing the Target Decider
     *     Dropdown, defaulting to target by id
     *       Target By ID
     *         Chooses a target by its ID, which can be found by selecting the entity
     *         Textbox for ID 
     *       Target By Position
     *         Chooses a target by its Position, which can be found by selecting the entity
     *         Textbox for x
     *         Textbox for y
     *       Target By Relative Position - choose a target by relative position
     *         Choose a target by its position relative to the top-left of this entity. Down is positive y,
     *         Right is positive x.
     *         Textbox for dx
     *         Textbox for dy
     *     
     *   3. Choosing the Transfer Restrictors
     *     for each transfer restrictor:
     *       Dropdown
     *         By Item Type
     *           Restrict what type of material you will transfer.
     *           Radio buttons for allow or deny material
     *           Dropbox of Materials - Images and Text!
     *           button Delete
     *         By Total Quantity
     *           Restrict by maximum amount to transfer
     *           Textbox for maximum amount
     *           button Delete
     *         By Receiving Inventory
     *           Restrict the transfer based on how many items would be in the targets
     *           inventory (optionally of a specific type) after the transfer
     *           Checkbox for type check
     *             Dropdown for material
     *           if pickup, the label for this textbox is Minimum. If dropoff, the label for this textbox is Maximum
     *           Textbox
     *           button Delete
     *         By Our Inventory
     *           Restrict the transfer based on how many items would be in our 
     *           inventory (optionally of a specific type) after the transfer
     *           Checkbox for type check
     *             Dropdown for material
     *           if pickup, the label for this textbox is Maximum. If dropoff, the label for this textbox is Minimum
     *           Textbox
     *           button Delete
     *     button Add Restrictor
     *     
     *   4. Choosing the result decider
     *     Dropdown
     *       Our Items
     *         Succeed when we have a certain number of items
     *         Checkbox for type check
     *           Dropdown for material
     *         if pickup, the label for this textbox is At least. If dropoff, the label for this textbox is At most
     *         Textbox
     *       Their Items
     *         Succeed when the target has a certain number of items
     *         Checkbox for type check
     *           Dropdown for material
     *         If pickup, the label for this textbox is At most. If dropoff, the label for this textbox is At least
     *         Textbox
     *       Items Transferred
     *         Succeed after a certain number of items have been transferred
     *         Checkbox for type check
     *           Dropdown for material
     *         Items transferred textbox
     *     
     */

    /// <summary>
    /// Allows the user to transfer items. As for all complex task items, all references
    /// are weak to allow the complex task item to call dispose on components without us
    /// having to null 6000 different components. Without weak references, adding a component
    /// anywhere requires remembering to null it, and checking if we missed any requires
    /// looking through a ludicrously large Dispose function. Furthermore, weak references
    /// help ensure the program crashes and burns if an item is removed from Components
    /// when we don't expect it to be.
    /// </summary>
    public class TransferItemTaskItem2 : ComplexTaskItem
    {
        protected enum TargetDeciderType
        {
            ByID,
            ByPosition,
            ByRelativePosition
        }

        protected enum RestrictorType
        {
            ByItemType,
            ByTotalQuantity,
            ByReceivingInventory,
            ByDeliveringInventory
        }
        
        protected enum ResultDeciderType
        {
            OurItems,
            TheirItems,
            ItemsTransferred
        }

        protected class TransferRestrictorComponent
        {
            public TransferItemTaskItem2 Outer;
            public ITransferRestrictor Original;

            public WeakReference<ITaskItemComponent> Component;

            public TransferRestrictorComponent(TransferItemTaskItem2 outer, ITransferRestrictor restrictor = null)
            {
                Outer = outer;
                Original = restrictor;

                Component = new WeakReference<ITaskItemComponent>(null);
            }

            public void InitializeComponent(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
            {
                var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var box = CreateComboBox(context, redraw, redrawAndReload,
                    Tuple.Create("By Item Type", RestrictorType.ByItemType),
                    Tuple.Create("By Total Quantity", RestrictorType.ByTotalQuantity),
                    Tuple.Create("By Receiving Inventory", RestrictorType.ByReceivingInventory),
                    Tuple.Create("By Delivering Inventory", RestrictorType.ByDeliveringInventory));
                var boxWrapped = Wrap(box);
                layout.Children.Add(Label(context, "Restriction Type", boxWrapped));

                InitItemTypeRestriction(context, redraw, redrawAndReload, layout, box);
                InitTotalQuantityRestriction(context, redraw, redrawAndReload, layout, box);
                InitReceivingRestriction(context, redraw, redrawAndReload, layout, box);
                InitDeliveringInventory(context, redraw, redrawAndReload, layout, box);

                var deleteButton = CreateButton(context, redraw, redrawAndReload, "Delete Restriction", UIUtils.ButtonColor.Yellow);
                deleteButton.PressReleased += (sender, args) =>
                {
                    Outer.RestrictorIndexQueuedToRemove = Outer.Restrictors.FindIndex((r) => ReferenceEquals(r, this));
                };
                layout.Children.Add(Wrap(deleteButton));

                Component.SetTarget(layout);
            }
            
            public void InitItemTypeRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer by either denying a
specific material or denying everything 
except a specific material in a transfer.

This is the most common restriction and is
useful whenever you have multiple item types
in your inventory or the targets inventory.");
                layout.Children.Add(Wrap(description));

                var buttonsLayout = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
                var radio = CreateRadioButton(context, redraw, redrawAndReload);
                var radioWrapped = Wrap(radio);
                buttonsLayout.Children.Add(Label(context, "Deny Everything Else", radioWrapped, false));

                radio = CreateRadioButton(context, redraw, redrawAndReload);
                radioWrapped = Wrap(radio);
                buttonsLayout.Children.Add(Label(context, "Deny Just This", radioWrapped, false));

                layout.Children.Add(buttonsLayout);

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var wrappedBox = Wrap(matBox);
                layout.Children.Add(Label(context, "Material", wrappedBox));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByItemType, layout);
                parent.Children.Add(layout);
            }

            public void InitTotalQuantityRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer by not transferring 
more than a specific amount. 

This is useful if you either know exactly 
what items should be in your inventory at 
this point and want to utilize the order 
that items are given out, or you are picking
up from a larger container.");
                layout.Children.Add(Wrap(description));

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                layout.Children.Add(Label(context, "Amount", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByTotalQuantity, layout);
                parent.Children.Add(layout);
            }

            public void InitReceivingRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer based on what 
would be in the receiving inventory
after the transfer.

This is useful if you want to avoid
one inventory consuming all of a 
specific type of item.");
                layout.Children.Add(Wrap(description));

                var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
                var checkWrapped = Wrap(materialCheck);
                layout.Children.Add(Label(context, "By material", checkWrapped, false));

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var matBoxWrapped = Wrap(matBox);
                var matBoxLabeled = Label(context, "Material", matBoxWrapped);
                SetupRadioButtonHiddenToggle(materialCheck, matBoxLabeled);
                layout.Children.Add(matBoxLabeled);

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                layout.Children.Add(Label(context, "Maximum", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByReceivingInventory, layout);
                parent.Children.Add(layout);
            }

            public void InitDeliveringInventory(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer based on what
would be in the delivering inventory
after the transfer.

This is most useful when you are trying
to maintain a certain distribution of 
items in your inventory and your 
inventory does not always empty out
every cycle, such as for a courier.");
                layout.Children.Add(Wrap(description));

                var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
                var checkWrapped = Wrap(materialCheck);
                layout.Children.Add(Label(context, "By material", checkWrapped, false));

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var matBoxWrapped = Wrap(matBox);
                var matBoxLabeled = Label(context, "Material", matBoxWrapped);
                SetupRadioButtonHiddenToggle(materialCheck, matBoxLabeled);
                layout.Children.Add(matBoxLabeled);

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                layout.Children.Add(Label(context, "Minimum", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByDeliveringInventory, layout);
                parent.Children.Add(layout);

            }

            public void LoadFromOriginal()
            {

            }

            public ITransferRestrictor CreateRestrictor(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
            {
                return null;
            }

            public bool IsValid(SharedGameState sharedState, LocalGameState localSTate, NetContext netContext)
            {
                return false;
            }
        }

        protected const string _InspectDescription = @"A transfer item task is a leaf task that transfers an 
item by type or inventory position(s) to/from this 
entity to/from an entity by ID, position, or relative 
position. 

Restrictions may be placed on how many items are 
transferred based on how many items would remain in
the giving inventory, how many items would be 
contained in the recieving inventory, and how many 
items have been transferred so far.

The result of a transfer item task can be selected
based on items in the giving inventory, items in
the recieving inventory, and items transferred so
far.";

        protected List<TransferRestrictorComponent> Restrictors;

        /// <summary>
        /// We don't add restrictors on button push since that happens
        /// in mouse released which is part of update, which means we
        /// would need to keep a reference to a render context somewhere
        /// in order to construct the restrictor. So instead we set this
        /// variable to true, and in predraw we check it and add to 
        /// restrictors there.
        /// </summary>
        protected bool RestrictorQueuedToAdd;

        /// <summary>
        /// For the same reason as queued to add, this is the index of the
        /// restrictor we want to remove or -1
        /// </summary>
        protected int RestrictorIndexQueuedToRemove;

        /// <summary>
        /// This is the layout that restrictors get appended to or removed from. It contains
        /// nothing but transfer restrictor components Component, in the same order that is
        /// in Restrictors.
        /// </summary>
        protected WeakReference<TaskItemComponentAsLayoutManager> RestrictorsInnerLayout;

        public TransferItemTaskItem2()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Savable = true;
            Expandable = false;
            Expanded = false;
            TaskName = "Transfer Item";


            Restrictors = new List<TransferRestrictorComponent>();

            RestrictorsInnerLayout = new WeakReference<TaskItemComponentAsLayoutManager>(null);

            RestrictorQueuedToAdd = false;
            RestrictorIndexQueuedToRemove = -1;
        }

        public TransferItemTaskItem2(EntityTransferItemTask task) : this()
        {
            Task = task;
        }


        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
            height += 150;
        }

        protected override void InitializeComponent(RenderContext context)
        {
            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredWidth, 7);

            InitPickupDropoff(context, redraw, redrawAndReload, layout);
            InitTargetDecider(context, redraw, redrawAndReload, layout);
            InitRestrictors(context, redraw, redrawAndReload, layout);
            InitResultDecider(context, redraw, redrawAndReload, layout);

            InspectComponent = layout;
        }

        protected void InitPickupDropoff(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var radioLayout = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
            var pickupRadio = CreateRadioButton(context, redraw, redrawAndReload);
            var wrappedRadio = Wrap(pickupRadio);
            radioLayout.Children.Add(Label(context, "Pickup", wrappedRadio, false));

            var dropoffRadio = CreateRadioButton(context, redraw, redrawAndReload);
            wrappedRadio = Wrap(dropoffRadio);
            radioLayout.Children.Add(Label(context, "Dropoff", wrappedRadio, false));

            GroupRadioButtons(context, redraw, redrawAndReload, pickupRadio, dropoffRadio);
            layout.Children.Add(radioLayout);

            main.Children.Add(layout);
        }

        protected void InitTargetDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);
            
            var combo = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By ID", TargetDeciderType.ByID),
                Tuple.Create("By Position", TargetDeciderType.ByPosition),
                Tuple.Create("By Relative Position", TargetDeciderType.ByRelativePosition));
            var wrapped = Wrap(combo);
            layout.Children.Add(Label(context, "Target Decider", wrapped));

            InitTargetDeciderByID(context, redraw, redrawAndReload, layout, combo);
            InitTargetDeciderByPosition(context, redraw, redrawAndReload, layout, combo);
            InitTargetDeciderByRelativePosition(context, redraw, redrawAndReload, layout, combo);

            main.Children.Add(layout);
        }

        protected void InitTargetDeciderByID(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for the entity 
with the specified ID. This is a good choice 
if you always want to transfer items with 
the same entity");
            layout.Children.Add(Wrap(description));

            var field = CreateTextField(context, redraw, redrawAndReload);
            field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            var fieldWrapped = Wrap(field);
            layout.Children.Add(Label(context, "ID", fieldWrapped));

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByID, layout);
            parent.Children.Add(layout);
        }

        protected void InitTargetDeciderByPosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for the entity 
at the specified position. This is helpful if
you want to be able to swap out the target 
without modifying this entity.");
            layout.Children.Add(Wrap(description));

            var fieldsLayout = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var xField = CreateTextField(context, redraw, redrawAndReload);
            xField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            var fieldWrapped = Wrap(xField);
            fieldsLayout.Children.Add(Label(context, "X", fieldWrapped));

            var yField = CreateTextField(context, redraw, redrawAndReload);
            yField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            fieldWrapped = Wrap(yField);
            fieldsLayout.Children.Add(Label(context, "Y", fieldWrapped));

            layout.Children.Add(fieldsLayout);

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByPosition, layout);
            parent.Children.Add(layout);
        }

        protected void InitTargetDeciderByRelativePosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for an entity 
at a position relative to this entity at the 
time that this task is evaluated. This is 
typically the easiest to use. Left is 
negative x, up is negative y.");
            layout.Children.Add(Wrap(description));

            var fieldsLayout = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var dxField = CreateTextField(context, redraw, redrawAndReload);
            dxField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            var fieldWrapped = Wrap(dxField);
            fieldsLayout.Children.Add(Label(context, "Delta X", fieldWrapped));

            var dyField = CreateTextField(context, redraw, redrawAndReload);
            dyField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            fieldWrapped = Wrap(dyField);
            fieldsLayout.Children.Add(Label(context, "Delta Y", fieldWrapped));

            layout.Children.Add(fieldsLayout);

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByRelativePosition, layout);
            parent.Children.Add(layout);
        }

        protected void InitRestrictors(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 7);

            var innerLayout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);
            RestrictorsInnerLayout.SetTarget(innerLayout);
            layout.Children.Add(innerLayout);
            
            var addRestrictorButton = CreateButton(context, redraw, redrawAndReload, "Add Restrictor");
            addRestrictorButton.PressReleased += (sender, args) =>
            {
                RestrictorQueuedToAdd = true;
            };

            var wrappedButton = Wrap(addRestrictorButton);
            layout.Children.Add(wrappedButton);

            main.Children.Add(layout);
        }

        protected void AddRestrictor(TransferRestrictorComponent restrictor)
        {
#if DEBUG
            if (Restrictors.Contains(restrictor))
                throw new InvalidProgramException("You are using this function incorrectly - it will add to Restrictors");
#endif

            ITaskItemComponent component;
            if (!TryGetWrapped(restrictor.Component, out component))
                throw new InvalidProgramException("Restrictor is not initialized");

            TaskItemComponentAsLayoutManager layout;
            if (!TryGetWrapped(RestrictorsInnerLayout, out layout))
                throw new InvalidOperationException("Visuals are disposed - cannot add restrictor!");

            Restrictors.Add(restrictor);
            layout.Children.Add(component);
        }

        protected void RemoveRestrictor(TransferRestrictorComponent restrictor)
        {
            int index = Restrictors.FindIndex((r) => ReferenceEquals(r, restrictor));
            RemoveRestrictorAt(index);
        }

        protected void RemoveRestrictorAt(int index)
        {
            if (index < 0)
                throw new InvalidProgramException("Restrictor is not in restrictors!");

            TaskItemComponentAsLayoutManager layout;
            if (!TryGetWrapped(RestrictorsInnerLayout, out layout))
                throw new InvalidOperationException("Visuals are disposed - can't remove the restrictor");

            var component = layout.Children[index];
            
            layout.Children.RemoveAt(index);
            Restrictors.RemoveAt(index);

            component.Dispose();
        }

        protected void InitResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, TaskItemComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var box = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By Items Transfered", ResultDeciderType.ItemsTransferred),
                Tuple.Create("By Our Items", ResultDeciderType.OurItems),
                Tuple.Create("By Their Items", ResultDeciderType.TheirItems));
            var boxWrapped = Wrap(box);
            layout.Children.Add(Label(context, "Result Decider", boxWrapped));

            InitItemsTransferedResultDecider(context, redraw, redrawAndReload, layout, box);
            InitOurItemsResultDecider(context, redraw, redrawAndReload, layout, box);
            InitTheirItemsResultDecider(context, redraw, redrawAndReload, layout, box);
            
            main.Children.Add(layout);
        }

        protected void InitItemsTransferedResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, 
            TaskItemComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            layout.Children.Add(Label(context, "Amount", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.ItemsTransferred, layout);
            parent.Children.Add(layout);
        }

        protected void InitOurItemsResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            TaskItemComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            layout.Children.Add(Label(context, "Amount", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.OurItems, layout);
            parent.Children.Add(layout);
        }

        protected void InitTheirItemsResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            TaskItemComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            layout.Children.Add(Label(context, "Amount", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.TheirItems, layout);
            parent.Children.Add(layout);
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return new EntityTransferItemTask();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return false;
        }

        protected override void LoadFromTask()
        {

            Reload = true;
            OnInspectRedrawRequired();
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            if(RestrictorIndexQueuedToRemove != -1)
            {
                RemoveRestrictorAt(RestrictorIndexQueuedToRemove);
                RestrictorIndexQueuedToRemove = -1;
            }

            if(RestrictorQueuedToAdd)
            {
                var restrictor = new TransferRestrictorComponent(this);
                restrictor.InitializeComponent(context, (sender, args) => OnInspectRedrawRequired(), (sender, args) => { Reload = true; OnInspectRedrawRequired(); });

                AddRestrictor(restrictor);

                RestrictorQueuedToAdd = false;
            }

            base.PreDrawInspect(context, x, y);
        }

    }
}
