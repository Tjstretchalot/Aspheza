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
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using BaseBuilder.Screens.Components;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Screens.GComponents.ScrollableComponents;
using BaseBuilder.Screens.Components.ScrollableComponents;

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
     *           Maximum
     *           Textbox
     *           button Delete
     *         By Delivering Inventory
     *           Restrict the transfer based on how many items would be in our 
     *           inventory (optionally of a specific type) after the transfer
     *           Checkbox for type check
     *             Dropdown for material
     *           Minimum
     *           Textbox
     *           button Delete
     *     button Add Restrictor
     *     
     *   4. Choosing the result decider
     *     Dropdown
     *       Delivering Items
     *         Succeed when the deliverer has at most a certain number of items
     *         Checkbox for type check
     *           Dropdown for material
     *         At most
     *         Textbox
     *       Receiving Items
     *         Succeed when the receiver has at least a certain number of items
     *         Checkbox for type check
     *           Dropdown for material
     *         At least
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
    public class TransferItemTaskItem : ComplexTaskItem
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
            ByDeliveringInventory,
            ByReceivingInventory,
            ByItemsTransferred
        }

        protected class TransferRestrictorComponent
        {
            public TransferItemTaskItem Outer;
            public ITransferRestrictor Original;

            public WeakReference<IScrollableComponent> Component;

            public WeakReference<ScrollableComponentFromScreenComponent<ComboBox<RestrictorType>>> TypeBox;

            public WeakReference<ScrollableComponentFromScreenComponent<RadioButton>> ByItemType_DenyOthersRadio;
            public WeakReference<ScrollableComponentFromScreenComponent<RadioButton>> ByItemType_DenyThisRadio;
            public WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ByItemType_MaterialBox;

            public WeakReference<ScrollableComponentFromScreenComponent<TextField>> ByQuantity_Field;

            public WeakReference<ScrollableComponentFromScreenComponent<CheckBox>> ByReceiving_MaterialCheck;
            public WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ByReceiving_MaterialBox;
            public WeakReference<ScrollableComponentFromScreenComponent<TextField>> ByReceiving_Field;

            public WeakReference<ScrollableComponentFromScreenComponent<CheckBox>> ByDelivering_MaterialCheck;
            public WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ByDelivering_MaterialBox;
            public WeakReference<ScrollableComponentFromScreenComponent<TextField>> ByDelivering_Field;

            public TransferRestrictorComponent(TransferItemTaskItem outer, ITransferRestrictor restrictor = null)
            {
                Outer = outer;
                Original = restrictor;

                Component = new WeakReference<IScrollableComponent>(null);

                TypeBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<RestrictorType>>>(null);

                ByItemType_DenyOthersRadio = new WeakReference<ScrollableComponentFromScreenComponent<RadioButton>>(null);
                ByItemType_DenyThisRadio = new WeakReference<ScrollableComponentFromScreenComponent<RadioButton>>(null);
                ByItemType_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);

                ByQuantity_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);

                ByReceiving_MaterialCheck = new WeakReference<ScrollableComponentFromScreenComponent<CheckBox>>(null);
                ByReceiving_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);
                ByReceiving_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);

                ByDelivering_MaterialCheck = new WeakReference<ScrollableComponentFromScreenComponent<CheckBox>>(null);
                ByDelivering_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);
                ByDelivering_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            }

            public void InitializeComponent(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
            {
                var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var box = CreateComboBox(context, redraw, redrawAndReload,
                    Tuple.Create("By Item Type", RestrictorType.ByItemType),
                    Tuple.Create("By Total Quantity", RestrictorType.ByTotalQuantity),
                    Tuple.Create("By Receiving Inventory", RestrictorType.ByReceivingInventory),
                    Tuple.Create("By Delivering Inventory", RestrictorType.ByDeliveringInventory));
                var boxWrapped = Wrap(box);
                TypeBox.SetTarget(boxWrapped);
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
            
            public void InitItemTypeRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer by either denying a
specific material or denying everything 
except a specific material in a transfer.

This is the most common restriction and is
useful whenever you have multiple item types
in your inventory or the targets inventory.", true);
                layout.Children.Add(Wrap(description));

                var buttonsLayout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
                var radio1 = CreateRadioButton(context, redraw, redrawAndReload);
                var radioWrapped = Wrap(radio1);
                ByItemType_DenyOthersRadio.SetTarget(radioWrapped);
                buttonsLayout.Children.Add(Label(context, "Deny Everything Else", radioWrapped, false));

                var radio2 = CreateRadioButton(context, redraw, redrawAndReload);
                radioWrapped = Wrap(radio2);
                ByItemType_DenyThisRadio.SetTarget(radioWrapped);
                buttonsLayout.Children.Add(Label(context, "Deny Just This", radioWrapped, false));

                layout.Children.Add(buttonsLayout);

                new RadioButtonGroup(new[] { radio1, radio2 }).Attach();

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var wrappedBox = Wrap(matBox);
                ByItemType_MaterialBox.SetTarget(wrappedBox);
                layout.Children.Add(Label(context, "Material", wrappedBox));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByItemType, layout);
                parent.Children.Add(layout);
            }

            public void InitTotalQuantityRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer by not transferring 
more than a specific amount. 

This is useful if you either know exactly 
what items should be in your inventory at 
this point and want to utilize the order 
that items are given out, or you are picking
up from a larger container.", true);
                layout.Children.Add(Wrap(description));

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                ByQuantity_Field.SetTarget(fieldWrapped);
                layout.Children.Add(Label(context, "Amount", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByTotalQuantity, layout);
                parent.Children.Add(layout);
            }

            public void InitReceivingRestriction(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer based on what 
would be in the receiving inventory
after the transfer.

This is useful if you want to avoid
one inventory consuming all of a 
specific type of item.", true);
                layout.Children.Add(Wrap(description));

                var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
                var checkWrapped = Wrap(materialCheck);
                ByReceiving_MaterialCheck.SetTarget(checkWrapped);
                layout.Children.Add(Label(context, "By material", checkWrapped, false));

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var matBoxWrapped = Wrap(matBox);
                ByReceiving_MaterialBox.SetTarget(matBoxWrapped);
                var matBoxLabeled = Label(context, "Material", matBoxWrapped);
                SetupRadioButtonHiddenToggle(materialCheck, matBoxLabeled);
                layout.Children.Add(matBoxLabeled);

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                ByReceiving_Field.SetTarget(fieldWrapped);
                layout.Children.Add(Label(context, "Maximum", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByReceivingInventory, layout);
                parent.Children.Add(layout);
            }

            public void InitDeliveringInventory(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent,
                ComboBox<RestrictorType> box)
            {
                var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

                var description = CreateText(context, @"Restrict the transfer based on what
would be in the delivering inventory
after the transfer.

This is most useful when you are trying
to maintain a certain distribution of 
items in your inventory and your 
inventory does not always empty out
every cycle, such as for a courier.", true);
                layout.Children.Add(Wrap(description));

                var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
                var checkWrapped = Wrap(materialCheck);
                ByDelivering_MaterialCheck.SetTarget(checkWrapped);
                layout.Children.Add(Label(context, "By material", checkWrapped, false));

                var matBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
                var matBoxWrapped = Wrap(matBox);
                ByDelivering_MaterialBox.SetTarget(matBoxWrapped);
                var matBoxLabeled = Label(context, "Material", matBoxWrapped);
                SetupRadioButtonHiddenToggle(materialCheck, matBoxLabeled);
                layout.Children.Add(matBoxLabeled);

                var field = CreateTextField(context, redraw, redrawAndReload);
                field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
                var fieldWrapped = Wrap(field);
                ByDelivering_Field.SetTarget(fieldWrapped);
                layout.Children.Add(Label(context, "Minimum", fieldWrapped));

                SetupComboBoxHiddenToggle(box, RestrictorType.ByDeliveringInventory, layout);
                parent.Children.Add(layout);

            }

            public void LoadFromOriginal()
            {
                if (Original == null)
                    return;

                var box = Unwrap(TypeBox);
                var origType = Original.GetType();

                if(origType == typeof(MaterialRestriction))
                {
                    var orig = Original as MaterialRestriction;

                    box.Selected = box.GetComboItemByValue(RestrictorType.ByItemType);

                    if (orig.AllExcept)
                        Unwrap(ByItemType_DenyOthersRadio).Pushed = true;
                    else
                        Unwrap(ByItemType_DenyThisRadio).Pushed = true;

                    if (orig.KeyMaterial != null)
                    {
                        var matBox = Unwrap(ByItemType_MaterialBox);
                        matBox.Selected = matBox.GetComboItemByValue(orig.KeyMaterial);
                    }
                }else if(origType == typeof(QuantityRestriction))
                {
                    var orig = Original as QuantityRestriction;

                    box.Selected = box.GetComboItemByValue(RestrictorType.ByTotalQuantity);

                    Unwrap(ByQuantity_Field).Text = orig.KeyQuantity.ToString();
                }else if(origType == typeof(InventoryRestriction))
                {
                    var orig = Original as InventoryRestriction;

                    if(orig.CheckRecievingInventory)
                    {
                        box.Selected = box.GetComboItemByValue(RestrictorType.ByReceivingInventory);

                        if(orig.KeyMaterial != null)
                        {
                            Unwrap(ByReceiving_MaterialCheck).Pushed = true;
                            var matBox = Unwrap(ByReceiving_MaterialBox);
                            matBox.Selected = matBox.GetComboItemByValue(orig.KeyMaterial);
                        }

                        Unwrap(ByReceiving_Field).Text = orig.KeyQuantity.ToString();
                    }else
                    {
                        box.Selected = box.GetComboItemByValue(RestrictorType.ByDeliveringInventory);

                        if(orig.KeyMaterial != null)
                        {
                            Unwrap(ByDelivering_MaterialCheck).Pushed = true;
                            var matBox = Unwrap(ByDelivering_MaterialBox);
                            matBox.Selected = matBox.GetComboItemByValue(orig.KeyMaterial);
                        }

                        Unwrap(ByDelivering_Field).Text = orig.KeyQuantity.ToString();
                    }
                }
            }

            public ITransferRestrictor CreateRestrictor(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
            {
                ScrollableComponentFromScreenComponent<ComboBox<RestrictorType>> typeBox;
                if(!TryGetWrapped(TypeBox, out typeBox))
                {
                    if(Original != null)
                    {
                        return CloneOriginal(taskable);
                    }

                    return null;
                }

                var box = typeBox.Component;

                if (box.Selected == null)
                    return null;
                
                ComboBox<Material> matBox;
                Material mat;
                int parsed;
                switch(box.Selected.Value)
                {
                    case RestrictorType.ByItemType:
                        matBox = Unwrap(ByItemType_MaterialBox);
                        return new MaterialRestriction(matBox.Selected == null ? null : matBox.Selected.Value, Unwrap(ByItemType_DenyOthersRadio).Pushed);
                    case RestrictorType.ByTotalQuantity:
                        if (!int.TryParse(Unwrap(ByQuantity_Field).Text, out parsed))
                            parsed = 0;
                        return new QuantityRestriction(null, parsed);
                    case RestrictorType.ByReceivingInventory:
                        mat = null;
                        if(Unwrap(ByReceiving_MaterialCheck).Pushed)
                        {
                            matBox = Unwrap(ByReceiving_MaterialBox);
                            mat = matBox.Selected == null ? null : matBox.Selected.Value;
                        }

                        if (!int.TryParse(Unwrap(ByReceiving_Field).Text, out parsed))
                            parsed = 0;

                        return new InventoryRestriction(true, parsed, mat);
                    case RestrictorType.ByDeliveringInventory:
                        mat = null;
                        if(Unwrap(ByDelivering_MaterialCheck).Pushed)
                        {
                            matBox = Unwrap(ByDelivering_MaterialBox);
                            mat = matBox.Selected == null ? null : matBox.Selected.Value;
                        }

                        if (!int.TryParse(Unwrap(ByDelivering_Field).Text, out parsed))
                            parsed = 0;

                        return new InventoryRestriction(false, parsed, mat);
                    default:
                        throw new InvalidOperationException($"Unknown restrictor type {box.Selected.Value}");
                }
            }

            /// <summary>
            /// This is not a true clone, we purposely use this to drop any state. If 
            /// the original is stateless, returns a reference to it.
            /// </summary>
            /// <param name="taskable"></param>
            public ITransferRestrictor CloneOriginal(ITaskable taskable)
            {
                var origType = Original.GetType();

                if(origType == typeof(InventoryRestriction))
                {
                    return Original; // "stateless"ish
                }else if(origType == typeof(MaterialRestriction))
                {
                    return Original; // "stateless"ish
                }else if(origType == typeof(QuantityRestriction))
                {
                    var orig = Original as QuantityRestriction;

                    return new QuantityRestriction(orig.KeyMaterial, orig.KeyQuantity);
                }else
                {
                    throw new InvalidOperationException($"Unknown restriction {origType.FullName}");
                }
            }

            public bool IsValid()
            {
                ScrollableComponentFromScreenComponent<ComboBox<RestrictorType>> typeBox;
                if (!TryGetWrapped(TypeBox, out typeBox))
                {
                    if (Original != null)
                    {
                        return Original.IsValid();
                    }

                    return false;
                }

                var box = typeBox.Component;
                if (box.Selected == null)
                    return false;

                int parsed;
                switch(box.Selected.Value)
                {
                    case RestrictorType.ByItemType:
                        if (!Unwrap(ByItemType_DenyOthersRadio).Pushed && !Unwrap(ByItemType_DenyThisRadio).Pushed)
                            return false;
                        if (Unwrap(ByItemType_MaterialBox).Selected == null)
                            return false;
                        return true;
                    case RestrictorType.ByTotalQuantity:
                        if (!int.TryParse(Unwrap(ByQuantity_Field).Text, out parsed))
                            return false;
                        if (parsed <= 0)
                            return false;
                        return true;
                    case RestrictorType.ByReceivingInventory:
                        if (Unwrap(ByReceiving_MaterialCheck).Pushed && Unwrap(ByReceiving_MaterialBox).Selected == null)
                            return false;
                        if (!int.TryParse(Unwrap(ByReceiving_Field).Text, out parsed))
                            return false;
                        if (parsed <= 0)
                            return false;
                        return true;
                    case RestrictorType.ByDeliveringInventory:
                        if (Unwrap(ByDelivering_MaterialCheck).Pushed && Unwrap(ByDelivering_MaterialBox).Selected == null)
                            return false;
                        if (!int.TryParse(Unwrap(ByDelivering_Field).Text, out parsed))
                            return false;
                        if (parsed <= 0)
                            return false;
                        return true;
                    default:
                        throw new InvalidOperationException($"Unknown restrictor type {box.Selected.Value}");
                }
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
        protected WeakReference<ScrollableComponentAsLayoutManager> RestrictorsInnerLayout;

        protected WeakReference<ScrollableComponentFromScreenComponent<RadioButton>> PickupRadio;
        protected WeakReference<ScrollableComponentFromScreenComponent<RadioButton>> DropoffRadio;

        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<TargetDeciderType>>> TargeterCombo;

        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByID_Field;

        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByPosition_XField;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByPosition_YField;

        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByRelPosition_DXField;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> TargetByRelPosition_DYField;

        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<ResultDeciderType>>> ResultDeciderCombo;

        protected WeakReference<ScrollableComponentFromScreenComponent<CheckBox>> ResultDeciderByItemsTransfered_MaterialCheck;
        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ResultDeciderByItemsTransfered_MaterialBox;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> ResultDeciderByItemsTransfered_Field;

        protected WeakReference<ScrollableComponentFromScreenComponent<CheckBox>> ResultDeciderByDeliveringItems_MaterialCheck;
        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ResultDeciderByDeliveringItems_MaterialBox;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> ResultDeciderByDeliveringItems_Field;

        protected WeakReference<ScrollableComponentFromScreenComponent<CheckBox>> ResultDeciderByReceivingItems_MaterialCheck;
        protected WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>> ResultDeciderByReceivingItems_MaterialBox;
        protected WeakReference<ScrollableComponentFromScreenComponent<TextField>> ResultDeciderByReceivingItems_Field;

        public TransferItemTaskItem() : base()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Savable = true;
            Expandable = false;
            Expanded = false;
            TaskName = "Transfer Item";


            Restrictors = new List<TransferRestrictorComponent>();

            RestrictorsInnerLayout = new WeakReference<ScrollableComponentAsLayoutManager>(null);

            PickupRadio = new WeakReference<ScrollableComponentFromScreenComponent<RadioButton>>(null);
            DropoffRadio = new WeakReference<ScrollableComponentFromScreenComponent<RadioButton>>(null);

            TargeterCombo = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<TargetDeciderType>>>(null);

            TargetByID_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);

            TargetByPosition_XField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByPosition_YField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);

            TargetByRelPosition_DXField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            TargetByRelPosition_DYField = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);

            ResultDeciderCombo = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<ResultDeciderType>>>(null);

            ResultDeciderByItemsTransfered_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            ResultDeciderByItemsTransfered_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);
            ResultDeciderByItemsTransfered_MaterialCheck = new WeakReference<ScrollableComponentFromScreenComponent<CheckBox>>(null);

            ResultDeciderByDeliveringItems_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            ResultDeciderByDeliveringItems_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);
            ResultDeciderByDeliveringItems_MaterialCheck = new WeakReference<ScrollableComponentFromScreenComponent<CheckBox>>(null);

            ResultDeciderByReceivingItems_Field = new WeakReference<ScrollableComponentFromScreenComponent<TextField>>(null);
            ResultDeciderByReceivingItems_MaterialBox = new WeakReference<ScrollableComponentFromScreenComponent<ComboBox<Material>>>(null);
            ResultDeciderByReceivingItems_MaterialCheck = new WeakReference<ScrollableComponentFromScreenComponent<CheckBox>>(null);

            RestrictorQueuedToAdd = false;
            RestrictorIndexQueuedToRemove = -1;
        }

        public TransferItemTaskItem(EntityTransferItemTask task) : this()
        {
            Task = task;
        }


        protected override void CalculateHeight(RenderContext renderContext, ref int height, int width)
        {
            base.CalculateHeight(renderContext, ref height, width);
            height += 150;
        }

        protected override IScrollableComponent InitializeComponent(RenderContext context)
        {
            EventHandler redraw = (sender, args) => OnInspectRedrawRequired();
            EventHandler redrawAndReload = (sender, args) =>
            {
                Reload = true;
                OnInspectRedrawRequired();
            };

            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredWidth, 7);

            InitPickupDropoff(context, redraw, redrawAndReload, layout);
            InitTargetDecider(context, redraw, redrawAndReload, layout);
            InitRestrictors(context, redraw, redrawAndReload, layout);
            InitResultDecider(context, redraw, redrawAndReload, layout);

            return layout;
        }

        protected void InitPickupDropoff(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var radioLayout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
            var pickupRadio = CreateRadioButton(context, redraw, redrawAndReload);
            var wrappedRadio = Wrap(pickupRadio);
            PickupRadio.SetTarget(wrappedRadio);
            radioLayout.Children.Add(Label(context, "Pickup", wrappedRadio, false));

            var dropoffRadio = CreateRadioButton(context, redraw, redrawAndReload);
            wrappedRadio = Wrap(dropoffRadio);
            DropoffRadio.SetTarget(wrappedRadio);
            radioLayout.Children.Add(Label(context, "Dropoff", wrappedRadio, false));

            GroupRadioButtons(context, redraw, redrawAndReload, pickupRadio, dropoffRadio);
            layout.Children.Add(radioLayout);

            main.Children.Add(layout);
        }

        protected void InitTargetDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);
            
            var combo = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By ID", TargetDeciderType.ByID),
                Tuple.Create("By Position", TargetDeciderType.ByPosition),
                Tuple.Create("By Relative Position", TargetDeciderType.ByRelativePosition));
            var wrapped = Wrap(combo);
            TargeterCombo.SetTarget(wrapped);
            layout.Children.Add(Label(context, "Target Decider", wrapped));

            InitTargetDeciderByID(context, redraw, redrawAndReload, layout, combo);
            InitTargetDeciderByPosition(context, redraw, redrawAndReload, layout, combo);
            InitTargetDeciderByRelativePosition(context, redraw, redrawAndReload, layout, combo);

            main.Children.Add(layout);
        }

        protected void InitTargetDeciderByID(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for the entity 
with the specified ID. This is a good choice 
if you always want to transfer items with 
the same entity", true);
            layout.Children.Add(Wrap(description));

            var field = CreateTextField(context, redraw, redrawAndReload);
            field.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            var fieldWrapped = Wrap(field);
            TargetByID_Field.SetTarget(fieldWrapped);
            layout.Children.Add(Label(context, "ID", fieldWrapped));

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByID, layout);
            parent.Children.Add(layout);
        }

        protected void InitTargetDeciderByPosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for the entity 
at the specified position. This is helpful if
you want to be able to swap out the target 
without modifying this entity.", true);
            layout.Children.Add(Wrap(description));

            var fieldsLayout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var xField = CreateTextField(context, redraw, redrawAndReload);
            xField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            var fieldWrapped = Wrap(xField);
            TargetByPosition_XField.SetTarget(fieldWrapped);
            fieldsLayout.Children.Add(Label(context, "X", fieldWrapped));

            var yField = CreateTextField(context, redraw, redrawAndReload);
            yField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, false);
            fieldWrapped = Wrap(yField);
            TargetByPosition_YField.SetTarget(fieldWrapped);
            fieldsLayout.Children.Add(Label(context, "Y", fieldWrapped));

            layout.Children.Add(fieldsLayout);

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByPosition, layout);
            parent.Children.Add(layout);
        }

        protected void InitTargetDeciderByRelativePosition(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager parent, ComboBox<TargetDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var description = CreateText(context, @"Select the target by searching for an entity 
at a position relative to this entity at the 
time that this task is evaluated. This is 
typically the easiest to use. Left is 
negative x, up is negative y.", true);
            layout.Children.Add(Wrap(description));

            var fieldsLayout = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);

            var dxField = CreateTextField(context, redraw, redrawAndReload);
            dxField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            var fieldWrapped = Wrap(dxField);
            TargetByRelPosition_DXField.SetTarget(fieldWrapped);
            fieldsLayout.Children.Add(Label(context, "Delta X", fieldWrapped));

            var dyField = CreateTextField(context, redraw, redrawAndReload);
            dyField.TextChanged += UIUtils.TextFieldRestrictToNumbers(false, true);
            fieldWrapped = Wrap(dyField);
            TargetByRelPosition_DYField.SetTarget(fieldWrapped);
            fieldsLayout.Children.Add(Label(context, "Delta Y", fieldWrapped));

            layout.Children.Add(fieldsLayout);

            SetupComboBoxHiddenToggle(box, TargetDeciderType.ByRelativePosition, layout);
            parent.Children.Add(layout);
        }

        protected void InitRestrictors(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 7);

            var innerLayout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);
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

            IScrollableComponent component;
            if (!TryGetWrapped(restrictor.Component, out component))
                throw new InvalidProgramException("Restrictor is not initialized");

            ScrollableComponentAsLayoutManager layout;
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

            ScrollableComponentAsLayoutManager layout;
            if (!TryGetWrapped(RestrictorsInnerLayout, out layout))
                throw new InvalidOperationException("Visuals are disposed - can't remove the restrictor");

            var component = layout.Children[index];
            
            layout.Children.RemoveAt(index);
            Restrictors.RemoveAt(index);

            component.Dispose();
        }

        protected void InitResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, ScrollableComponentAsLayoutManager main)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            var box = CreateComboBox(context, redraw, redrawAndReload,
                Tuple.Create("By Items Transfered", ResultDeciderType.ByItemsTransferred),
                Tuple.Create("By Deliv. Inv. Items", ResultDeciderType.ByDeliveringInventory),
                Tuple.Create("By Receiv. Inv. Items", ResultDeciderType.ByReceivingInventory));
            var boxWrapped = Wrap(box);
            ResultDeciderCombo.SetTarget(boxWrapped);
            layout.Children.Add(Label(context, "Result Decider", boxWrapped));

            InitItemsTransferedResultDecider(context, redraw, redrawAndReload, layout, box);
            InitDeliveringInventoryResultDecider(context, redraw, redrawAndReload, layout, box);
            InitReceivingInventoryResultDecider(context, redraw, redrawAndReload, layout, box);
            
            main.Children.Add(layout);
        }

        protected void InitItemsTransferedResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, 
            ScrollableComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            layout.Children.Add(Wrap(CreateText(context, @"The result of this transfer will be
failure unless at least the specified
number of items were transfered. 
Optionally only count items of a 
specific type.", true)));
            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            ResultDeciderByItemsTransfered_MaterialCheck.SetTarget(wrappedCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            ResultDeciderByItemsTransfered_MaterialBox.SetTarget(wrappedMatBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            ResultDeciderByItemsTransfered_Field.SetTarget(wrappedField);
            layout.Children.Add(Label(context, "Amount", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.ByItemsTransferred, layout);
            parent.Children.Add(layout);
        }

        protected void InitDeliveringInventoryResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            ScrollableComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);

            layout.Children.Add(Wrap(CreateText(context, @"The result of this transfer will be
failure if the delivering inventory
still has more than the specified 
number of items after the transfer.
Optionally only count items of a 
specific type.", true)));

            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            ResultDeciderByDeliveringItems_MaterialCheck.SetTarget(wrappedCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            ResultDeciderByDeliveringItems_MaterialBox.SetTarget(wrappedMatBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            ResultDeciderByDeliveringItems_Field.SetTarget(wrappedField);
            layout.Children.Add(Label(context, "At most", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.ByDeliveringInventory, layout);
            parent.Children.Add(layout);
        }

        protected void InitReceivingInventoryResultDecider(RenderContext context, EventHandler redraw, EventHandler redrawAndReload,
            ScrollableComponentAsLayoutManager parent, ComboBox<ResultDeciderType> box)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);


            layout.Children.Add(Wrap(CreateText(context, @"The result of this transfer will be
failure if the receiving inventory
does not have at least the specified
number of items after the transfer.
Optionally only count items of a 
specific type.", true)));

            var materialCheck = CreateCheckBox(context, redraw, redrawAndReload);
            var wrappedCheck = Wrap(materialCheck);
            ResultDeciderByReceivingItems_MaterialCheck.SetTarget(wrappedCheck);
            layout.Children.Add(Label(context, "By material", wrappedCheck, false));

            var materialBox = CreateMaterialComboBox(context, redraw, redrawAndReload);
            var wrappedMatBox = Wrap(materialBox);
            ResultDeciderByReceivingItems_MaterialBox.SetTarget(wrappedMatBox);
            var labelledMatBox = Label(context, "Material", wrappedMatBox);
            SetupRadioButtonHiddenToggle(materialCheck, labelledMatBox);
            layout.Children.Add(labelledMatBox);

            var field = CreateTextField(context, redraw, redrawAndReload);
            var wrappedField = Wrap(field);
            ResultDeciderByReceivingItems_Field.SetTarget(wrappedField);
            layout.Children.Add(Label(context, "At least", wrappedField));

            SetupComboBoxHiddenToggle(box, ResultDeciderType.ByReceivingInventory, layout);
            parent.Children.Add(layout);
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            ScrollableComponentFromScreenComponent<RadioButton> pickupWrapped;
            if (!TryGetWrapped(PickupRadio, out pickupWrapped))
            {
                if (Task == null)
                    return new EntityTransferItemTask();
                else
                {
                    var task = Task as EntityTransferItemTask;
                    return new EntityTransferItemTask(((Thing)taskable).ID, task.Pickup, task.Targeter, task.Restrictors, task.ResultDecider);
                }
            }

            return new EntityTransferItemTask(((Thing)taskable).ID, Unwrap(pickupWrapped).Pushed, CreateTargeter(taskable), CreateRestrictors(taskable, sharedState, localState, netContext), CreateResultDecider(taskable));
        }

        protected ITransferTargeter CreateTargeter(ITaskable taskable)
        {
            var box = Unwrap(TargeterCombo);

            if (box.Selected == null)
                return null;

            TextField field;
            int parsed, x, y;
            switch(box.Selected.Value)
            {
                case TargetDeciderType.ByID:
                    field = Unwrap(TargetByID_Field);

                    if (!int.TryParse(field.Text, out parsed))
                        parsed = 0;

                    return new TransferTargetByID(parsed);
                case TargetDeciderType.ByPosition:
                    if (!int.TryParse(Unwrap(TargetByPosition_XField).Text, out x))
                        x = 0;
                    
                    if (!int.TryParse(Unwrap(TargetByPosition_YField).Text, out y))
                        y = 0;

                    return new TransferTargetByPosition(new PointI2D(x, y));
                case TargetDeciderType.ByRelativePosition:
                    if (!int.TryParse(Unwrap(TargetByRelPosition_DXField).Text, out x))
                        x = 0;

                    if (!int.TryParse(Unwrap(TargetByRelPosition_DYField).Text, out y))
                        y = 0;

                    return new TransferTargetByRelativePosition(new VectorD2D(x, y));
                default:
                    throw new InvalidOperationException($"Unknown target type: {box.Selected.Value}");
            }
        }

        protected List<ITransferRestrictor> CreateRestrictors(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var result = new List<ITransferRestrictor>();
            foreach(var restrComp in Restrictors)
            {
                result.Add(restrComp.CreateRestrictor(taskable, sharedState, localState, netContext));
            }

            return result;
        }

        protected ITransferResultDecider CreateResultDecider(ITaskable taskable)
        {
            var typeBox = Unwrap(ResultDeciderCombo);

            if (typeBox.Selected == null)
                return null;

            int parsed;
            Material mat;
            switch(typeBox.Selected.Value)
            {
                case ResultDeciderType.ByItemsTransferred:
                    mat = null;
                    if(Unwrap(ResultDeciderByItemsTransfered_MaterialCheck).Pushed)
                    {
                        var box = Unwrap(ResultDeciderByItemsTransfered_MaterialBox);
                        mat = box.Selected == null ? null : box.Selected.Value;
                    }
                    if (!int.TryParse(Unwrap(ResultDeciderByItemsTransfered_Field).Text, out parsed))
                        parsed = 0;
                    return new ItemsTransferedResultDecider(mat, parsed, EntityTaskStatus.Failure);
                case ResultDeciderType.ByDeliveringInventory:
                    mat = null;
                    if (Unwrap(ResultDeciderByDeliveringItems_MaterialCheck).Pushed)
                    {
                        var box = Unwrap(ResultDeciderByDeliveringItems_MaterialBox);
                        mat = box.Selected == null ? null : box.Selected.Value;
                    }
                    if (!int.TryParse(Unwrap(ResultDeciderByDeliveringItems_Field).Text, out parsed))
                        parsed = 0;
                    return new FromInventoryResultDecider(mat, parsed, EntityTaskStatus.Failure);
                case ResultDeciderType.ByReceivingInventory:
                    mat = null;
                    if (Unwrap(ResultDeciderByReceivingItems_MaterialCheck).Pushed)
                    {
                        var box = Unwrap(ResultDeciderByReceivingItems_MaterialBox);
                        mat = box.Selected == null ? null : box.Selected.Value;
                    }
                    if (!int.TryParse(Unwrap(ResultDeciderByReceivingItems_Field).Text, out parsed))
                        parsed = 0;
                    return new ToInventoryResultDecider(mat, parsed, EntityTaskStatus.Failure);
                default:
                    throw new InvalidOperationException($"Unknown result decider type {typeBox.Selected.Value}");
            }
        }

        public override bool IsValid()
        {
            ScrollableComponentFromScreenComponent<RadioButton> pickupWrapped;
            if (!TryGetWrapped(PickupRadio, out pickupWrapped))
            {
                if (Task == null)
                    return false;
                else
                {
                    return Task.IsValid();
                }
            }

            if (!Unwrap(PickupRadio).Pushed && !Unwrap(DropoffRadio).Pushed)
                return false;
            if (!IsTargetDeciderValid())
                return false;
            if (!AreRestrictorsValid())
                return false;
            if (!IsResultDeciderValid())
                return false;

            return true;
        }

        protected bool IsTargetDeciderValid()
        {
            var box = Unwrap(TargeterCombo);

            if (box.Selected == null)
                return false;
            
            int parsed;
            switch(box.Selected.Value)
            {
                case TargetDeciderType.ByID:
                    if (!int.TryParse(Unwrap(TargetByID_Field).Text, out parsed))
                        return false;
                    return true;
                case TargetDeciderType.ByPosition:
                    if (!int.TryParse(Unwrap(TargetByPosition_XField).Text, out parsed))
                        return false;
                    if (!int.TryParse(Unwrap(TargetByPosition_YField).Text, out parsed))
                        return false;
                    return true;
                case TargetDeciderType.ByRelativePosition:
                    if (!int.TryParse(Unwrap(TargetByRelPosition_DXField).Text, out parsed))
                        return false;
                    if (!int.TryParse(Unwrap(TargetByRelPosition_DYField).Text, out parsed))
                        return false;
                    return true;
                default:
                    throw new InvalidOperationException($"Unknown targeter type {box.Selected.Value}");
            }
        }

        protected bool AreRestrictorsValid()
        {
            foreach(var restr in Restrictors)
            {
                if (!restr.IsValid())
                    return false;
            }
            return true;
        }

        protected bool IsResultDeciderValid()
        {
            var typeBox = Unwrap(ResultDeciderCombo);

            CheckBox check = null;
            ComboBox<Material> matBox = null;
            TextField field = null;

            if (typeBox.Selected == null)
                return false;

            switch(typeBox.Selected.Value)
            {
                case ResultDeciderType.ByItemsTransferred:
                    check = Unwrap(ResultDeciderByItemsTransfered_MaterialCheck);
                    matBox = Unwrap(ResultDeciderByItemsTransfered_MaterialBox);
                    field = Unwrap(ResultDeciderByItemsTransfered_Field);
                    break;
                case ResultDeciderType.ByDeliveringInventory:
                    check = Unwrap(ResultDeciderByDeliveringItems_MaterialCheck);
                    matBox = Unwrap(ResultDeciderByDeliveringItems_MaterialBox);
                    field = Unwrap(ResultDeciderByDeliveringItems_Field);
                    break;
                case ResultDeciderType.ByReceivingInventory:
                    check = Unwrap(ResultDeciderByReceivingItems_MaterialCheck);
                    matBox = Unwrap(ResultDeciderByReceivingItems_MaterialBox);
                    field = Unwrap(ResultDeciderByReceivingItems_Field);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown result decider type {typeBox.Selected.Value}");
            }

            if (check.Pushed && matBox.Selected == null)
                return false;
            int parsed;
            if (!int.TryParse(field.Text, out parsed))
                return false;
            if (parsed <= 0)
                return false;
            return true;
        }

        protected override void LoadFromTask(RenderContext context)
        {
            var task = Task as EntityTransferItemTask;
            LoadPickupDropoffFromTask(context, task);
            LoadTargeterFromTask(context, task);
            LoadRestrictorsFromTask(context, task);
            LoadResultDeciderFromTask(context, task);

            Reload = true;
            OnInspectRedrawRequired();
        }

        protected void LoadPickupDropoffFromTask(RenderContext context, EntityTransferItemTask task)
        {
            if (task.Pickup)
                Unwrap(PickupRadio).Pushed = true;
            else
                Unwrap(DropoffRadio).Pushed = true;
        }

        protected void LoadTargeterFromTask(RenderContext context, EntityTransferItemTask task)
        {
            var targeter = task.Targeter;

            if (targeter == null)
                return;

            var box = Unwrap(TargeterCombo);
            if(targeter.GetType() == typeof(TransferTargetByID))
            {
                var byid = targeter as TransferTargetByID;

                box.Selected = box.GetComboItemByValue(TargetDeciderType.ByID);
                Unwrap(TargetByID_Field).Text = byid.TargetID.ToString();
            }else if(targeter.GetType() == typeof(TransferTargetByPosition))
            {
                var bypos = targeter as TransferTargetByPosition;
                box.Selected = box.GetComboItemByValue(TargetDeciderType.ByPosition);
                Unwrap(TargetByPosition_XField).Text = bypos.Position.X.ToString();
                Unwrap(TargetByPosition_YField).Text = bypos.Position.Y.ToString();
            }else if(targeter.GetType() == typeof(TransferTargetByRelativePosition))
            {
                var byrelpos = targeter as TransferTargetByRelativePosition;
                box.Selected = box.GetComboItemByValue(TargetDeciderType.ByRelativePosition);
                Unwrap(TargetByRelPosition_DXField).Text = ((int)byrelpos.Offset.DeltaX).ToString();
                Unwrap(TargetByRelPosition_DYField).Text = ((int)byrelpos.Offset.DeltaY).ToString();
            }else
            {
                throw new InvalidProgramException($"Unknown targeter {targeter.GetType().FullName}");
            }
        }

        protected void LoadRestrictorsFromTask(RenderContext context, EntityTransferItemTask task)
        {
            if (task.Restrictors == null)
                return;

            foreach(var restrictor in task.Restrictors)
            {
                var restrComp = new TransferRestrictorComponent(this, restrictor);

                restrComp.InitializeComponent(context, (sender, args) => OnInspectRedrawRequired(), (sender, args) => { Reload = true; OnInspectRedrawRequired(); });
                restrComp.LoadFromOriginal();

                AddRestrictor(restrComp);
            }
        }

        protected void LoadResultDeciderFromTask(RenderContext context, EntityTransferItemTask task)
        {
            if (task.ResultDecider == null)
                return;

            var box = Unwrap(ResultDeciderCombo);
            var resultType = task.ResultDecider.GetType();

            if (resultType == typeof(ItemsTransferedResultDecider))
            {
                var decider = task.ResultDecider as ItemsTransferedResultDecider;

                box.Selected = box.GetComboItemByValue(ResultDeciderType.ByItemsTransferred);

                if (decider.KeyMaterial != null)
                {
                    Unwrap(ResultDeciderByItemsTransfered_MaterialCheck).Pushed = true;
                    var matBox = Unwrap(ResultDeciderByItemsTransfered_MaterialBox);
                    matBox.Selected = matBox.GetComboItemByValue(decider.KeyMaterial);
                }

                Unwrap(ResultDeciderByItemsTransfered_Field).Text = decider.KeyQuantity.ToString();
            }
            else if (resultType == typeof(ToInventoryResultDecider))
            {
                var decider = task.ResultDecider as ToInventoryResultDecider;

                box.Selected = box.GetComboItemByValue(ResultDeciderType.ByReceivingInventory);

                if (decider.KeyMaterial != null)
                {
                    Unwrap(ResultDeciderByReceivingItems_MaterialCheck).Pushed = true;
                    var matBox = Unwrap(ResultDeciderByReceivingItems_MaterialBox);
                    matBox.Selected = matBox.GetComboItemByValue(decider.KeyMaterial);
                }

                Unwrap(ResultDeciderByReceivingItems_Field).Text = decider.KeyQuantity.ToString();
            }
            else if (resultType == typeof(FromInventoryResultDecider))
            {
                var decider = task.ResultDecider as FromInventoryResultDecider;

                box.Selected = box.GetComboItemByValue(ResultDeciderType.ByDeliveringInventory);

                if (decider.KeyMaterial != null)
                {
                    Unwrap(ResultDeciderByDeliveringItems_MaterialCheck).Pushed = true;
                    var matBox = Unwrap(ResultDeciderByDeliveringItems_MaterialBox);
                    matBox.Selected = matBox.GetComboItemByValue(decider.KeyMaterial);
                }

                Unwrap(ResultDeciderByDeliveringItems_Field).Text = decider.KeyQuantity.ToString();
            }
            else
            {
                throw new InvalidOperationException($"Unknown result decider {resultType.FullName}");
            }
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
