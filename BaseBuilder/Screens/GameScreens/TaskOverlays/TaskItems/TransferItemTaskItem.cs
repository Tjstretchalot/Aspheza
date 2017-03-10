﻿using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /*
     * Interface Overview:
     *   1. Choosing the transferer:
     *     Radio buttons for pickup/dropdown. Initially both radio buttons are unselected
     *     
     *     Pickup pressed causes the following dropbox to appear, defaulting with Simple Pickup
     *       Simple Pickup
     *         (when selected the following text is written beneath it):
     *         Picks up items from the target.
     *     
     *     Dropdown pressed causes the following to appear, defaulting with Simple Dropoff
     *       Simple Dropoff
     *         Gives items to the target.
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
     *     
     *     
     */

    public class TransferItemTaskItem : SimpleTaskItem
    {
        protected enum PickupType
        {
            Simple
        }

        protected enum DropoffType
        {
            Simple
        }

        protected enum TargetDeciderType
        {
            ByID,
            ByPosition,
            ByRelativePosition
        }

        protected enum TransferRestrictorType
        {
            ByItemType,
            ByTotalQuantity,
            ByRecievingInventory,
            ByOurInventory
        }
        protected class TransferRestrictorComp
        {
            public ComboBox<TransferRestrictorType> TypeBox;

            public RadioButton ByItemType_AllowRadio;
            public RadioButton ByItemType_DenyRadio;

            public ComboBox<Material> MaterialBox;

            public TextField ByTotalQuantity_MaxField;

            public CheckBox ByRecievingInventory_TypeCheck;
            public ComboBox<Material> ByRecievingInventory_TypeBox;
            public Text ByRecievingInventory_FieldLabel;
            public TextField ByRecievingInventory_Field;

            public CheckBox ByOurInventory_TypeCheck;
            public ComboBox<Material> ByOurInventory_TypeBox;
            public Text ByOurInventory_FieldLabel;
            public TextField ByOurInventory_Field;

            public List<IMyGameComponent> Components;
        }

        protected const string _InspectDescription = @"A transfer item task is a leaf task that transfers an 
item by type or inventory position(s) to/from this 
entity to/from an entity by ID, position, or relative 
position. 

Restrictions may be placed on how many items are 
transfered based on how many items would remain in
the giving inventory, how many items would be 
contained in the recieving inventory, and how many 
items have been transfered so far.

The result of a transfer item task can be selected
based on items in the giving inventory, items in
the recieving inventory, and items transfered so
far.";


        protected RadioButton PickupRadio;
        protected Text PickupRadioLabel;
        protected RadioButton DropoffRadio;
        protected Text DropoffRadioLabel;

        protected Text PickupTypeLabel;
        protected ComboBox<PickupType> PickupTypeCombo;
        protected Text DropoffTypeLabel;
        protected ComboBox<DropoffType> DropoffTypeCombo;

        protected Text TargetDeciderLabel;
        protected ComboBox<TargetDeciderType> TargetDeciderCombo;

        protected Text TargetDeciderByID_DescriptionText;
        protected Text TargetDeciderByID_IDLabel;
        protected TextField TargetDeciderByID_IDTextField;

        protected Text TargetDeciderByPosition_DescriptionText;
        protected Text TargetDeciderByPosition_XLabel;
        protected TextField TargetDeciderByPosition_XField;
        protected Text TargetDeciderByPosition_YLabel;
        protected TextField TargetDeciderByPosition_YField;

        protected Text TargetDeciderByRelPosition_DescriptionText;
        protected Text TargetDeciderByRelPosition_DXLabel;
        protected TextField TargetDeciderByRelPosition_DXField;
        protected Text TargetDeciderByRelPosition_DYLabel;
        protected TextField TargetDeciderByRelPosition_DYField;

        protected List<TransferRestrictorComp> Restrictors;
        protected Button AddRestrictorButton;

        /// <summary>
        /// Contains all of the components  of this task item (an alternative
        /// for going through each thing above one by one)
        /// </summary>
        protected List<IScreenComponent> Components;

        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public TransferItemTaskItem(EntityTransferItemTask task) : this()
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
        }

        /// <summary>
        /// Creates a give item task item with no child. This will
        /// mean the give item task item is in a bad state.
        /// </summary>
        public TransferItemTaskItem()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Transfer Item";

            Components = new List<IScreenComponent>();
        }

        protected override void InitializeThings(RenderContext renderContext)
        {
            base.InitializeThings(renderContext);

            if (PickupRadio == null || DropoffRadio == null)
            {
                if (PickupRadio != null)
                {
                    Components.Remove(PickupRadio);
                    PickupRadio.Dispose();
                    PickupRadio = null;
                }
                if (DropoffRadio != null)
                {
                    Components.Remove(DropoffRadio);
                    DropoffRadio.Dispose();
                    DropoffRadio = null;
                }
                PickupRadio = new RadioButton(new Point(0, 0));
                DropoffRadio = new RadioButton(new Point(0, 0));

                PickupRadio.PushedChanged += (sender, args) =>
                {
                    if (PickupRadio.Pushed)
                    {
                        Components.Add(PickupTypeLabel);
                        Components.Add(PickupTypeCombo);
                    }
                    else
                    {
                        Components.Remove(PickupTypeLabel);
                        Components.Remove(PickupTypeCombo);
                    }

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                DropoffRadio.PushedChanged += (sender, args) =>
                {
                    if (DropoffRadio.Pushed)
                    {
                        Components.Add(DropoffTypeLabel);
                        Components.Add(DropoffTypeCombo);
                    }
                    else
                    {
                        Components.Remove(DropoffTypeLabel);
                        Components.Remove(DropoffTypeCombo);
                    }

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                var group = new RadioButtonGroup(new List<RadioButton> { PickupRadio, DropoffRadio });
                group.Attach();

                Components.Add(PickupRadio);
                Components.Add(DropoffRadio);
            }

            if (PickupRadioLabel == null)
            {
                PickupRadioLabel = new Text(new Point(0, 0), "Pickup", renderContext.DefaultFont, Color.Black);

                Components.Add(PickupRadioLabel);
            }

            if (DropoffRadioLabel == null)
            {
                DropoffRadioLabel = new Text(new Point(0, 0), "Dropoff", renderContext.DefaultFont, Color.Black);

                Components.Add(DropoffRadioLabel);
            }

            if (PickupTypeCombo == null)
            {
                PickupTypeCombo = new ComboBox<PickupType>(new List<ComboBoxItem<PickupType>>
                {
                    new ComboBoxItem<PickupType>(renderContext.DefaultFont, "Standard Pickup", PickupType.Simple)
                }, new Point(150, 34));

                PickupTypeCombo.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                PickupTypeCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                PickupTypeCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (PickupTypeLabel == null)
            {
                PickupTypeLabel = new Text(new Point(0, 0), "Pickup Type", renderContext.DefaultFont, Color.Black);
            }

            if (DropoffTypeCombo == null)
            {
                DropoffTypeCombo = new ComboBox<DropoffType>(new List<ComboBoxItem<DropoffType>>
                {
                    new ComboBoxItem<DropoffType>(renderContext.DefaultFont, "Standard Dropoff", DropoffType.Simple)
                }, new Point(150, 34));

                DropoffTypeCombo.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                DropoffTypeCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                DropoffTypeCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (DropoffTypeLabel == null)
            {
                DropoffTypeLabel = new Text(new Point(0, 0), "Dropoff Type", renderContext.DefaultFont, Color.Black);
            }

            if (TargetDeciderCombo == null)
            {
                TargetDeciderCombo = new ComboBox<TargetDeciderType>(new List<ComboBoxItem<TargetDeciderType>>
                {
                    new ComboBoxItem<TargetDeciderType>(renderContext.DefaultFont, "By Entity ID", TargetDeciderType.ByID),
                    new ComboBoxItem<TargetDeciderType>(renderContext.DefaultFont, "By Position", TargetDeciderType.ByPosition),
                    new ComboBoxItem<TargetDeciderType>(renderContext.DefaultFont, "By Relative Position", TargetDeciderType.ByRelativePosition),
                }, new Point(200, 34));

                TargetDeciderCombo.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                TargetDeciderCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                TargetDeciderCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();

                TargetDeciderCombo.SelectedChanged += (sender, oldSelected) =>
                {
                    if (TargetDeciderCombo.Selected == null || oldSelected?.Value == TargetDeciderCombo.Selected.Value)
                        return;
                    
                    if (oldSelected?.Value == TargetDeciderType.ByID)
                    {
                        Components.Remove(TargetDeciderByID_DescriptionText);
                        Components.Remove(TargetDeciderByID_IDLabel);
                        Components.Remove(TargetDeciderByID_IDTextField);
                    }
                    else if (oldSelected?.Value == TargetDeciderType.ByPosition)
                    {
                        Components.Remove(TargetDeciderByPosition_DescriptionText);
                        Components.Remove(TargetDeciderByPosition_XField);
                        Components.Remove(TargetDeciderByPosition_XLabel);
                        Components.Remove(TargetDeciderByPosition_YField);
                        Components.Remove(TargetDeciderByPosition_YLabel);
                    }
                    else if (oldSelected?.Value == TargetDeciderType.ByRelativePosition)
                    {
                        Components.Remove(TargetDeciderByRelPosition_DescriptionText);
                        Components.Remove(TargetDeciderByRelPosition_DXField);
                        Components.Remove(TargetDeciderByRelPosition_DXLabel);
                        Components.Remove(TargetDeciderByRelPosition_DYField);
                        Components.Remove(TargetDeciderByRelPosition_DYLabel);
                    }

                    var insertIndex = Components.FindIndex((c) => ReferenceEquals(c, TargetDeciderCombo));

                    if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByID)
                    {
                        Components.Insert(insertIndex, TargetDeciderByID_DescriptionText);
                        Components.Add(TargetDeciderByID_IDLabel);
                        Components.Add(TargetDeciderByID_IDTextField);
                    }
                    else if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByPosition)
                    {
                        Components.Insert(insertIndex, TargetDeciderByPosition_DescriptionText);
                        Components.Add(TargetDeciderByPosition_XField);
                        Components.Add(TargetDeciderByPosition_XLabel);
                        Components.Add(TargetDeciderByPosition_YField);
                        Components.Add(TargetDeciderByPosition_YLabel);
                    }
                    else if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByRelativePosition)
                    {
                        Components.Insert(insertIndex, TargetDeciderByRelPosition_DescriptionText);
                        Components.Add(TargetDeciderByRelPosition_DXField);
                        Components.Add(TargetDeciderByRelPosition_DYField);
                        Components.Add(TargetDeciderByRelPosition_DXLabel);
                        Components.Add(TargetDeciderByRelPosition_DYLabel);
                    }

                    OnInspectRedrawRequired();
                    Reload = true;
                };

                TargetDeciderCombo.Selected = null;

                Components.Add(TargetDeciderCombo);
            }

            if (TargetDeciderLabel == null)
            {
                TargetDeciderLabel = new Text(new Point(0, 0), "Targetting Method", renderContext.DefaultFont, Color.Black);
                Components.Add(TargetDeciderLabel);
            }

            if (TargetDeciderByID_DescriptionText == null)
            {
                TargetDeciderByID_DescriptionText = new Text(new Point(0, 0), @"
Select the target by searching for the entity 
with the specified ID. This is a good choice 
if you always want to transfer items with 
the same entity", renderContext.DefaultFont, Color.White);
            }

            if (TargetDeciderByID_IDTextField == null)
            {
                TargetDeciderByID_IDTextField = CreateTextField(150, 30);
            }

            if (TargetDeciderByID_IDLabel == null)
            {
                TargetDeciderByID_IDLabel = new Text(new Point(0, 0), "Target ID", renderContext.DefaultFont, Color.Black);
            }
            
            if(TargetDeciderByPosition_DescriptionText == null)
            {
                TargetDeciderByPosition_DescriptionText = new Text(new Point(0, 0), @"Select the target by searching for the entity 
at the specified position. This is helpful if
you want to be able to swap out the target 
without modifying this entity.", renderContext.DefaultFont, Color.White);
            }

            if(TargetDeciderByPosition_XField == null)
            {
                TargetDeciderByPosition_XField = CreateTextField(70, 30);
            }

            if(TargetDeciderByPosition_XLabel == null)
            {
                TargetDeciderByPosition_XLabel = new Text(new Point(0, 0), "X", renderContext.DefaultFont, Color.Black);
            }

            if(TargetDeciderByPosition_YField == null)
            {
                TargetDeciderByPosition_YField = CreateTextField(70, 30);
            }

            if(TargetDeciderByPosition_YLabel == null)
            {
                TargetDeciderByPosition_YLabel = new Text(new Point(0, 0), "Y", renderContext.DefaultFont, Color.Black);
            }

            if(TargetDeciderByRelPosition_DescriptionText == null)
            {
                TargetDeciderByRelPosition_DescriptionText = new Text(new Point(0, 0), @"Select the target by searching for an entity 
at a position relative to this entity at the 
time that this task is evaluated. This is 
often helpful for complicated and/or flexible 
behavior trees.", renderContext.DefaultFont, Color.White);
            }

            if (TargetDeciderByRelPosition_DXField == null)
            {
                TargetDeciderByRelPosition_DXField = CreateTextField(70, 30);
            }

            if (TargetDeciderByRelPosition_DXLabel == null)
            {
                TargetDeciderByRelPosition_DXLabel = new Text(new Point(0, 0), "DeltaX", renderContext.DefaultFont, Color.Black);
            }

            if(TargetDeciderByRelPosition_DYField == null)
            {
                TargetDeciderByRelPosition_DYField = CreateTextField(70, 30);
            }

            if(TargetDeciderByRelPosition_DYLabel == null)
            {
                TargetDeciderByRelPosition_DYLabel = new Text(new Point(0, 0), "DeltaY", renderContext.DefaultFont, Color.Black);
            }
        }

        /// <summary>
        /// Creates a text field with the specified width and height with all the necessary
        /// redrawing hooks created. Does not add to components.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>The text field</returns>
        TextField CreateTextField(int width, int height)
        {
            var result = UIUtils.CreateTextField(new Point(0, 0), new Point(width, height));

            result.FocusGained += (sender, args) => OnInspectRedrawRequired();
            result.FocusLost += (sender, args) => OnInspectRedrawRequired();
            result.TextChanged += (sender, args) =>
            {
                UIUtils.TextFieldNumbersOnly(sender, args);
                OnInspectRedrawRequired();
            };
            return result;
        }

        Point GetCenterForTopLeftLabel(Text label, IScreenComponent comp)
        {
            return new Point(comp.Center.X - comp.Size.X / 2 + label.Size.X / 2, comp.Center.Y - comp.Size.Y / 2 - label.Size.Y / 2 - 3);
        }

        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {

            var neededHeight = Math.Max(Math.Max(DropoffRadio.Size.Y, PickupRadio.Size.Y), Math.Max(DropoffRadioLabel.Size.Y, PickupRadioLabel.Size.Y));
            var neededWidth = PickupRadio.Size.X + 3 + PickupRadioLabel.Size.X + 7 + DropoffRadio.Size.X + 3 + DropoffRadioLabel.Size.X;
            var paddingLeft = (width - neededWidth) / 2;
            PickupRadio.Center = new Point(paddingLeft + PickupRadio.Size.X / 2, height + neededHeight / 2);
            PickupRadioLabel.Center = new Point(paddingLeft + PickupRadio.Size.X + 3 + PickupRadioLabel.Size.X / 2, height + neededHeight / 2);
            DropoffRadio.Center = new Point(paddingLeft + PickupRadio.Size.X + 3 + PickupRadioLabel.Size.X + 7 + DropoffRadio.Size.X / 2, height + neededHeight / 2);
            DropoffRadioLabel.Center = new Point(paddingLeft + PickupRadio.Size.X + 3 + PickupRadioLabel.Size.X + 7 + DropoffRadio.Size.X + 3 + DropoffRadioLabel.Size.X / 2, height + neededHeight / 2);

            height += neededHeight + 5;

            if (PickupRadio.Pushed)
            {
                height += PickupTypeLabel.Size.Y + 3;
                PickupTypeCombo.Center = new Point(width / 2, height + PickupTypeCombo.Size.Y / 2);
                PickupTypeLabel.Center = GetCenterForTopLeftLabel(PickupTypeLabel, PickupTypeCombo);
                height += PickupTypeCombo.Size.Y + 3;
            }
            if (DropoffRadio.Pushed)
            {
                height += DropoffTypeLabel.Size.Y + 3;
                DropoffTypeCombo.Center = new Point(width / 2, height + DropoffTypeCombo.Size.Y / 2);
                DropoffTypeLabel.Center = GetCenterForTopLeftLabel(DropoffTypeLabel, DropoffTypeCombo);
                height += DropoffTypeCombo.Size.Y + 3;
            }

            height += 8;

            height += TargetDeciderLabel.Size.Y + 3;
            TargetDeciderCombo.Center = new Point(width / 2, height + TargetDeciderCombo.Size.Y / 2);
            TargetDeciderLabel.Center = GetCenterForTopLeftLabel(TargetDeciderLabel, TargetDeciderCombo);
            height += TargetDeciderCombo.Size.Y + 5;

            if (TargetDeciderCombo.Selected != null)
            {
                int reqWidth, padLeft;
                switch (TargetDeciderCombo.Selected.Value)
                {
                    case TargetDeciderType.ByID:
                        TargetDeciderByID_DescriptionText.Center = new Point(width / 2, height + TargetDeciderByID_DescriptionText.Size.Y / 2);
                        height += TargetDeciderByID_DescriptionText.Size.Y + 3;

                        height += TargetDeciderByID_IDLabel.Size.Y + 3;
                        TargetDeciderByID_IDTextField.Center = new Point(width / 2, height + TargetDeciderByID_IDTextField.Size.Y / 2);
                        TargetDeciderByID_IDLabel.Center = GetCenterForTopLeftLabel(TargetDeciderByID_IDLabel, TargetDeciderByID_IDTextField);
                        height += TargetDeciderByID_IDTextField.Size.Y + 3;
                        break;
                    case TargetDeciderType.ByPosition:
                        TargetDeciderByPosition_DescriptionText.Center = new Point(width / 2, height + TargetDeciderByPosition_DescriptionText.Size.Y / 2);
                        height += TargetDeciderByPosition_DescriptionText.Size.Y + 3;

                        reqWidth = TargetDeciderByPosition_XField.Size.X + 5 + TargetDeciderByPosition_YField.Size.X;
                        padLeft = (width - reqWidth) / 2;

                        height += Math.Max(TargetDeciderByPosition_XLabel.Size.Y, TargetDeciderByPosition_YLabel.Size.Y) + 3;
                        TargetDeciderByPosition_XField.Center = new Point(padLeft + TargetDeciderByPosition_XField.Size.X / 2, height + TargetDeciderByPosition_XField.Size.Y / 2);
                        TargetDeciderByPosition_YField.Center = new Point(padLeft + TargetDeciderByPosition_XField.Size.X + 5 + TargetDeciderByPosition_YField.Size.X / 2, height + TargetDeciderByPosition_YField.Size.Y / 2);
                        TargetDeciderByPosition_XLabel.Center = GetCenterForTopLeftLabel(TargetDeciderByPosition_XLabel, TargetDeciderByPosition_XField);
                        TargetDeciderByPosition_YLabel.Center = GetCenterForTopLeftLabel(TargetDeciderByPosition_YLabel, TargetDeciderByPosition_YField);
                        height += Math.Max(TargetDeciderByPosition_XField.Size.Y, TargetDeciderByPosition_YField.Size.Y) + 3;
                        break;
                    case TargetDeciderType.ByRelativePosition:
                        TargetDeciderByRelPosition_DescriptionText.Center = new Point(width / 2, height + TargetDeciderByRelPosition_DescriptionText.Size.Y / 2);
                        height += TargetDeciderByRelPosition_DescriptionText.Size.Y + 3;

                        reqWidth = TargetDeciderByRelPosition_DXField.Size.X + 5 + TargetDeciderByRelPosition_DYField.Size.X;
                        padLeft = (width - reqWidth) / 2;

                        height += Math.Max(TargetDeciderByRelPosition_DXLabel.Size.Y, TargetDeciderByRelPosition_DYLabel.Size.Y) + 3;
                        TargetDeciderByRelPosition_DXField.Center = new Point(padLeft + TargetDeciderByRelPosition_DXField.Size.X / 2, height + TargetDeciderByRelPosition_DXField.Size.Y / 2);
                        TargetDeciderByRelPosition_DYField.Center = new Point(padLeft + TargetDeciderByRelPosition_DXField.Size.X + 5 + TargetDeciderByRelPosition_DYField.Size.X / 2, height + TargetDeciderByRelPosition_DYField.Size.Y / 2);
                        TargetDeciderByRelPosition_DXLabel.Center = GetCenterForTopLeftLabel(TargetDeciderByRelPosition_DXLabel, TargetDeciderByRelPosition_DXField);
                        TargetDeciderByRelPosition_DYLabel.Center = GetCenterForTopLeftLabel(TargetDeciderByRelPosition_DYLabel, TargetDeciderByRelPosition_DYField);
                        height += Math.Max(TargetDeciderByRelPosition_DXField.Size.Y, TargetDeciderByRelPosition_DYField.Size.Y) + 3;
                        break;
                }
            }

            height += 8;
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
            height += 150;
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return new EntityTransferItemTask(taskable as Container, -1, new List<ITransferRestrictor>(), new TransferResultDecider());
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 0;
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            base.PreDrawInspect(context, x, y);

            foreach (var comp in Components)
            {
                comp.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            }
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            if (ButtonShiftLast.X != x || ButtonShiftLast.Y != y)
            {
                foreach (var comp in Components)
                {
                    comp.Center = new Point(comp.Center.X - ButtonShiftLast.X + x, comp.Center.Y - ButtonShiftLast.Y + y);
                }
            }

            base.DrawInspect(context, x, y);

            foreach (var comp in Components)
            {
                comp.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            }
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);

            foreach (var comp in Components)
            {
                comp.Update(Content, timeMS);
            }
        }

        protected override void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                Components[i].HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
            }

            base.HandleInspectComponentsMouseState(last, current, ref handled, ref scrollHandled);
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            var handled = false;

            foreach (var comp in Components)
            {
                comp.HandleKeyboardState(Content, last, current, ref handled);
            }

            return handled;
        }

        public override void DisposeInspect()
        {
            base.DisposeInspect();

            PickupRadio?.Dispose();
            PickupRadio = null;

            PickupRadioLabel?.Dispose();
            PickupRadioLabel = null;

            DropoffRadio?.Dispose();
            DropoffRadio = null;

            DropoffRadioLabel?.Dispose();
            DropoffRadioLabel = null;

            PickupTypeCombo?.Dispose();
            PickupTypeCombo = null;

            DropoffTypeLabel?.Dispose();
            DropoffTypeLabel = null;

            DropoffTypeCombo?.Dispose();
            DropoffTypeCombo = null;

            TargetDeciderCombo?.Dispose();
            TargetDeciderCombo = null;

            TargetDeciderLabel?.Dispose();
            TargetDeciderLabel = null;

            TargetDeciderByID_DescriptionText?.Dispose();
            TargetDeciderByID_DescriptionText = null;

            TargetDeciderByID_IDLabel?.Dispose();
            TargetDeciderByID_IDLabel = null;

            TargetDeciderByID_IDTextField?.Dispose();
            TargetDeciderByID_IDTextField = null;

            TargetDeciderByPosition_DescriptionText?.Dispose();
            TargetDeciderByPosition_DescriptionText = null;

            TargetDeciderByPosition_XField?.Dispose();
            TargetDeciderByPosition_XField = null;

            TargetDeciderByPosition_XLabel?.Dispose();
            TargetDeciderByPosition_XLabel = null;

            TargetDeciderByPosition_YField?.Dispose();
            TargetDeciderByPosition_YField = null;

            TargetDeciderByPosition_YLabel?.Dispose();
            TargetDeciderByPosition_YLabel = null;

            TargetDeciderByRelPosition_DescriptionText?.Dispose();
            TargetDeciderByRelPosition_DescriptionText = null;

            TargetDeciderByRelPosition_DXLabel?.Dispose();
            TargetDeciderByRelPosition_DXLabel = null;

            TargetDeciderByRelPosition_DXField?.Dispose();
            TargetDeciderByRelPosition_DXField = null;

            TargetDeciderByRelPosition_DYLabel?.Dispose();
            TargetDeciderByRelPosition_DYLabel = null;

            Components.Clear();
        }
    }
}