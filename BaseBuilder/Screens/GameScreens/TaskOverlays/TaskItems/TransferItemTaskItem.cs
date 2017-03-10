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
            protected TransferItemTaskItem Outer;

            public Text TypeBoxLabel;
            public ComboBox<TransferRestrictorType> TypeBox;

            public Text ByItemType_Description;
            public Text ByItemType_AllowLabel;
            public RadioButton ByItemType_AllowRadio;
            public Text ByItemType_DenyLabel;
            public RadioButton ByItemType_DenyRadio;
            public Text ByItemType_MaterialLabel;
            public ComboBox<Material> ByItemType_MaterialBox;

            public Text ByTotalQuantity_Description;
            public Text ByTotalQuantity_MaxLabel;
            public TextField ByTotalQuantity_MaxField;

            public Text ByReceivingInventory_Description;
            public Text ByReceivingInventory_TypeCheckLabel;
            public CheckBox ByReceivingInventory_TypeCheck;
            public Text ByReceivingInventory_TypeBoxLabel;
            public ComboBox<Material> ByReceivingInventory_TypeBox;
            public Text ByReceivingInventory_FieldLabel;
            public TextField ByReceivingInventory_Field;

            public Text ByOurInventory_Description;
            public Text ByOurInventory_TypeCheckLabel;
            public CheckBox ByOurInventory_TypeCheck;
            public Text ByOurInventory_TypeBoxLabel;
            public ComboBox<Material> ByOurInventory_TypeBox;
            public Text ByOurInventory_FieldLabel;
            public TextField ByOurInventory_Field;

            public Button DeleteButton;

            public List<IScreenComponent> Components;

            public TransferRestrictorComp(TransferItemTaskItem outer)
            {
                Outer = outer;
                Components = new List<IScreenComponent>();
            }

            public void InitializeThings(RenderContext renderContext)
            {
                if(TypeBoxLabel == null)
                {
                    TypeBoxLabel = new Text(new Point(0, 0), "Restrict", renderContext.DefaultFont, Color.Black);
                    Components.Add(TypeBoxLabel);
                }

                if(TypeBox == null)
                {
                    TypeBox = new ComboBox<TransferRestrictorType>(new List<ComboBoxItem<TransferRestrictorType>>
                    {
                        new ComboBoxItem<TransferRestrictorType>(renderContext.DefaultFont, "By Item Type", TransferRestrictorType.ByItemType),
                        new ComboBoxItem<TransferRestrictorType>(renderContext.DefaultFont, "By Total Quantity", TransferRestrictorType.ByTotalQuantity),
                        new ComboBoxItem<TransferRestrictorType>(renderContext.DefaultFont, "By Target Inventory", TransferRestrictorType.ByRecievingInventory),
                        new ComboBoxItem<TransferRestrictorType>(renderContext.DefaultFont, "By Our Inventory", TransferRestrictorType.ByOurInventory)
                    }, new Point(200, 30));

                    TypeBox.Selected = null;

                    TypeBox.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    TypeBox.ExpandedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    TypeBox.SelectedChanged += (sender, oldSelected) =>
                    {
                        if(oldSelected != null)
                        {
                            switch(oldSelected.Value)
                            {
                                case TransferRestrictorType.ByItemType:
                                    Components.Remove(ByItemType_Description);
                                    Components.Remove(ByItemType_AllowRadio);
                                    Components.Remove(ByItemType_AllowLabel);
                                    Components.Remove(ByItemType_DenyRadio);
                                    Components.Remove(ByItemType_DenyLabel);
                                    Components.Remove(ByItemType_MaterialLabel);
                                    Components.Remove(ByItemType_MaterialBox);
                                    break;
                                case TransferRestrictorType.ByTotalQuantity:
                                    Components.Remove(ByTotalQuantity_Description);
                                    Components.Remove(ByTotalQuantity_MaxLabel);
                                    Components.Remove(ByTotalQuantity_MaxField);
                                    break;
                                case TransferRestrictorType.ByRecievingInventory:
                                    Components.Remove(ByReceivingInventory_Description);
                                    Components.Remove(ByReceivingInventory_TypeCheck);
                                    Components.Remove(ByReceivingInventory_TypeCheckLabel);

                                    if(ByReceivingInventory_TypeCheck.Pushed)
                                    {
                                        Components.Remove(ByReceivingInventory_TypeBoxLabel);
                                        Components.Remove(ByReceivingInventory_TypeBox);
                                    }

                                    Components.Remove(ByReceivingInventory_FieldLabel);
                                    Components.Remove(ByReceivingInventory_Field);
                                    break;
                                case TransferRestrictorType.ByOurInventory:
                                    Components.Remove(ByOurInventory_Description);
                                    Components.Remove(ByOurInventory_TypeCheck);
                                    Components.Remove(ByOurInventory_TypeCheckLabel);

                                    if(ByOurInventory_TypeCheck.Pushed)
                                    {
                                        Components.Remove(ByOurInventory_TypeBoxLabel);
                                        Components.Remove(ByOurInventory_TypeBox);
                                    }

                                    Components.Remove(ByOurInventory_FieldLabel);
                                    Components.Remove(ByOurInventory_Field);
                                    break;
                            }
                        }

                        switch(TypeBox.Selected.Value)
                        {
                            case TransferRestrictorType.ByItemType:
                                Components.Add(ByItemType_Description);
                                Components.Add(ByItemType_AllowRadio);
                                Components.Add(ByItemType_AllowLabel);
                                Components.Add(ByItemType_DenyRadio);
                                Components.Add(ByItemType_DenyLabel);
                                Components.Add(ByItemType_MaterialLabel);
                                Components.Add(ByItemType_MaterialBox);
                                break;
                            case TransferRestrictorType.ByTotalQuantity:
                                Components.Add(ByTotalQuantity_Description);
                                Components.Add(ByTotalQuantity_MaxLabel);
                                Components.Add(ByTotalQuantity_MaxField);
                                break;
                            case TransferRestrictorType.ByRecievingInventory:
                                Components.Add(ByReceivingInventory_Description);
                                Components.Add(ByReceivingInventory_TypeCheck);
                                Components.Add(ByReceivingInventory_TypeCheckLabel);

                                if(ByReceivingInventory_TypeCheck.Pushed)
                                {
                                    Components.Add(ByReceivingInventory_TypeBoxLabel);
                                    Components.Add(ByReceivingInventory_TypeBox);
                                }

                                Components.Add(ByReceivingInventory_FieldLabel);
                                Components.Add(ByReceivingInventory_Field);
                                break;
                            case TransferRestrictorType.ByOurInventory:
                                Components.Add(ByOurInventory_Description);
                                Components.Add(ByOurInventory_TypeCheck);
                                Components.Add(ByOurInventory_TypeCheckLabel);

                                if(ByOurInventory_TypeCheck.Pushed)
                                {
                                    Components.Add(ByOurInventory_TypeBoxLabel);
                                    Components.Add(ByOurInventory_TypeBox);
                                }

                                Components.Add(ByOurInventory_FieldLabel);
                                Components.Add(ByOurInventory_Field);
                                break;
                        }
                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };

                    Components.Add(TypeBox);
                }

                if(ByItemType_Description == null)
                {
                    ByItemType_Description = new Text(new Point(0, 0), @"Restrict the transfer by either denying a
specific material or denying everything 
except a specific material in a transfer.

This is the most common restriction and is
useful whenever you have multiple item types
in your inventory or the targets inventory.", renderContext.DefaultFont, Color.White);
                }

                if(ByItemType_AllowLabel == null)
                {
                    ByItemType_AllowLabel = new Text(new Point(0, 0), "Deny Everything Else", renderContext.DefaultFont, Color.Black);
                }

                if(ByItemType_DenyLabel == null)
                {
                    ByItemType_DenyLabel = new Text(new Point(0, 0), "Deny Just This", renderContext.DefaultFont, Color.Black);
                }

                if(ByItemType_AllowRadio == null || ByItemType_DenyRadio == null)
                {
                    ByItemType_AllowRadio?.Dispose();
                    ByItemType_AllowRadio = null;
                    ByItemType_DenyRadio?.Dispose();
                    ByItemType_DenyRadio = null;

                    ByItemType_AllowRadio = new RadioButton(new Point(0, 0));
                    ByItemType_DenyRadio = new RadioButton(new Point(0, 0));

                    var group = new RadioButtonGroup(new[] { ByItemType_AllowRadio, ByItemType_DenyRadio });
                    group.Attach();

                    ByItemType_AllowRadio.PushedChanged += (sender, args) =>
                    {
                        Outer.OnInspectRedrawRequired();
                    };

                    ByItemType_DenyRadio.PushedChanged += (sender, args) =>
                    {
                        Outer.OnInspectRedrawRequired();
                    };
                }

                if(ByItemType_MaterialLabel == null)
                {
                    ByItemType_MaterialLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
                }

                if(ByItemType_MaterialBox == null)
                {
                    ByItemType_MaterialBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                    ByItemType_MaterialBox.ExpandedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByItemType_MaterialBox.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByItemType_MaterialBox.SelectedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                }

                if(ByTotalQuantity_Description == null)
                {
                    ByTotalQuantity_Description = new Text(new Point(0, 0), @"Restrict the transfer by not transferring 
more than a specific amount. 

This is useful if you either know exactly 
what items should be in your inventory at 
this point and want to utilize the order 
that items are given out, or you want to 
throttle a portion of your inputs to get 
an even split.", renderContext.DefaultFont, Color.White);
                }

                if(ByTotalQuantity_MaxLabel == null)
                {
                    ByTotalQuantity_MaxLabel = new Text(new Point(0, 0), "Max To Transfer:", renderContext.DefaultFont, Color.Black);
                }

                if(ByTotalQuantity_MaxField == null)
                {
                    ByTotalQuantity_MaxField = Outer.CreateTextField(75, 30);
                }

                if(ByReceivingInventory_Description == null)
                {
                    ByReceivingInventory_Description = new Text(new Point(0, 0), @"Restrict the transfer based on what 
would be in the other inventory 
after the transfer. Optionally
only look at a specific material.

This is useful if you have a task
that does a one-way conversion of 
items (ie. logs to chopped wood), 
but you still want to maintain a 
stockpile of the original item.", renderContext.DefaultFont, Color.White);
                }

                if(ByReceivingInventory_TypeCheckLabel == null)
                {
                    ByReceivingInventory_TypeCheckLabel = new Text(new Point(0, 0), "Search by type", renderContext.DefaultFont, Color.Black);
                }

                if(ByReceivingInventory_TypeCheck == null)
                {
                    ByReceivingInventory_TypeCheck = new CheckBox(new Point(0, 0));

                    ByReceivingInventory_TypeCheck.PushedChanged += (sender, args) =>
                    {
                        if (ByReceivingInventory_TypeCheck.Pushed)
                        {
                            Components.Add(ByReceivingInventory_TypeBoxLabel);
                            Components.Add(ByReceivingInventory_TypeBox);
                        }
                        else
                        {
                            Components.Remove(ByReceivingInventory_TypeBoxLabel);
                            Components.Remove(ByReceivingInventory_TypeBox);
                        }

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };
                }

                if(ByReceivingInventory_TypeBoxLabel == null)
                {
                    ByReceivingInventory_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
                }

                if(ByReceivingInventory_TypeBox == null)
                {
                    ByReceivingInventory_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                    ByReceivingInventory_TypeBox.ExpandedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByReceivingInventory_TypeBox.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByReceivingInventory_TypeBox.SelectedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                }

                if(ByReceivingInventory_FieldLabel == null)
                {
                    ByReceivingInventory_FieldLabel = new Text(new Point(0, 0), Outer.PickupRadio.Pushed ? "Minimum" : "Maximum", renderContext.DefaultFont, Color.Black);

                    EventHandler tmp = (sender, args) =>
                    {
                        ByReceivingInventory_FieldLabel.Content = Outer.PickupRadio.Pushed ? "Minimum" : "Maximum";

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };

                    Outer.PickupRadio.PushedChanged += tmp;
                    Outer.DropoffRadio.PushedChanged += tmp;

                    ByReceivingInventory_FieldLabel.Disposing += (sender, args) =>
                    {
                        if (Outer.PickupRadio != null)
                            Outer.PickupRadio.PushedChanged -= tmp;
                        if (Outer.DropoffRadio != null)
                            Outer.DropoffRadio.PushedChanged -= tmp;
                    };
                }

                if(ByReceivingInventory_Field == null)
                {
                    ByReceivingInventory_Field = Outer.CreateTextField(150, 30);
                }

                if(ByOurInventory_Description == null)
                {
                    ByOurInventory_Description = new Text(new Point(0, 0), @"Restrict the transfer based on what
would be in our inventory after the
transfer. Optionally only look at 
a specific material.

This is most useful when you are trying
to maintain a certain distribution of 
items in your inventory and your 
inventory does not always empty out
every cycle, such as for a courier.", renderContext.DefaultFont, Color.White);
                }

                if(ByOurInventory_TypeCheck == null)
                {
                    ByOurInventory_TypeCheck = new CheckBox(new Point(0, 0));

                    ByOurInventory_TypeCheck.PushedChanged += (sender, args) =>
                    {
                        if(ByOurInventory_TypeCheck.Pushed)
                        {
                            Components.Add(ByOurInventory_TypeBoxLabel);
                            Components.Add(ByOurInventory_TypeBox);
                        }else
                        {
                            Components.Remove(ByOurInventory_TypeBoxLabel);
                            Components.Remove(ByOurInventory_TypeBox);
                        }

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };
                }

                if(ByOurInventory_TypeCheckLabel == null)
                {
                    ByOurInventory_TypeCheckLabel = new Text(new Point(0, 0), "Search by type", renderContext.DefaultFont, Color.Black);
                }

                if (ByOurInventory_TypeBoxLabel == null)
                {
                    ByOurInventory_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
                }

                if (ByOurInventory_TypeBox == null)
                {
                    ByOurInventory_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                    ByOurInventory_TypeBox.ExpandedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByOurInventory_TypeBox.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByOurInventory_TypeBox.SelectedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                }

                if (ByOurInventory_FieldLabel == null)
                {
                    ByOurInventory_FieldLabel = new Text(new Point(0, 0), Outer.PickupRadio.Pushed ? "Maximum" : "Minimum", renderContext.DefaultFont, Color.Black);

                    EventHandler tmp = (sender, args) =>
                    {
                        ByOurInventory_FieldLabel.Content = Outer.PickupRadio.Pushed ? "Maximum" : "Minimum";

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };

                    Outer.PickupRadio.PushedChanged += tmp;
                    Outer.DropoffRadio.PushedChanged += tmp;

                    ByOurInventory_FieldLabel.Disposing += (sender, args) =>
                    {
                        if (Outer.PickupRadio != null)
                            Outer.PickupRadio.PushedChanged -= tmp;
                        if (Outer.DropoffRadio != null)
                            Outer.DropoffRadio.PushedChanged -= tmp;
                    };
                }

                if (ByOurInventory_Field == null)
                {
                    ByOurInventory_Field = Outer.CreateTextField(150, 30);
                }

                if(DeleteButton == null)
                {
                    DeleteButton = UIUtils.CreateButton(new Point(0, 0), "Delete Restrictor", UIUtils.ButtonColor.Yellow, UIUtils.ButtonSize.Medium);

                    DeleteButton.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    DeleteButton.PressedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    DeleteButton.PressReleased += (sender, args) =>
                    {
                        Outer.RestrictorsToDelete.Add(this);
                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };

                    Components.Add(DeleteButton);
                }
            }

            public void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
            {
                height += TypeBoxLabel.Size.Y + 3;
                TypeBox.Center = new Point(width / 2, height + TypeBox.Size.Y / 2);
                TypeBoxLabel.Center = Outer.GetCenterForTopLeftLabel(TypeBoxLabel, TypeBox);
                height += TypeBox.Size.Y + 5;

                if (TypeBox.Selected != null)
                {

                    int requiredWidth, requiredHeight;
                    int x, y;
                    switch (TypeBox.Selected.Value)
                    {
                        case TransferRestrictorType.ByItemType:
                            ByItemType_Description.Center = new Point(width / 2, height + ByItemType_Description.Size.Y / 2);
                            height += ByItemType_Description.Size.Y + 5;

                            requiredWidth = ByItemType_AllowRadio.Size.X + 3 + ByItemType_AllowLabel.Size.X + 7 + ByItemType_DenyRadio.Size.X + 3 + ByItemType_DenyLabel.Size.X;
                            requiredHeight = Math.Max(Math.Max(ByItemType_AllowRadio.Size.Y, ByItemType_DenyRadio.Size.Y), Math.Max(ByItemType_AllowLabel.Size.Y, ByItemType_DenyLabel.Size.Y));

                            x = (width - requiredWidth) / 2;
                            y = height + requiredHeight / 2;

                            ByItemType_AllowRadio.Center = new Point(x + ByItemType_AllowRadio.Size.X / 2, y);
                            x += ByItemType_AllowRadio.Size.X + 3;
                            ByItemType_AllowLabel.Center = new Point(x + ByItemType_AllowLabel.Size.X / 2, y);
                            x += ByItemType_AllowLabel.Size.X + 7;
                            ByItemType_DenyRadio.Center = new Point(x + ByItemType_DenyRadio.Size.X / 2, y);
                            x += ByItemType_DenyRadio.Size.X + 3;
                            ByItemType_DenyLabel.Center = new Point(x + ByItemType_DenyLabel.Size.X / 2, y);

                            height += requiredHeight + 5;

                            height += ByItemType_MaterialLabel.Size.Y + 3;
                            ByItemType_MaterialBox.Center = new Point(width / 2, height + ByItemType_MaterialBox.Size.Y / 2);
                            ByItemType_MaterialLabel.Center = Outer.GetCenterForTopLeftLabel(ByItemType_MaterialLabel, ByItemType_MaterialBox);
                            height += ByItemType_MaterialBox.Size.Y + 3;
                            break;
                        case TransferRestrictorType.ByTotalQuantity:
                            ByTotalQuantity_Description.Center = new Point(width / 2, height + ByTotalQuantity_Description.Size.Y / 2);
                            height += ByTotalQuantity_Description.Size.Y + 5;

                            requiredHeight = Math.Max(ByTotalQuantity_MaxLabel.Size.Y, ByTotalQuantity_MaxField.Size.Y);
                            requiredWidth = ByTotalQuantity_MaxLabel.Size.X + 3 + ByTotalQuantity_MaxField.Size.X;

                            x = (width - requiredWidth) / 2;
                            y = height + requiredHeight / 2;

                            ByTotalQuantity_MaxLabel.Center = new Point(x + ByTotalQuantity_MaxLabel.Size.X / 2, y);
                            x += ByTotalQuantity_MaxLabel.Size.X + 3;
                            ByTotalQuantity_MaxField.Center = new Point(x + ByTotalQuantity_MaxField.Size.X / 2, y);

                            height += requiredHeight + 3;
                            break;
                        case TransferRestrictorType.ByRecievingInventory:
                            ByReceivingInventory_Description.Center = new Point(width / 2, height + ByReceivingInventory_Description.Size.Y / 2);
                            height += ByReceivingInventory_Description.Size.Y + 5;

                            requiredHeight = Math.Max(ByReceivingInventory_TypeCheckLabel.Size.Y, ByReceivingInventory_TypeCheck.Size.Y);
                            requiredWidth = ByReceivingInventory_TypeCheck.Size.X + 3 + ByReceivingInventory_TypeCheckLabel.Size.X;

                            x = (width - requiredWidth) / 2;
                            y = height + requiredHeight / 2;

                            ByReceivingInventory_TypeCheck.Center = new Point(x + ByReceivingInventory_TypeCheck.Size.X / 2, y);
                            x += ByReceivingInventory_TypeCheck.Size.X + 3;
                            ByReceivingInventory_TypeCheckLabel.Center = new Point(x + ByReceivingInventory_TypeCheckLabel.Size.X / 2, y);

                            height += requiredHeight + 3;

                            if (ByReceivingInventory_TypeCheck.Pushed)
                            {
                                height += ByReceivingInventory_TypeBoxLabel.Size.Y + 3;
                                ByReceivingInventory_TypeBox.Center = new Point(width / 2, height + ByReceivingInventory_TypeBox.Size.Y / 2);
                                ByReceivingInventory_TypeBoxLabel.Center = Outer.GetCenterForTopLeftLabel(ByReceivingInventory_TypeBoxLabel, ByReceivingInventory_TypeBox);
                                height += ByReceivingInventory_TypeBox.Size.Y + 3;
                            }

                            height += ByReceivingInventory_FieldLabel.Size.Y + 3;
                            ByReceivingInventory_Field.Center = new Point(width / 2, height + ByReceivingInventory_Field.Size.Y / 2);
                            ByReceivingInventory_FieldLabel.Center = Outer.GetCenterForTopLeftLabel(ByReceivingInventory_FieldLabel, ByReceivingInventory_Field);
                            height += ByReceivingInventory_Field.Size.Y + 3;
                            break;
                        case TransferRestrictorType.ByOurInventory:
                            ByOurInventory_Description.Center = new Point(width / 2, height + ByOurInventory_Description.Size.Y / 2);
                            height += ByOurInventory_Description.Size.Y + 5;

                            requiredHeight = Math.Max(ByOurInventory_TypeCheckLabel.Size.Y, ByOurInventory_TypeCheck.Size.Y);
                            requiredWidth = ByOurInventory_TypeCheck.Size.X + 3 + ByOurInventory_TypeCheckLabel.Size.X;

                            x = (width - requiredWidth) / 2;
                            y = height + requiredHeight / 2;

                            ByOurInventory_TypeCheck.Center = new Point(x + ByOurInventory_TypeCheck.Size.X / 2, y);
                            x += ByOurInventory_TypeCheck.Size.X + 3;
                            ByOurInventory_TypeCheckLabel.Center = new Point(x + ByOurInventory_TypeCheckLabel.Size.X / 2, y);

                            height += requiredHeight + 3;

                            if (ByOurInventory_TypeCheck.Pushed)
                            {
                                height += ByOurInventory_TypeBoxLabel.Size.Y + 3;
                                ByOurInventory_TypeBox.Center = new Point(width / 2, height + ByOurInventory_TypeBox.Size.Y / 2);
                                ByOurInventory_TypeBoxLabel.Center = Outer.GetCenterForTopLeftLabel(ByOurInventory_TypeBoxLabel, ByOurInventory_TypeBox);
                                height += ByOurInventory_TypeBox.Size.Y + 3;
                            }

                            height += ByOurInventory_FieldLabel.Size.Y + 3;
                            ByOurInventory_Field.Center = new Point(width / 2, height + ByOurInventory_Field.Size.Y / 2);
                            ByOurInventory_FieldLabel.Center = Outer.GetCenterForTopLeftLabel(ByOurInventory_FieldLabel, ByOurInventory_Field);
                            height += ByOurInventory_Field.Size.Y + 3;
                            break;
                    }
                }

                DeleteButton.Center = new Point(width / 2, height + DeleteButton.Size.Y / 2);
                height += DeleteButton.Size.Y + 3;

                height += 5;
            }

            public void Dispose()
            {
                TypeBoxLabel?.Dispose();
                TypeBoxLabel = null;

                TypeBox?.Dispose();
                TypeBox = null;

                ByItemType_Description?.Dispose();
                ByItemType_Description = null;

                ByItemType_AllowLabel?.Dispose();
                ByItemType_AllowLabel = null;

                ByItemType_AllowRadio?.Dispose();
                ByItemType_AllowRadio = null;

                ByItemType_DenyLabel?.Dispose();
                ByItemType_DenyLabel = null;

                ByItemType_DenyRadio?.Dispose();
                ByItemType_DenyRadio = null;

                ByItemType_MaterialLabel?.Dispose();
                ByItemType_MaterialLabel = null;

                ByItemType_MaterialBox?.Dispose();
                ByItemType_MaterialBox = null;

                ByTotalQuantity_Description?.Dispose();
                ByTotalQuantity_Description = null;

                ByTotalQuantity_MaxLabel?.Dispose();
                ByTotalQuantity_MaxLabel = null;

                ByTotalQuantity_MaxField?.Dispose();
                ByTotalQuantity_MaxField = null;

                ByReceivingInventory_Description?.Dispose();
                ByReceivingInventory_Description = null;

                ByReceivingInventory_TypeCheckLabel?.Dispose();
                ByReceivingInventory_TypeCheckLabel = null;

                ByReceivingInventory_TypeCheck?.Dispose();
                ByReceivingInventory_TypeCheck = null;

                ByReceivingInventory_TypeBoxLabel?.Dispose();
                ByReceivingInventory_TypeBoxLabel = null;

                ByReceivingInventory_TypeBox?.Dispose();
                ByReceivingInventory_TypeBox = null;

                ByReceivingInventory_FieldLabel?.Dispose();
                ByReceivingInventory_FieldLabel = null;

                ByReceivingInventory_Field?.Dispose();
                ByReceivingInventory_Field = null;

                ByOurInventory_Description?.Dispose();
                ByOurInventory_Description = null;

                ByOurInventory_TypeCheckLabel?.Dispose();
                ByOurInventory_TypeCheckLabel = null;

                ByOurInventory_TypeCheck?.Dispose();
                ByOurInventory_TypeCheck = null;

                ByOurInventory_TypeBoxLabel?.Dispose();
                ByOurInventory_TypeBoxLabel = null;

                ByOurInventory_TypeBox?.Dispose();
                ByOurInventory_TypeBox = null;

                ByOurInventory_FieldLabel?.Dispose();
                ByOurInventory_FieldLabel = null;

                ByOurInventory_Field?.Dispose();
                ByOurInventory_Field = null;
                Components.Clear();
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
        protected List<TransferRestrictorComp> RestrictorsToDelete;
        protected Button AddRestrictorButton;

        /// <summary>
        /// Contains all of the components  of this task item (an alternative
        /// for going through each thing above one by one)
        /// </summary>
        protected List<IScreenComponent> Components;
        
        protected IEnumerable<IScreenComponent> ComponentsInRenderOrder
        {
            get
            {
                foreach(var comp in Components)
                {
                    if (!comp.HighPriorityZ)
                        yield return comp;
                }

                foreach(var restr in Restrictors)
                {
                    foreach(var comp in restr.Components)
                    {
                        if (!comp.HighPriorityZ)
                            yield return comp;
                    }
                }

                foreach(var comp in Components)
                {
                    if (comp.HighPriorityZ)
                        yield return comp;
                }

                foreach(var restr in Restrictors)
                {
                    foreach(var comp in restr.Components)
                    {
                        if (comp.HighPriorityZ)
                            yield return comp;
                    }
                }
            }
        }

        protected IEnumerable<IScreenComponent> ComponentsInUpdateOrder
        {
            get
            {
                for (int i = Restrictors.Count - 1; i >= 0; i--)
                {
                    for (int j = Restrictors[i].Components.Count - 1; j >= 0; j--)
                    {
                        if (Restrictors[i].Components[j].HighPriorityZ)
                            yield return Restrictors[i].Components[j];
                    }
                }

                for(int i = Components.Count - 1; i >= 0; i--)
                {
                    if (Components[i].HighPriorityZ)
                        yield return Components[i];
                }

                for (int i = Restrictors.Count - 1; i >= 0; i--)
                {
                    for (int j = Restrictors[i].Components.Count - 1; j >= 0; j--)
                    {
                        if (!Restrictors[i].Components[j].HighPriorityZ)
                            yield return Restrictors[i].Components[j];
                    }
                }

                for (int i = Components.Count - 1; i >= 0; i--)
                {
                    if (!Components[i].HighPriorityZ)
                        yield return Components[i];
                }
            }
        }

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
            Restrictors = new List<TransferRestrictorComp>();
            RestrictorsToDelete = new List<TransferRestrictorComp>();

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
                    
                    if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByID)
                    {
                        Components.Add(TargetDeciderByID_DescriptionText);
                        Components.Add(TargetDeciderByID_IDLabel);
                        Components.Add(TargetDeciderByID_IDTextField);
                    }
                    else if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByPosition)
                    {
                        Components.Add(TargetDeciderByPosition_DescriptionText);
                        Components.Add(TargetDeciderByPosition_XField);
                        Components.Add(TargetDeciderByPosition_XLabel);
                        Components.Add(TargetDeciderByPosition_YField);
                        Components.Add(TargetDeciderByPosition_YLabel);
                    }
                    else if (TargetDeciderCombo.Selected.Value == TargetDeciderType.ByRelativePosition)
                    {
                        Components.Add(TargetDeciderByRelPosition_DescriptionText);
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

            if(AddRestrictorButton == null)
            {
                AddRestrictorButton = UIUtils.CreateButton(new Point(0, 0), "Add Restriction", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
                AddRestrictorButton.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                AddRestrictorButton.PressedChanged += (sender, args) => OnInspectRedrawRequired();
                AddRestrictorButton.PressReleased += (sender, args) =>
                {
                    var restrictor = new TransferRestrictorComp(this);
                    restrictor.InitializeThings(renderContext);
                    Restrictors.Add(restrictor);

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                Components.Add(AddRestrictorButton);
            }

            foreach(var restrictor in Restrictors)
            {
                restrictor.InitializeThings(renderContext);
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
            result.CaretToggled += (sender, args) => OnInspectRedrawRequired();
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
            height += 5;

            foreach(var restrictor in Restrictors)
            {
                restrictor.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
            }

            AddRestrictorButton.Center = new Point(width / 2, height + AddRestrictorButton.Size.Y / 2);

            height += AddRestrictorButton.Size.Y + 3;

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
            if (Children.Count != 0)
                return false;

            if (PickupRadio == null)
                return Task.IsValid();

            if(PickupRadio.Pushed)
            {
                if (PickupTypeCombo.Selected == null)
                    return false;
            }else if(DropoffRadio.Pushed)
            {
                if (DropoffTypeCombo.Selected == null)
                    return false;
            }else
            {
                return false;
            }

            if (TargetDeciderCombo.Selected == null)
                return false;


            return true;
        }

        public override void PreDrawInspect(RenderContext context, int x, int y)
        {
            base.PreDrawInspect(context, x, y);

            foreach (var comp in ComponentsInRenderOrder)
            {
                comp.PreDraw(context.Content, context.Graphics, context.GraphicsDevice);
            }
        }

        public override void DrawInspect(RenderContext context, int x, int y)
        {
            if (ButtonShiftLast.X != x || ButtonShiftLast.Y != y)
            {
                foreach (var comp in ComponentsInRenderOrder)
                {
                    comp.Center = new Point(comp.Center.X - ButtonShiftLast.X + x, comp.Center.Y - ButtonShiftLast.Y + y);
                }
            }

            base.DrawInspect(context, x, y);

            foreach (var comp in ComponentsInRenderOrder)
            {
                comp.Draw(context.Content, context.Graphics, context.GraphicsDevice, context.SpriteBatch);
            }

        }

        public override void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.UpdateInspect(sharedGameState, localGameState, netContext, timeMS);
            
            foreach(var restr in RestrictorsToDelete)
            {
                Restrictors.Remove(restr);
                restr.Dispose();
            }
            RestrictorsToDelete.Clear();

            foreach (var comp in ComponentsInUpdateOrder)
            {
                comp.Update(Content, timeMS);
            }
        }

        protected override void HandleInspectComponentsMouseState(MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            foreach(var comp in ComponentsInUpdateOrder)
            {
                comp.HandleMouseState(Content, last, current, ref handled, ref scrollHandled);
            }
            
            base.HandleInspectComponentsMouseState(last, current, ref handled, ref scrollHandled);
        }

        public override bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            var handled = false;

            foreach (var comp in ComponentsInUpdateOrder)
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

            foreach(var restrictor in Restrictors)
            {
                restrictor.Dispose();
            }

            AddRestrictorButton?.Dispose();
            AddRestrictorButton = null;

            Restrictors.Clear();
            Components.Clear();
        }
    }
}