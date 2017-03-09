using BaseBuilder.Engine.Context;
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
            ByItemType,
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

        protected TextField TargetDeciderByID_IDTextField;

        protected TextField TargetDeciderByPosition_XField;
        protected TextField TargetDeciderByPosition_YField;

        protected TextField TargetDeciderByRelPosition_XField;
        protected TextField TargetDeciderbyRelPosition_YField;

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
                if(PickupRadio != null)
                {
                    Components.Remove(PickupRadio);
                    PickupRadio.Dispose();
                    PickupRadio = null;
                }
                if(DropoffRadio != null)
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
                    } else
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

            if(PickupRadioLabel == null)
            {
                PickupRadioLabel = new Text(new Point(0, 0), "Pickup", renderContext.DefaultFont, Color.Black);

                Components.Add(PickupRadioLabel);
            }

            if(DropoffRadioLabel == null)
            {
                DropoffRadioLabel = new Text(new Point(0, 0), "Dropoff", renderContext.DefaultFont, Color.Black);

                Components.Add(DropoffRadioLabel);
            }

            if(PickupTypeCombo == null)
            {
                PickupTypeCombo = new ComboBox<PickupType>(new List<ComboBoxItem<PickupType>>
                {
                    new ComboBoxItem<PickupType>(renderContext.DefaultFont, "Standard Pickup", PickupType.Simple)
                }, new Point(150, 34));

                PickupTypeCombo.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                PickupTypeCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                PickupTypeCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if(PickupTypeLabel == null)
            {
                PickupTypeLabel = new Text(new Point(0, 0), "Pickup Type", renderContext.DefaultFont, Color.Black);
            }

            if(DropoffTypeCombo == null)
            {
                DropoffTypeCombo = new ComboBox<DropoffType>(new List<ComboBoxItem<DropoffType>>
                {
                    new ComboBoxItem<DropoffType>(renderContext.DefaultFont, "Standard Dropoff", DropoffType.Simple)
                }, new Point(150, 34));

                DropoffTypeCombo.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                DropoffTypeCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                DropoffTypeCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if(DropoffTypeLabel == null)
            {
                DropoffTypeLabel = new Text(new Point(0, 0), "Dropoff Type", renderContext.DefaultFont, Color.Black);
            }
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
            if(DropoffRadio.Pushed)
            {
                height += DropoffTypeLabel.Size.Y + 3;
                DropoffTypeCombo.Center = new Point(width / 2, height + DropoffTypeCombo.Size.Y / 2);
                DropoffTypeLabel.Center = GetCenterForTopLeftLabel(DropoffTypeLabel, DropoffTypeCombo);
                height += DropoffTypeCombo.Size.Y + 3;
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

            foreach(var comp in Components)
            {
                comp.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            }
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            if(ButtonShiftLast.X != x || ButtonShiftLast.Y != y)
            {
                foreach(var comp in Components)
                {
                    comp.Center = new Point(comp.Center.X - ButtonShiftLast.X + x, comp.Center.Y - ButtonShiftLast.Y + y);
                }
            }

            base.DrawInspect(context, x, y);

            foreach(var comp in Components)
            {
                comp.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            }
        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);

            foreach(var comp in Components)
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

            foreach(var comp in Components)
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

            Components.Clear();
        }
    }
}
