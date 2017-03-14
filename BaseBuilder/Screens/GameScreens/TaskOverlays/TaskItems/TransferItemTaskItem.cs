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
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;

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
            ByTargetInventory,
            ByOurInventory
        }

        protected enum ResultDeciderType
        {
            OurItems,
            TheirItems,
            ItemsTransferred
        }

        protected class TransferRestrictorComp
        {
            protected TransferItemTaskItem Outer;
            protected ITransferRestrictor Original;
            
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

            public Text ByTargetInventory_Description; 
            public Text ByTargetInventory_TypeCheckLabel;
            public CheckBox ByTargetInventory_TypeCheck;
            public Text ByTargetInventory_TypeBoxLabel;
            public ComboBox<Material> ByTargetInventory_TypeBox;
            public Text ByTargetInventory_FieldLabel;
            public TextField ByTargetInventory_Field;

            public Text ByOurInventory_Description;
            public Text ByOurInventory_TypeCheckLabel;
            public CheckBox ByOurInventory_TypeCheck;
            public Text ByOurInventory_TypeBoxLabel;
            public ComboBox<Material> ByOurInventory_TypeBox;
            public Text ByOurInventory_FieldLabel;
            public TextField ByOurInventory_Field;

            public Button DeleteButton;

            public List<IScreenComponent> Components;

            public bool LoadedFromOriginal;

            public TransferRestrictorComp(TransferItemTaskItem outer, ITransferRestrictor restrictor)
            {
                Outer = outer;
                Original = restrictor;

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
                        new ComboBoxItem<TransferRestrictorType>(renderContext.DefaultFont, "By Target Inventory", TransferRestrictorType.ByTargetInventory),
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
                                case TransferRestrictorType.ByTargetInventory:
                                    Components.Remove(ByTargetInventory_Description);
                                    Components.Remove(ByTargetInventory_TypeCheck);
                                    Components.Remove(ByTargetInventory_TypeCheckLabel);

                                    if(ByTargetInventory_TypeCheck.Pushed)
                                    {
                                        Components.Remove(ByTargetInventory_TypeBoxLabel);
                                        Components.Remove(ByTargetInventory_TypeBox);
                                    }

                                    Components.Remove(ByTargetInventory_FieldLabel);
                                    Components.Remove(ByTargetInventory_Field);
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
                            case TransferRestrictorType.ByTargetInventory:
                                Components.Add(ByTargetInventory_Description);
                                Components.Add(ByTargetInventory_TypeCheck);
                                Components.Add(ByTargetInventory_TypeCheckLabel);

                                if(ByTargetInventory_TypeCheck.Pushed)
                                {
                                    Components.Add(ByTargetInventory_TypeBoxLabel);
                                    Components.Add(ByTargetInventory_TypeBox);
                                }

                                Components.Add(ByTargetInventory_FieldLabel);
                                Components.Add(ByTargetInventory_Field);
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

                if(ByTargetInventory_Description == null)
                {
                    ByTargetInventory_Description = new Text(new Point(0, 0), @"Restrict the transfer based on what 
would be in the other inventory 
after the transfer. Optionally
only look at a specific material.

This is useful if you have a task
that does a one-way conversion of 
items (ie. logs to chopped wood), 
but you still want to maintain a 
stockpile of the original item.", renderContext.DefaultFont, Color.White);
                }

                if(ByTargetInventory_TypeCheckLabel == null)
                {
                    ByTargetInventory_TypeCheckLabel = new Text(new Point(0, 0), "Search by type", renderContext.DefaultFont, Color.Black);
                }

                if(ByTargetInventory_TypeCheck == null)
                {
                    ByTargetInventory_TypeCheck = new CheckBox(new Point(0, 0));

                    ByTargetInventory_TypeCheck.PushedChanged += (sender, args) =>
                    {
                        if (ByTargetInventory_TypeCheck.Pushed)
                        {
                            Components.Add(ByTargetInventory_TypeBoxLabel);
                            Components.Add(ByTargetInventory_TypeBox);
                        }
                        else
                        {
                            Components.Remove(ByTargetInventory_TypeBoxLabel);
                            Components.Remove(ByTargetInventory_TypeBox);
                        }

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };
                }

                if(ByTargetInventory_TypeBoxLabel == null)
                {
                    ByTargetInventory_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
                }

                if(ByTargetInventory_TypeBox == null)
                {
                    ByTargetInventory_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                    ByTargetInventory_TypeBox.ExpandedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByTargetInventory_TypeBox.HoveredChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                    ByTargetInventory_TypeBox.SelectedChanged += (sender, args) => Outer.OnInspectRedrawRequired();
                }

                if(ByTargetInventory_FieldLabel == null)
                {
                    ByTargetInventory_FieldLabel = new Text(new Point(0, 0), Outer.PickupRadio.Pushed ? "Minimum" : "Maximum", renderContext.DefaultFont, Color.Black);

                    EventHandler tmp = (sender, args) =>
                    {
                        ByTargetInventory_FieldLabel.Content = Outer.PickupRadio.Pushed ? "Minimum" : "Maximum";

                        Outer.Reload = true;
                        Outer.OnInspectRedrawRequired();
                    };

                    Outer.PickupRadio.PushedChanged += tmp;
                    Outer.DropoffRadio.PushedChanged += tmp;

                    ByTargetInventory_FieldLabel.Disposing += (sender, args) =>
                    {
                        if (Outer.PickupRadio != null)
                            Outer.PickupRadio.PushedChanged -= tmp;
                        if (Outer.DropoffRadio != null)
                            Outer.DropoffRadio.PushedChanged -= tmp;
                    };
                }

                if(ByTargetInventory_Field == null)
                {
                    ByTargetInventory_Field = Outer.CreateTextField(150, 30);
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

                if(!LoadedFromOriginal)
                {
                    LoadFromOriginal();
                    LoadedFromOriginal = true;
                }
            }
            
            void LoadFromOriginal()
            {
                if (Original == null)
                    return;

                if (Original.GetType() == typeof(InventoryRestriction))
                {
                    var tmp = (InventoryRestriction)Original;

                    if (Outer.PickupRadio.Pushed == tmp.CheckRecievingInventory)
                    {
                        // talking about them
                        TypeBox.Selected = TypeBox.GetComboItemByValue(TransferRestrictorType.ByTargetInventory);
                        if (tmp.KeyMaterial != null)
                        {
                            ByTargetInventory_TypeCheck.Pushed = true;
                            ByTargetInventory_TypeBox.Selected = ByTargetInventory_TypeBox.GetComboItemByValue(tmp.KeyMaterial);
                        }
                        ByTargetInventory_Field.Text = tmp.KeyQuantity.ToString();
                    }
                    else
                    {
                        TypeBox.Selected = TypeBox.GetComboItemByValue(TransferRestrictorType.ByOurInventory);
                        if (tmp.KeyMaterial != null)
                        {
                            ByOurInventory_TypeCheck.Pushed = true;
                            ByOurInventory_TypeBox.Selected = ByOurInventory_TypeBox.GetComboItemByValue(tmp.KeyMaterial);
                        }
                        ByOurInventory_Field.Text = tmp.KeyQuantity.ToString();
                    }
                }
                else if (Original.GetType() == typeof(MaterialRestriction))
                {
                    var tmp = (MaterialRestriction)Original;

                    TypeBox.Selected = TypeBox.GetComboItemByValue(TransferRestrictorType.ByItemType);

                    if (tmp.AllExcept)
                    {
                        ByItemType_AllowRadio.Pushed = true;
                    }
                    else
                    {
                        ByItemType_DenyRadio.Pushed = true;
                    }

                    if (tmp.KeyMaterial != null)
                    {
                        ByItemType_MaterialBox.Selected = ByItemType_MaterialBox.GetComboItemByValue(tmp.KeyMaterial);
                    }
                }else if(Original.GetType() == typeof(QuantityRestriction))
                {
                    var tmp = (QuantityRestriction)Original;

                    TypeBox.Selected = TypeBox.GetComboItemByValue(TransferRestrictorType.ByTotalQuantity);

                    ByTotalQuantity_MaxField.Text = tmp.KeyQuantity.ToString();
                }else
                {
                    throw new InvalidProgramException();
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
                        case TransferRestrictorType.ByTargetInventory:
                            ByTargetInventory_Description.Center = new Point(width / 2, height + ByTargetInventory_Description.Size.Y / 2);
                            height += ByTargetInventory_Description.Size.Y + 5;

                            requiredHeight = Math.Max(ByTargetInventory_TypeCheckLabel.Size.Y, ByTargetInventory_TypeCheck.Size.Y);
                            requiredWidth = ByTargetInventory_TypeCheck.Size.X + 3 + ByTargetInventory_TypeCheckLabel.Size.X;

                            x = (width - requiredWidth) / 2;
                            y = height + requiredHeight / 2;

                            ByTargetInventory_TypeCheck.Center = new Point(x + ByTargetInventory_TypeCheck.Size.X / 2, y);
                            x += ByTargetInventory_TypeCheck.Size.X + 3;
                            ByTargetInventory_TypeCheckLabel.Center = new Point(x + ByTargetInventory_TypeCheckLabel.Size.X / 2, y);

                            height += requiredHeight + 3;

                            if (ByTargetInventory_TypeCheck.Pushed)
                            {
                                height += ByTargetInventory_TypeBoxLabel.Size.Y + 3;
                                ByTargetInventory_TypeBox.Center = new Point(width / 2, height + ByTargetInventory_TypeBox.Size.Y / 2);
                                ByTargetInventory_TypeBoxLabel.Center = Outer.GetCenterForTopLeftLabel(ByTargetInventory_TypeBoxLabel, ByTargetInventory_TypeBox);
                                height += ByTargetInventory_TypeBox.Size.Y + 3;
                            }

                            height += ByTargetInventory_FieldLabel.Size.Y + 3;
                            ByTargetInventory_Field.Center = new Point(width / 2, height + ByTargetInventory_Field.Size.Y / 2);
                            ByTargetInventory_FieldLabel.Center = Outer.GetCenterForTopLeftLabel(ByTargetInventory_FieldLabel, ByTargetInventory_Field);
                            height += ByTargetInventory_Field.Size.Y + 3;
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

                ByTargetInventory_Description?.Dispose();
                ByTargetInventory_Description = null;

                ByTargetInventory_TypeCheckLabel?.Dispose();
                ByTargetInventory_TypeCheckLabel = null;

                ByTargetInventory_TypeCheck?.Dispose();
                ByTargetInventory_TypeCheck = null;

                ByTargetInventory_TypeBoxLabel?.Dispose();
                ByTargetInventory_TypeBoxLabel = null;

                ByTargetInventory_TypeBox?.Dispose();
                ByTargetInventory_TypeBox = null;

                ByTargetInventory_FieldLabel?.Dispose();
                ByTargetInventory_FieldLabel = null;

                ByTargetInventory_Field?.Dispose();
                ByTargetInventory_Field = null;

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

            public bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
            {
                if (TypeBoxLabel == null)
                    return Original != null && Original.IsValid();

                if (TypeBox.Selected == null)
                    return false;

                int parsed;
                switch(TypeBox.Selected.Value)
                {
                    case TransferRestrictorType.ByItemType:
                        if (!ByItemType_AllowRadio.Pushed && !ByItemType_DenyRadio.Pushed)
                            return false;
                        if (ByItemType_MaterialBox.Selected == null)
                            return false;
                        break;
                    case TransferRestrictorType.ByTotalQuantity:
                        if (ByTotalQuantity_MaxField.Text.Length == 0)
                            return false;
                        
                        if (!int.TryParse(ByTotalQuantity_MaxField.Text, out parsed))
                            return false;

                        if (parsed <= 0)
                            return false;
                        break;
                    case TransferRestrictorType.ByTargetInventory:
                        if (!Outer.PickupRadio.Pushed && !Outer.DropoffRadio.Pushed)
                            return false;

                        if (ByTargetInventory_TypeCheck.Pushed && ByTargetInventory_TypeBox.Selected == null)
                            return false;

                        if (ByTargetInventory_Field.Text.Length == 0)
                            return false;

                        if (!int.TryParse(ByTargetInventory_Field.Text, out parsed))
                            return false;

                        if (parsed <= 0)
                            return false;
                        break;
                    case TransferRestrictorType.ByOurInventory:
                        if (!Outer.PickupRadio.Pushed && !Outer.DropoffRadio.Pushed)
                            return false;

                        if (ByOurInventory_TypeCheck.Pushed && ByOurInventory_TypeBox.Selected == null)
                            return false;

                        if (ByOurInventory_Field.Text.Length == 0)
                            return false;

                        if (!int.TryParse(ByOurInventory_Field.Text, out parsed))
                            return false;

                        if (parsed <= 0)
                            return false;
                        break;
                }
                return true;
            }
            
            public ITransferRestrictor CreateRestrictor(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
            {
                if (TypeBox.Selected == null)
                    return null;

                int quantity;
                Material material;
                bool pickup, allow;
                switch(TypeBox.Selected.Value)
                {
                    case TransferRestrictorType.ByItemType:
                        allow = ByItemType_AllowRadio.Pushed;
                        material = ByItemType_MaterialBox.Selected == null ? null : ByItemType_MaterialBox.Selected.Value;

                        return new MaterialRestriction(material, allow);
                    case TransferRestrictorType.ByTotalQuantity:
                        if (!int.TryParse(ByTotalQuantity_MaxField.Text, out quantity))
                            quantity = 1;

                        return new QuantityRestriction(null, quantity);
                    case TransferRestrictorType.ByTargetInventory:
                    case TransferRestrictorType.ByOurInventory:
                        if (!int.TryParse(ByTargetInventory_Field.Text, out quantity))
                            quantity = 1;

                        pickup = Outer.PickupRadio.Pushed;
                        material = (ByTargetInventory_TypeCheck.Pushed && ByTargetInventory_TypeBox.Selected != null) ? ByTargetInventory_TypeBox.Selected.Value : null;

                        return new InventoryRestriction(TypeBox.Selected.Value == TransferRestrictorType.ByTargetInventory ? !pickup : pickup, quantity, material);
                }

                return null;
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

        protected Text ResultDeciderComboLabel;
        protected ComboBox<ResultDeciderType> ResultDeciderCombo;

        protected Text ResultDeciderOurItems_Description;
        protected Text ResultDeciderOurItems_TypeCheckLabel;
        protected CheckBox ResultDeciderOurItems_TypeCheck;
        protected Text ResultDeciderOurItems_TypeBoxLabel;
        protected ComboBox<Material> ResultDeciderOurItems_TypeBox;
        protected Text ResultDeciderOurItems_RepeatFailGroupLabel;
        protected Text ResultDeciderOurItems_RepeatRadioLabel;
        protected RadioButton ResultDeciderOurItems_RepeatRadio;
        protected Text ResultDeciderOurItems_FailRadioLabel;
        protected RadioButton ResultDeciderOurItems_FailRadio;
        protected Text ResultDeciderOurItems_FieldLabel;
        protected TextField ResultDeciderOurItems_Field;

        protected Text ResultDeciderTheirItems_Description;
        protected Text ResultDeciderTheirItems_TypeCheckLabel;
        protected CheckBox ResultDeciderTheirItems_TypeCheck;
        protected Text ResultDeciderTheirItems_TypeBoxLabel;
        protected ComboBox<Material> ResultDeciderTheirItems_TypeBox;
        protected Text ResultDeciderTheirItems_RepeatFailGroupLabel;
        protected Text ResultDeciderTheirItems_RepeatRadioLabel;
        protected RadioButton ResultDeciderTheirItems_RepeatRadio;
        protected Text ResultDeciderTheirItems_FailRadioLabel;
        protected RadioButton ResultDeciderTheirItems_FailRadio;
        protected Text ResultDeciderTheirItems_FieldLabel;
        protected TextField ResultDeciderTheirItems_Field;

        protected Text ResultDeciderItemsTransfered_Description;
        protected Text ResultDeciderItemsTransfered_TypeCheckLabel;
        protected CheckBox ResultDeciderItemsTransfered_TypeCheck;
        protected Text ResultDeciderItemsTransfered_TypeBoxLabel;
        protected ComboBox<Material> ResultDeciderItemsTransfered_TypeBox;
        protected Text ResultDeciderItemsTransfered_RepeatFailGroupLabel;
        protected Text ResultDeciderItemsTransfered_RepeatRadioLabel;
        protected RadioButton ResultDeciderItemsTransfered_RepeatRadio;
        protected Text ResultDeciderItemsTransfered_FailRadioLabel;
        protected RadioButton ResultDeciderItemsTransfered_FailRadio;
        protected Text ResultDeciderItemsTransfered_FieldLabel;
        protected TextField ResultDeciderItemsTransfered_Field;

        protected bool LoadedFromTask;

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

            LoadedFromTask = false;
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
            Savable = true;
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

                PickupRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();

                DropoffRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();

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

            if (TargetDeciderByPosition_DescriptionText == null)
            {
                TargetDeciderByPosition_DescriptionText = new Text(new Point(0, 0), @"Select the target by searching for the entity 
at the specified position. This is helpful if
you want to be able to swap out the target 
without modifying this entity.", renderContext.DefaultFont, Color.White);
            }

            if (TargetDeciderByPosition_XField == null)
            {
                TargetDeciderByPosition_XField = CreateTextField(70, 30);
            }

            if (TargetDeciderByPosition_XLabel == null)
            {
                TargetDeciderByPosition_XLabel = new Text(new Point(0, 0), "X", renderContext.DefaultFont, Color.Black);
            }

            if (TargetDeciderByPosition_YField == null)
            {
                TargetDeciderByPosition_YField = CreateTextField(70, 30);
            }

            if (TargetDeciderByPosition_YLabel == null)
            {
                TargetDeciderByPosition_YLabel = new Text(new Point(0, 0), "Y", renderContext.DefaultFont, Color.Black);
            }

            if (TargetDeciderByRelPosition_DescriptionText == null)
            {
                TargetDeciderByRelPosition_DescriptionText = new Text(new Point(0, 0), @"Select the target by searching for an entity 
at a position relative to this entity at the 
time that this task is evaluated. This is 
often helpful for complicated and/or flexible 
behavior trees.", renderContext.DefaultFont, Color.White);
            }

            if (TargetDeciderByRelPosition_DXField == null)
            {
                TargetDeciderByRelPosition_DXField = CreateTextField(70, 30, negatives: true);
            }

            if (TargetDeciderByRelPosition_DXLabel == null)
            {
                TargetDeciderByRelPosition_DXLabel = new Text(new Point(0, 0), "DeltaX", renderContext.DefaultFont, Color.Black);
            }

            if (TargetDeciderByRelPosition_DYField == null)
            {
                TargetDeciderByRelPosition_DYField = CreateTextField(70, 30, negatives: true);
            }

            if (TargetDeciderByRelPosition_DYLabel == null)
            {
                TargetDeciderByRelPosition_DYLabel = new Text(new Point(0, 0), "DeltaY", renderContext.DefaultFont, Color.Black);
            }

            if (AddRestrictorButton == null)
            {
                AddRestrictorButton = UIUtils.CreateButton(new Point(0, 0), "Add Restriction", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
                AddRestrictorButton.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                AddRestrictorButton.PressedChanged += (sender, args) => OnInspectRedrawRequired();
                AddRestrictorButton.PressReleased += (sender, args) =>
                {
                    var restrictor = new TransferRestrictorComp(this, null);
                    restrictor.InitializeThings(renderContext);
                    Restrictors.Add(restrictor);

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                Components.Add(AddRestrictorButton);
            }

            foreach (var restrictor in Restrictors)
            {
                restrictor.InitializeThings(renderContext);
            }

            if (ResultDeciderComboLabel == null)
            {
                ResultDeciderComboLabel = new Text(new Point(0, 0), "Result Decider", renderContext.DefaultFont, Color.Black);
                Components.Add(ResultDeciderComboLabel);
            }

            if (ResultDeciderCombo == null)
            {
                ResultDeciderCombo = new ComboBox<ResultDeciderType>(new List<ComboBoxItem<ResultDeciderType>> {
                    new ComboBoxItem<ResultDeciderType>(renderContext.DefaultFont, "Our Items", ResultDeciderType.OurItems),
                    new ComboBoxItem<ResultDeciderType>(renderContext.DefaultFont, "Targets Items", ResultDeciderType.TheirItems),
                    new ComboBoxItem<ResultDeciderType>(renderContext.DefaultFont, "Items Transferred", ResultDeciderType.ItemsTransferred),
                }, new Point(200, 30));

                ResultDeciderCombo.Selected = null;

                ResultDeciderCombo.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderCombo.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderCombo.SelectedChanged += (sender, oldSelected) =>
                {
                    if (oldSelected != null)
                    {
                        switch (oldSelected.Value)
                        {
                            case ResultDeciderType.OurItems:
                                Components.Remove(ResultDeciderOurItems_Description);
                                Components.Remove(ResultDeciderOurItems_TypeCheck);
                                Components.Remove(ResultDeciderOurItems_TypeCheckLabel);

                                if (ResultDeciderOurItems_TypeCheck.Pushed)
                                {
                                    Components.Remove(ResultDeciderOurItems_TypeBoxLabel);
                                    Components.Remove(ResultDeciderOurItems_TypeBox);
                                }

                                Components.Remove(ResultDeciderOurItems_RepeatFailGroupLabel);
                                Components.Remove(ResultDeciderOurItems_RepeatRadio);
                                Components.Remove(ResultDeciderOurItems_RepeatRadioLabel);
                                Components.Remove(ResultDeciderOurItems_FailRadio);
                                Components.Remove(ResultDeciderOurItems_FailRadioLabel);
                                Components.Remove(ResultDeciderOurItems_Field);
                                Components.Remove(ResultDeciderOurItems_FieldLabel);
                                break;
                            case ResultDeciderType.TheirItems:
                                Components.Remove(ResultDeciderTheirItems_Description);
                                Components.Remove(ResultDeciderTheirItems_TypeCheck);
                                Components.Remove(ResultDeciderTheirItems_TypeCheckLabel);

                                if (ResultDeciderTheirItems_TypeCheck.Pushed)
                                {
                                    Components.Remove(ResultDeciderTheirItems_TypeBoxLabel);
                                    Components.Remove(ResultDeciderTheirItems_TypeBox);
                                }

                                Components.Remove(ResultDeciderTheirItems_RepeatFailGroupLabel);
                                Components.Remove(ResultDeciderTheirItems_RepeatRadio);
                                Components.Remove(ResultDeciderTheirItems_RepeatRadioLabel);
                                Components.Remove(ResultDeciderTheirItems_FailRadio);
                                Components.Remove(ResultDeciderTheirItems_FailRadioLabel);
                                Components.Remove(ResultDeciderTheirItems_Field);
                                Components.Remove(ResultDeciderTheirItems_FieldLabel);
                                break;
                            case ResultDeciderType.ItemsTransferred:
                                Components.Remove(ResultDeciderItemsTransfered_Description);
                                Components.Remove(ResultDeciderItemsTransfered_TypeCheck);
                                Components.Remove(ResultDeciderItemsTransfered_TypeCheckLabel);

                                if (ResultDeciderItemsTransfered_TypeCheck.Pushed)
                                {
                                    Components.Remove(ResultDeciderItemsTransfered_TypeBoxLabel);
                                    Components.Remove(ResultDeciderItemsTransfered_TypeBox);
                                }

                                Components.Remove(ResultDeciderItemsTransfered_RepeatFailGroupLabel);
                                Components.Remove(ResultDeciderItemsTransfered_RepeatRadio);
                                Components.Remove(ResultDeciderItemsTransfered_RepeatRadioLabel);
                                Components.Remove(ResultDeciderItemsTransfered_FailRadio);
                                Components.Remove(ResultDeciderItemsTransfered_FailRadioLabel);
                                Components.Remove(ResultDeciderItemsTransfered_Field);
                                Components.Remove(ResultDeciderItemsTransfered_FieldLabel);
                                break;
                        }
                    }

                    switch (ResultDeciderCombo.Selected.Value)
                    {
                        case ResultDeciderType.OurItems:
                            Components.Add(ResultDeciderOurItems_Description);
                            Components.Add(ResultDeciderOurItems_TypeCheck);
                            Components.Add(ResultDeciderOurItems_TypeCheckLabel);

                            if (ResultDeciderOurItems_TypeCheck.Pushed)
                            {
                                Components.Add(ResultDeciderOurItems_TypeBoxLabel);
                                Components.Add(ResultDeciderOurItems_TypeBox);
                            }

                            Components.Add(ResultDeciderOurItems_RepeatFailGroupLabel);
                            Components.Add(ResultDeciderOurItems_RepeatRadio);
                            Components.Add(ResultDeciderOurItems_RepeatRadioLabel);
                            Components.Add(ResultDeciderOurItems_FailRadio);
                            Components.Add(ResultDeciderOurItems_FailRadioLabel);
                            Components.Add(ResultDeciderOurItems_Field);
                            Components.Add(ResultDeciderOurItems_FieldLabel);
                            break;
                        case ResultDeciderType.TheirItems:
                            Components.Add(ResultDeciderTheirItems_Description);
                            Components.Add(ResultDeciderTheirItems_TypeCheck);
                            Components.Add(ResultDeciderTheirItems_TypeCheckLabel);

                            if (ResultDeciderTheirItems_TypeCheck.Pushed)
                            {
                                Components.Add(ResultDeciderTheirItems_TypeBoxLabel);
                                Components.Add(ResultDeciderTheirItems_TypeBox);
                            }

                            Components.Add(ResultDeciderTheirItems_RepeatFailGroupLabel);
                            Components.Add(ResultDeciderTheirItems_RepeatRadio);
                            Components.Add(ResultDeciderTheirItems_RepeatRadioLabel);
                            Components.Add(ResultDeciderTheirItems_FailRadio);
                            Components.Add(ResultDeciderTheirItems_FailRadioLabel);
                            Components.Add(ResultDeciderTheirItems_Field);
                            Components.Add(ResultDeciderTheirItems_FieldLabel);
                            break;
                        case ResultDeciderType.ItemsTransferred:
                            Components.Add(ResultDeciderItemsTransfered_Description);
                            Components.Add(ResultDeciderItemsTransfered_TypeCheck);
                            Components.Add(ResultDeciderItemsTransfered_TypeCheckLabel);

                            if (ResultDeciderItemsTransfered_TypeCheck.Pushed)
                            {
                                Components.Add(ResultDeciderItemsTransfered_TypeBoxLabel);
                                Components.Add(ResultDeciderItemsTransfered_TypeBox);
                            }

                            Components.Add(ResultDeciderItemsTransfered_RepeatFailGroupLabel);
                            Components.Add(ResultDeciderItemsTransfered_RepeatRadio);
                            Components.Add(ResultDeciderItemsTransfered_RepeatRadioLabel);
                            Components.Add(ResultDeciderItemsTransfered_FailRadio);
                            Components.Add(ResultDeciderItemsTransfered_FailRadioLabel);
                            Components.Add(ResultDeciderItemsTransfered_Field);
                            Components.Add(ResultDeciderItemsTransfered_FieldLabel);
                            break;
                    }

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                Components.Add(ResultDeciderCombo);
            }

            if (ResultDeciderOurItems_Description == null)
            {
                ResultDeciderOurItems_Description = new Text(new Point(0, 0), @"The result of the transfer will not be success 
until our inventory has a certain number of items,
optionally of a specific type. You can choose
between either returning failure or running 
until that happens.", renderContext.DefaultFont, Color.White);
            }

            if (ResultDeciderOurItems_TypeCheckLabel == null)
            {
                ResultDeciderOurItems_TypeCheckLabel = new Text(new Point(0, 0), "By material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderOurItems_TypeCheck == null)
            {
                ResultDeciderOurItems_TypeCheck = new CheckBox(new Point(0, 0));

                ResultDeciderOurItems_TypeCheck.PushedChanged += (sender, args) =>
                {
                    if (ResultDeciderOurItems_TypeCheck.Pushed)
                    {
                        Components.Add(ResultDeciderOurItems_TypeBoxLabel);
                        Components.Add(ResultDeciderOurItems_TypeBox);
                    }
                    else
                    {
                        Components.Remove(ResultDeciderOurItems_TypeBoxLabel);
                        Components.Remove(ResultDeciderOurItems_TypeBox);
                    }

                    Reload = true;
                    OnInspectRedrawRequired();
                };
            }

            if (ResultDeciderOurItems_TypeBoxLabel == null)
            {
                ResultDeciderOurItems_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderOurItems_TypeBox == null)
            {
                ResultDeciderOurItems_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                ResultDeciderOurItems_TypeBox.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderOurItems_TypeBox.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderOurItems_TypeBox.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderOurItems_TypeBox.SelectedChanged += (sender, oldSelected) => OnInspectRedrawRequired();
            }

            if (ResultDeciderOurItems_RepeatFailGroupLabel == null)
            {
                ResultDeciderOurItems_RepeatFailGroupLabel = new Text(new Point(0, 0), "Result if condition not met", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderOurItems_RepeatRadioLabel == null)
            {
                ResultDeciderOurItems_RepeatRadioLabel = new Text(new Point(0, 0), "Running", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderOurItems_FailRadioLabel == null)
            {
                ResultDeciderOurItems_FailRadioLabel = new Text(new Point(0, 0), "Failure", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderOurItems_RepeatRadio == null || ResultDeciderOurItems_FailRadio == null)
            {
                ResultDeciderOurItems_RepeatRadio?.Dispose();
                ResultDeciderOurItems_RepeatRadio = null;
                ResultDeciderOurItems_FailRadio?.Dispose();
                ResultDeciderOurItems_FailRadio = null;

                ResultDeciderOurItems_RepeatRadio = new RadioButton(new Point(0, 0));
                ResultDeciderOurItems_FailRadio = new RadioButton(new Point(0, 0));

                var grp = new RadioButtonGroup(new List<RadioButton> { ResultDeciderOurItems_FailRadio, ResultDeciderOurItems_RepeatRadio });
                grp.Attach();

                ResultDeciderOurItems_RepeatRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderOurItems_FailRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (ResultDeciderOurItems_FieldLabel == null)
            {
                ResultDeciderOurItems_FieldLabel = new Text(new Point(0, 0), PickupRadio.Pushed ? "At least" : "At most", renderContext.DefaultFont, Color.Black);

                EventHandler tmp = (sender, args) =>
                {
                    ResultDeciderOurItems_FieldLabel.Content = PickupRadio.Pushed ? "At least" : "At most";

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                PickupRadio.PushedChanged += tmp;
                DropoffRadio.PushedChanged += tmp;

                ResultDeciderOurItems_FieldLabel.Disposing += (sender, args) =>
                {
                    if (PickupRadio != null)
                        PickupRadio.PushedChanged -= tmp;

                    if (DropoffRadio != null)
                        DropoffRadio.PushedChanged -= tmp;
                };
            }

            if (ResultDeciderOurItems_Field == null)
            {
                ResultDeciderOurItems_Field = CreateTextField(150, 30);
            }

            if (ResultDeciderTheirItems_Description == null)
            {
                ResultDeciderTheirItems_Description = new Text(new Point(0, 0), @"The result of the transfer will not be success
until their inventory has a certain number of 
items, optionally of a specific type. You can
choose between either returning failure or 
running until that happens.", renderContext.DefaultFont, Color.White);
            }

            if (ResultDeciderTheirItems_TypeCheckLabel == null)
            {
                ResultDeciderTheirItems_TypeCheckLabel = new Text(new Point(0, 0), "By material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderTheirItems_TypeCheck == null)
            {
                ResultDeciderTheirItems_TypeCheck = new CheckBox(new Point(0, 0));

                ResultDeciderTheirItems_TypeCheck.PushedChanged += (sender, args) =>
                {
                    if (ResultDeciderTheirItems_TypeCheck.Pushed)
                    {
                        Components.Add(ResultDeciderTheirItems_TypeBox);
                        Components.Add(ResultDeciderTheirItems_TypeBoxLabel);
                    }
                    else
                    {
                        Components.Remove(ResultDeciderTheirItems_TypeBox);
                        Components.Remove(ResultDeciderTheirItems_TypeBoxLabel);
                    }
                };
            }

            if (ResultDeciderTheirItems_TypeBoxLabel == null)
            {
                ResultDeciderTheirItems_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderTheirItems_TypeBox == null)
            {
                ResultDeciderTheirItems_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                ResultDeciderTheirItems_TypeBox.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderTheirItems_TypeBox.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderTheirItems_TypeBox.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderTheirItems_TypeBox.SelectedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (ResultDeciderTheirItems_RepeatFailGroupLabel == null)
            {
                ResultDeciderTheirItems_RepeatFailGroupLabel = new Text(new Point(0, 0), "Result if condition not met", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderTheirItems_RepeatRadioLabel == null)
            {
                ResultDeciderTheirItems_RepeatRadioLabel = new Text(new Point(0, 0), "Running", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderTheirItems_FailRadioLabel == null)
            {
                ResultDeciderTheirItems_FailRadioLabel = new Text(new Point(0, 0), "Failure", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderTheirItems_FailRadio == null || ResultDeciderTheirItems_RepeatRadio == null)
            {
                ResultDeciderTheirItems_RepeatRadio?.Dispose();
                ResultDeciderTheirItems_RepeatRadio = null;
                ResultDeciderTheirItems_FailRadio?.Dispose();
                ResultDeciderTheirItems_FailRadio = null;

                ResultDeciderTheirItems_RepeatRadio = new RadioButton(new Point(0, 0));
                ResultDeciderTheirItems_FailRadio = new RadioButton(new Point(0, 0));

                var grp = new RadioButtonGroup(new List<RadioButton> { ResultDeciderTheirItems_FailRadio, ResultDeciderTheirItems_RepeatRadio });
                grp.Attach();

                ResultDeciderTheirItems_RepeatRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderTheirItems_FailRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (ResultDeciderTheirItems_FieldLabel == null)
            {
                ResultDeciderTheirItems_FieldLabel = new Text(new Point(0, 0), PickupRadio.Pushed ? "At most" : "At least", renderContext.DefaultFont, Color.Black);

                EventHandler tmp = (sender, args) =>
                {
                    ResultDeciderTheirItems_FieldLabel.Content = PickupRadio.Pushed ? "At most" : "At least";

                    Reload = true;
                    OnInspectRedrawRequired();
                };

                PickupRadio.PushedChanged += tmp;
                DropoffRadio.PushedChanged += tmp;

                ResultDeciderTheirItems_FieldLabel.Disposing += (sender, args) =>
                {
                    if (PickupRadio != null)
                        PickupRadio.PushedChanged -= tmp;

                    if (DropoffRadio != null)
                        DropoffRadio.PushedChanged -= tmp;
                };
            }

            if (ResultDeciderTheirItems_Field == null)
            {
                ResultDeciderTheirItems_Field = CreateTextField(150, 30);
            }

            if (ResultDeciderItemsTransfered_Description == null)
            {
                ResultDeciderItemsTransfered_Description = new Text(new Point(0, 0), @"The result of the transfer will not be a success
until a certain number of items have been 
transferred, optionally of a specific type. You
can choose between either returning failure or 
running until that happens.", renderContext.DefaultFont, Color.White);
            }

            if (ResultDeciderItemsTransfered_TypeCheckLabel == null)
            {
                ResultDeciderItemsTransfered_TypeCheckLabel = new Text(new Point(0, 0), "By material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_TypeCheck == null)
            {
                ResultDeciderItemsTransfered_TypeCheck = new CheckBox(new Point(0, 0));

                ResultDeciderItemsTransfered_TypeCheck.PushedChanged += (sender, args) =>
                {
                    if (ResultDeciderItemsTransfered_TypeCheck.Pushed)
                    {
                        Components.Add(ResultDeciderItemsTransfered_TypeBoxLabel);
                        Components.Add(ResultDeciderItemsTransfered_TypeBox);
                    }
                    else
                    {
                        Components.Remove(ResultDeciderItemsTransfered_TypeBoxLabel);
                        Components.Remove(ResultDeciderItemsTransfered_TypeBox);
                    }

                    Reload = true;
                    OnInspectRedrawRequired();
                };
            }

            if (ResultDeciderItemsTransfered_TypeBoxLabel == null)
            {
                ResultDeciderItemsTransfered_TypeBoxLabel = new Text(new Point(0, 0), "Material", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_TypeBox == null)
            {
                ResultDeciderItemsTransfered_TypeBox = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(renderContext.DefaultFont), new Point(250, 34));

                ResultDeciderItemsTransfered_TypeBox.HoveredChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderItemsTransfered_TypeBox.SelectedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderItemsTransfered_TypeBox.ScrollChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderItemsTransfered_TypeBox.ExpandedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (ResultDeciderItemsTransfered_RepeatFailGroupLabel == null)
            {
                ResultDeciderItemsTransfered_RepeatFailGroupLabel = new Text(new Point(0, 0), "Result if condition not met", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_RepeatRadioLabel == null)
            {
                ResultDeciderItemsTransfered_RepeatRadioLabel = new Text(new Point(0, 0), "Running", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_FailRadioLabel == null)
            {
                ResultDeciderItemsTransfered_FailRadioLabel = new Text(new Point(0, 0), "Failure", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_FailRadio == null || ResultDeciderItemsTransfered_RepeatRadio == null)
            {
                ResultDeciderItemsTransfered_FailRadio?.Dispose();
                ResultDeciderItemsTransfered_FailRadio = null;
                ResultDeciderItemsTransfered_RepeatRadio?.Dispose();
                ResultDeciderItemsTransfered_RepeatRadio = null;

                ResultDeciderItemsTransfered_FailRadio = new RadioButton(new Point(0, 0));
                ResultDeciderItemsTransfered_RepeatRadio = new RadioButton(new Point(0, 0));

                var grp = new RadioButtonGroup(new[] { ResultDeciderItemsTransfered_RepeatRadio, ResultDeciderItemsTransfered_FailRadio });
                grp.Attach();

                ResultDeciderItemsTransfered_FailRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
                ResultDeciderItemsTransfered_RepeatRadio.PushedChanged += (sender, args) => OnInspectRedrawRequired();
            }

            if (ResultDeciderItemsTransfered_FieldLabel == null)
            {
                ResultDeciderItemsTransfered_FieldLabel = new Text(new Point(0, 0), "At least", renderContext.DefaultFont, Color.Black);
            }

            if (ResultDeciderItemsTransfered_Field == null)
            {
                ResultDeciderItemsTransfered_Field = CreateTextField(150, 30);
            }

            if(Task != null && !LoadedFromTask)
            {
                LoadedFromTask = true;
                LoadFromTask(renderContext);
            }
        }
        
        void SetupDecider(Material material, int quantity, EntityTaskStatus result, CheckBox typeCheck, ComboBox<Material> typeBox, RadioButton repeatRadio,
            RadioButton failRadio, TextField field)
        {

            if (material != null)
            {
                typeCheck.Pushed = true;
                typeBox.Selected = typeBox.GetComboItemByValue(material);
            }

            if (result == EntityTaskStatus.Running)
            {
                repeatRadio.Pushed = true;
            }
            else
            {
                failRadio.Pushed = true;
            }

            field.Text = quantity.ToString();
        }

        protected void LoadFromTask(RenderContext context)
        {
            var task = Task as EntityTransferItemTask;

            if(task.Pickup)
            {
                PickupRadio.Pushed = true;
            }else
            {
                DropoffRadio.Pushed = true;
            }

            if(task.Targeter != null)
            {
                if(task.Targeter.GetType() == typeof(TransferTargetByID))
                {
                    var tmp = (TransferTargetByID)task.Targeter;
                    TargetDeciderCombo.Selected = TargetDeciderCombo.GetComboItemByValue(TargetDeciderType.ByID);
                    TargetDeciderByID_IDTextField.Text = tmp.TargetID.ToString();
                }else if(task.Targeter.GetType() == typeof(TransferTargetByPosition))
                {
                    var tmp = (TransferTargetByPosition)task.Targeter;
                    TargetDeciderCombo.Selected = TargetDeciderCombo.GetComboItemByValue(TargetDeciderType.ByPosition);
                    TargetDeciderByPosition_XField.Text = tmp.Position.X.ToString();
                    TargetDeciderByPosition_YField.Text = tmp.Position.Y.ToString();
                }else if(task.Targeter.GetType() == typeof(TransferTargetByRelativePosition))
                {
                    var tmp = (TransferTargetByRelativePosition)task.Targeter;
                    TargetDeciderCombo.Selected = TargetDeciderCombo.GetComboItemByValue(TargetDeciderType.ByRelativePosition);
                    TargetDeciderByRelPosition_DXField.Text = ((int)tmp.Offset.DeltaX).ToString();
                    TargetDeciderByRelPosition_DYField.Text = ((int)tmp.Offset.DeltaY).ToString();
                }else
                {
                    throw new InvalidProgramException();
                }
            }

            if(task.Restrictors != null)
            {
                foreach(var restr in task.Restrictors)
                {
                    var comp = new TransferRestrictorComp(this, restr);
                    comp.InitializeThings(context);

                    Restrictors.Add(comp);
                }
            }

            if(task.ResultDecider != null)
            { 
                if(task.ResultDecider.GetType() == typeof(FromInventoryResultDecider))
                {
                    var tmp = (FromInventoryResultDecider)task.ResultDecider;
                    if (PickupRadio.Pushed)
                    {
                        ResultDeciderCombo.Selected = ResultDeciderCombo.GetComboItemByValue(ResultDeciderType.TheirItems);
                        SetupDecider(tmp.KeyMaterial, tmp.KeyQuantity, tmp.ResultIfConditionUnmet, ResultDeciderTheirItems_TypeCheck,
                            ResultDeciderTheirItems_TypeBox, ResultDeciderTheirItems_RepeatRadio, ResultDeciderTheirItems_FailRadio, ResultDeciderTheirItems_Field);
                    }
                    else
                    {
                        ResultDeciderCombo.Selected = ResultDeciderCombo.GetComboItemByValue(ResultDeciderType.OurItems);
                        SetupDecider(tmp.KeyMaterial, tmp.KeyQuantity, tmp.ResultIfConditionUnmet, ResultDeciderOurItems_TypeCheck,
                            ResultDeciderOurItems_TypeBox, ResultDeciderOurItems_RepeatRadio, ResultDeciderOurItems_FailRadio, ResultDeciderOurItems_Field);
                    }
                }else if(task.ResultDecider.GetType() == typeof(ToInventoryResultDecider))
                {
                    var tmp = (ToInventoryResultDecider)task.ResultDecider;
                    if(PickupRadio.Pushed)
                    {
                        ResultDeciderCombo.Selected = ResultDeciderCombo.GetComboItemByValue(ResultDeciderType.OurItems);
                        SetupDecider(tmp.KeyMaterial, tmp.KeyQuantity, tmp.ResultIfConditionUnmet, ResultDeciderOurItems_TypeCheck,
                            ResultDeciderOurItems_TypeBox, ResultDeciderOurItems_RepeatRadio, ResultDeciderOurItems_FailRadio, ResultDeciderOurItems_Field);
                    }else
                    {
                        ResultDeciderCombo.Selected = ResultDeciderCombo.GetComboItemByValue(ResultDeciderType.TheirItems);
                        SetupDecider(tmp.KeyMaterial, tmp.KeyQuantity, tmp.ResultIfConditionUnmet, ResultDeciderTheirItems_TypeCheck,
                            ResultDeciderTheirItems_TypeBox, ResultDeciderTheirItems_RepeatRadio, ResultDeciderTheirItems_FailRadio, ResultDeciderTheirItems_Field);
                    }
                }else if(task.ResultDecider.GetType() == typeof(ItemsTransferedResultDecider))
                {
                    var tmp = (ItemsTransferedResultDecider)task.ResultDecider;
                    ResultDeciderCombo.Selected = ResultDeciderCombo.GetComboItemByValue(ResultDeciderType.ItemsTransferred);
                    SetupDecider(tmp.KeyMaterial, tmp.KeyQuantity, tmp.ResultIfConditionUnmet, ResultDeciderItemsTransfered_TypeCheck,
                        ResultDeciderItemsTransfered_TypeBox, ResultDeciderItemsTransfered_RepeatRadio, ResultDeciderItemsTransfered_FailRadio,
                        ResultDeciderItemsTransfered_Field);
                }else
                {
                    throw new InvalidProgramException();
                }
            }
        }

        /// <summary>
        /// Creates a text field with the specified width and height with all the necessary
        /// redrawing hooks created. Does not add to components.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>The text field</returns>
        TextField CreateTextField(int width, int height, bool numbersOnly = true, bool decimals = false, bool negatives = false)
        {
            var result = UIUtils.CreateTextField(new Point(0, 0), new Point(width, height));

            result.FocusGained += (sender, args) => OnInspectRedrawRequired();
            result.FocusLost += (sender, args) => OnInspectRedrawRequired();

            EventHandler handler = null;
            if(numbersOnly)
                handler = UIUtils.TextFieldRestrictToNumbers(decimals, negatives);

            result.TextChanged += (sender, args) =>
            {
                handler?.Invoke(sender, args);
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

            height += ResultDeciderComboLabel.Size.Y + 3;
            ResultDeciderCombo.Center = new Point(width / 2, height + ResultDeciderCombo.Size.Y / 2);
            ResultDeciderComboLabel.Center = GetCenterForTopLeftLabel(ResultDeciderComboLabel, ResultDeciderCombo);
            height += ResultDeciderCombo.Size.Y + 3;

            if(ResultDeciderCombo.Selected != null)
            {
                int requiredHeight, requiredWidth;
                int x, y;
                switch(ResultDeciderCombo.Selected.Value)
                {
                    case ResultDeciderType.OurItems:
                        ResultDeciderOurItems_Description.Center = new Point(width / 2, height + ResultDeciderOurItems_Description.Size.Y / 2);
                        height += ResultDeciderOurItems_Description.Size.Y + 3;

                        requiredHeight = Math.Max(ResultDeciderOurItems_TypeCheck.Size.Y, ResultDeciderOurItems_TypeCheckLabel.Size.Y);
                        requiredWidth = ResultDeciderOurItems_TypeCheck.Size.X + 3 + ResultDeciderOurItems_TypeCheckLabel.Size.X;
                        x = (width - requiredWidth) / 2;
                        y = height + requiredHeight / 2;
                        ResultDeciderOurItems_TypeCheck.Center = new Point(x + ResultDeciderOurItems_TypeCheck.Size.X / 2, y);
                        x += ResultDeciderOurItems_TypeCheck.Size.X + 3;
                        ResultDeciderOurItems_TypeCheckLabel.Center = new Point(x + ResultDeciderOurItems_TypeCheckLabel.Size.X / 2, y);

                        height += requiredHeight + 3;

                        if (ResultDeciderOurItems_TypeCheck.Pushed)
                        {
                            height += ResultDeciderOurItems_TypeBoxLabel.Size.Y + 3;
                            ResultDeciderOurItems_TypeBox.Center = new Point(width / 2, height + ResultDeciderOurItems_TypeBox.Size.Y / 2);
                            ResultDeciderOurItems_TypeBoxLabel.Center = GetCenterForTopLeftLabel(ResultDeciderOurItems_TypeBoxLabel, ResultDeciderOurItems_TypeBox);
                            height += ResultDeciderOurItems_TypeBox.Size.Y + 3;
                        }

                        height += 5;

                        requiredHeight = Math.Max(Math.Max(ResultDeciderOurItems_FailRadio.Size.Y, ResultDeciderOurItems_RepeatRadio.Size.Y), Math.Max(ResultDeciderOurItems_FailRadioLabel.Size.Y, ResultDeciderOurItems_RepeatRadioLabel.Size.Y));
                        requiredWidth = ResultDeciderOurItems_RepeatRadio.Size.X + 3 + ResultDeciderOurItems_RepeatRadioLabel.Size.X + 7 + ResultDeciderOurItems_FailRadio.Size.X + 3 + ResultDeciderOurItems_FailRadioLabel.Size.X;


                        x = (width - requiredWidth) / 2;
                        ResultDeciderOurItems_RepeatFailGroupLabel.Center = new Point(x + ResultDeciderOurItems_RepeatFailGroupLabel.Size.X / 2, height + ResultDeciderOurItems_RepeatFailGroupLabel.Size.Y / 2);
                        height += ResultDeciderOurItems_RepeatFailGroupLabel.Size.Y + 3;

                        y = height + requiredHeight / 2;
                        ResultDeciderOurItems_RepeatRadio.Center = new Point(x + ResultDeciderOurItems_RepeatRadio.Size.X / 2, y);
                        x += ResultDeciderOurItems_RepeatRadio.Size.X + 3;
                        ResultDeciderOurItems_RepeatRadioLabel.Center = new Point(x + ResultDeciderOurItems_RepeatRadioLabel.Size.X / 2, y);
                        x += ResultDeciderOurItems_RepeatRadioLabel.Size.X + 7;
                        ResultDeciderOurItems_FailRadio.Center = new Point(x + ResultDeciderOurItems_FailRadio.Size.X / 2, y);
                        x += ResultDeciderOurItems_FailRadio.Size.X + 3;
                        ResultDeciderOurItems_FailRadioLabel.Center = new Point(x + ResultDeciderOurItems_FailRadioLabel.Size.X / 2, y);
                        height += requiredHeight + 3;

                        height += ResultDeciderOurItems_FieldLabel.Size.Y + 3;
                        ResultDeciderOurItems_Field.Center = new Point(width / 2, height + ResultDeciderOurItems_Field.Size.Y / 2);
                        ResultDeciderOurItems_FieldLabel.Center = GetCenterForTopLeftLabel(ResultDeciderOurItems_FieldLabel, ResultDeciderOurItems_Field);
                        height += ResultDeciderOurItems_Field.Size.Y + 3;
                        break;
                    case ResultDeciderType.TheirItems:
                        ResultDeciderTheirItems_Description.Center = new Point(width / 2, height + ResultDeciderTheirItems_Description.Size.Y / 2);
                        height += ResultDeciderTheirItems_Description.Size.Y + 3;

                        requiredHeight = Math.Max(ResultDeciderTheirItems_TypeCheck.Size.Y, ResultDeciderTheirItems_TypeCheckLabel.Size.Y);
                        requiredWidth = ResultDeciderTheirItems_TypeCheck.Size.X + 3 + ResultDeciderTheirItems_TypeCheckLabel.Size.X;
                        x = (width - requiredWidth) / 2;
                        y = height + requiredHeight / 2;
                        ResultDeciderTheirItems_TypeCheck.Center = new Point(x + ResultDeciderTheirItems_TypeCheck.Size.X / 2, y);
                        x += ResultDeciderTheirItems_TypeCheck.Size.X + 3;
                        ResultDeciderTheirItems_TypeCheckLabel.Center = new Point(x + ResultDeciderTheirItems_TypeCheckLabel.Size.X / 2, y);

                        height += requiredHeight + 3;

                        if (ResultDeciderTheirItems_TypeCheck.Pushed)
                        {
                            height += ResultDeciderTheirItems_TypeBoxLabel.Size.Y + 3;
                            ResultDeciderTheirItems_TypeBox.Center = new Point(width / 2, height + ResultDeciderTheirItems_TypeBox.Size.Y / 2);
                            ResultDeciderTheirItems_TypeBoxLabel.Center = GetCenterForTopLeftLabel(ResultDeciderTheirItems_TypeBoxLabel, ResultDeciderTheirItems_TypeBox);
                            height += ResultDeciderTheirItems_TypeBox.Size.Y + 3;
                        }

                        height += 5;

                        requiredHeight = Math.Max(Math.Max(ResultDeciderTheirItems_FailRadio.Size.Y, ResultDeciderTheirItems_RepeatRadio.Size.Y), Math.Max(ResultDeciderTheirItems_FailRadioLabel.Size.Y, ResultDeciderTheirItems_RepeatRadioLabel.Size.Y));
                        requiredWidth = ResultDeciderTheirItems_RepeatRadio.Size.X + 3 + ResultDeciderTheirItems_RepeatRadioLabel.Size.X + 7 + ResultDeciderTheirItems_FailRadio.Size.X + 3 + ResultDeciderTheirItems_FailRadioLabel.Size.X;


                        x = (width - requiredWidth) / 2;
                        ResultDeciderTheirItems_RepeatFailGroupLabel.Center = new Point(x + ResultDeciderTheirItems_RepeatFailGroupLabel.Size.X / 2, height + ResultDeciderTheirItems_RepeatFailGroupLabel.Size.Y / 2);
                        height += ResultDeciderTheirItems_RepeatFailGroupLabel.Size.Y + 3;

                        y = height + requiredHeight / 2;
                        ResultDeciderTheirItems_RepeatRadio.Center = new Point(x + ResultDeciderTheirItems_RepeatRadio.Size.X / 2, y);
                        x += ResultDeciderTheirItems_RepeatRadio.Size.X + 3;
                        ResultDeciderTheirItems_RepeatRadioLabel.Center = new Point(x + ResultDeciderTheirItems_RepeatRadioLabel.Size.X / 2, y);
                        x += ResultDeciderTheirItems_RepeatRadioLabel.Size.X + 7;
                        ResultDeciderTheirItems_FailRadio.Center = new Point(x + ResultDeciderTheirItems_FailRadio.Size.X / 2, y);
                        x += ResultDeciderTheirItems_FailRadio.Size.X + 3;
                        ResultDeciderTheirItems_FailRadioLabel.Center = new Point(x + ResultDeciderTheirItems_FailRadioLabel.Size.X / 2, y);
                        height += requiredHeight + 3;

                        height += ResultDeciderTheirItems_FieldLabel.Size.Y + 3;
                        ResultDeciderTheirItems_Field.Center = new Point(width / 2, height + ResultDeciderTheirItems_Field.Size.Y / 2);
                        ResultDeciderTheirItems_FieldLabel.Center = GetCenterForTopLeftLabel(ResultDeciderTheirItems_FieldLabel, ResultDeciderTheirItems_Field);
                        height += ResultDeciderTheirItems_Field.Size.Y + 3;
                        break;
                    case ResultDeciderType.ItemsTransferred:
                        ResultDeciderItemsTransfered_Description.Center = new Point(width / 2, height + ResultDeciderItemsTransfered_Description.Size.Y / 2);
                        height += ResultDeciderItemsTransfered_Description.Size.Y + 3;

                        requiredHeight = Math.Max(ResultDeciderItemsTransfered_TypeCheck.Size.Y, ResultDeciderItemsTransfered_TypeCheckLabel.Size.Y);
                        requiredWidth = ResultDeciderItemsTransfered_TypeCheck.Size.X + 3 + ResultDeciderItemsTransfered_TypeCheckLabel.Size.X;
                        x = (width - requiredWidth) / 2;
                        y = height + requiredHeight / 2;
                        ResultDeciderItemsTransfered_TypeCheck.Center = new Point(x + ResultDeciderItemsTransfered_TypeCheck.Size.X / 2, y);
                        x += ResultDeciderItemsTransfered_TypeCheck.Size.X + 3;
                        ResultDeciderItemsTransfered_TypeCheckLabel.Center = new Point(x + ResultDeciderItemsTransfered_TypeCheckLabel.Size.X / 2, y);

                        height += requiredHeight + 3;

                        if (ResultDeciderItemsTransfered_TypeCheck.Pushed)
                        {
                            height += ResultDeciderItemsTransfered_TypeBoxLabel.Size.Y + 3;
                            ResultDeciderItemsTransfered_TypeBox.Center = new Point(width / 2, height + ResultDeciderItemsTransfered_TypeBox.Size.Y / 2);
                            ResultDeciderItemsTransfered_TypeBoxLabel.Center = GetCenterForTopLeftLabel(ResultDeciderItemsTransfered_TypeBoxLabel, ResultDeciderItemsTransfered_TypeBox);
                            height += ResultDeciderItemsTransfered_TypeBox.Size.Y + 3;
                        }

                        height += 5;

                        requiredHeight = Math.Max(Math.Max(ResultDeciderItemsTransfered_FailRadio.Size.Y, ResultDeciderItemsTransfered_RepeatRadio.Size.Y), Math.Max(ResultDeciderItemsTransfered_FailRadioLabel.Size.Y, ResultDeciderItemsTransfered_RepeatRadioLabel.Size.Y));
                        requiredWidth = ResultDeciderItemsTransfered_RepeatRadio.Size.X + 3 + ResultDeciderItemsTransfered_RepeatRadioLabel.Size.X + 7 + ResultDeciderItemsTransfered_FailRadio.Size.X + 3 + ResultDeciderItemsTransfered_FailRadioLabel.Size.X;


                        x = (width - requiredWidth) / 2;
                        ResultDeciderItemsTransfered_RepeatFailGroupLabel.Center = new Point(x + ResultDeciderItemsTransfered_RepeatFailGroupLabel.Size.X / 2, height + ResultDeciderItemsTransfered_RepeatFailGroupLabel.Size.Y / 2);
                        height += ResultDeciderItemsTransfered_RepeatFailGroupLabel.Size.Y + 3;

                        y = height + requiredHeight / 2;
                        ResultDeciderItemsTransfered_RepeatRadio.Center = new Point(x + ResultDeciderItemsTransfered_RepeatRadio.Size.X / 2, y);
                        x += ResultDeciderItemsTransfered_RepeatRadio.Size.X + 3;
                        ResultDeciderItemsTransfered_RepeatRadioLabel.Center = new Point(x + ResultDeciderItemsTransfered_RepeatRadioLabel.Size.X / 2, y);
                        x += ResultDeciderItemsTransfered_RepeatRadioLabel.Size.X + 7;
                        ResultDeciderItemsTransfered_FailRadio.Center = new Point(x + ResultDeciderItemsTransfered_FailRadio.Size.X / 2, y);
                        x += ResultDeciderItemsTransfered_FailRadio.Size.X + 3;
                        ResultDeciderItemsTransfered_FailRadioLabel.Center = new Point(x + ResultDeciderItemsTransfered_FailRadioLabel.Size.X / 2, y);
                        height += requiredHeight + 3;

                        height += ResultDeciderItemsTransfered_FieldLabel.Size.Y + 3;
                        ResultDeciderItemsTransfered_Field.Center = new Point(width / 2, height + ResultDeciderItemsTransfered_Field.Size.Y / 2);
                        ResultDeciderItemsTransfered_FieldLabel.Center = GetCenterForTopLeftLabel(ResultDeciderItemsTransfered_FieldLabel, ResultDeciderItemsTransfered_Field);
                        height += ResultDeciderItemsTransfered_Field.Size.Y + 3;
                        break;
                }
            }
            height += 8;
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
            height += 150;
        }

        protected bool GetPickup()
        {
            return PickupRadio.Pushed;
        }

        protected ITransferTargeter GetTargeter()
        {
            if(TargetDeciderCombo.Selected == null)
            {
                return null;
            }

            int id, x, y;
            switch (TargetDeciderCombo.Selected.Value)
            {
                case TargetDeciderType.ByID:
                    if (!int.TryParse(TargetDeciderByID_IDTextField.Text, out id))
                        id = 1;

                    return new TransferTargetByID(id);
                case TargetDeciderType.ByPosition:
                    if (!int.TryParse(TargetDeciderByPosition_XField.Text, out x))
                        x = 0;
                    if (!int.TryParse(TargetDeciderByPosition_YField.Text, out y))
                        y = 0;

                    return new TransferTargetByPosition(new PointI2D(x, y));
                case TargetDeciderType.ByRelativePosition:
                    if (!int.TryParse(TargetDeciderByRelPosition_DXField.Text, out x))
                        x = 0;
                    if (!int.TryParse(TargetDeciderByRelPosition_DYField.Text, out y))
                        y = 0;

                    return new TransferTargetByRelativePosition(new VectorD2D(x, y));
            }

            return null;
        }

        protected List<ITransferRestrictor> GetRestrictors(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            var result = new List<ITransferRestrictor>(Restrictors.Count);

            foreach(var restr in Restrictors)
            {
                var tmp = restr.CreateRestrictor(sharedState, localState, netContext);

                if(tmp != null)
                {
                    result.Add(tmp);
                }
            }

            return result;
        }

        protected ITransferResultDecider GetResultDecider()
        {
            if (ResultDeciderCombo.Selected == null)
                return null;

            Material material;
            bool repeat;
            int quantity;
            switch(ResultDeciderCombo.Selected.Value)
            {
                case ResultDeciderType.OurItems:
                    material = (ResultDeciderOurItems_TypeCheck.Pushed && ResultDeciderOurItems_TypeBox.Selected != null) ? ResultDeciderOurItems_TypeBox.Selected.Value : null;
                    repeat = ResultDeciderOurItems_RepeatRadio.Pushed;
                    if (!int.TryParse(ResultDeciderOurItems_Field.Text, out quantity))
                        quantity = 0;

                    if (GetPickup())
                    {
                        return new FromInventoryResultDecider(material, quantity, repeat ? EntityTaskStatus.Running : EntityTaskStatus.Failure);
                    }else
                    {
                        return new ToInventoryResultDecider(material, quantity, repeat ? EntityTaskStatus.Running : EntityTaskStatus.Failure);
                    }
                case ResultDeciderType.TheirItems:
                    material = (ResultDeciderTheirItems_TypeCheck.Pushed && ResultDeciderTheirItems_TypeBox.Selected != null) ? ResultDeciderTheirItems_TypeBox.Selected.Value : null;
                    repeat = ResultDeciderTheirItems_RepeatRadio.Pushed;
                    if (!int.TryParse(ResultDeciderTheirItems_Field.Text, out quantity))
                        quantity = 0;

                    if (!GetPickup())
                    {
                        return new FromInventoryResultDecider(material, quantity, repeat ? EntityTaskStatus.Running : EntityTaskStatus.Failure);
                    }
                    else
                    {
                        return new ToInventoryResultDecider(material, quantity, repeat ? EntityTaskStatus.Running : EntityTaskStatus.Failure);
                    }
                case ResultDeciderType.ItemsTransferred:
                    material = (ResultDeciderItemsTransfered_TypeCheck.Pushed && ResultDeciderItemsTransfered_TypeBox.Selected != null) ? ResultDeciderItemsTransfered_TypeBox.Selected.Value : null;
                    repeat = ResultDeciderItemsTransfered_RepeatRadio.Pushed;
                    if (!int.TryParse(ResultDeciderItemsTransfered_Field.Text, out quantity))
                        quantity = 0;

                    return new ItemsTransferedResultDecider(material, quantity, repeat ? EntityTaskStatus.Running : EntityTaskStatus.Failure);
            }

            return null;
        }

        public override IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if(PickupRadio == null)
            {
                if(Task == null)
                {
                    return new EntityTransferItemTask();
                }else
                {
                    var task = Task as EntityTransferItemTask;
                    return new EntityTransferItemTask(task.SourceID, task.Pickup, task.Targeter, task.Restrictors, task.ResultDecider);
                }
            }

            return new EntityTransferItemTask(((Thing)taskable).ID, GetPickup(), GetTargeter(), GetRestrictors(sharedState, localState, netContext), GetResultDecider());
        }

        protected bool IsTargetValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if (TargetDeciderCombo.Selected == null)
                return false;
            
            int parsed;
            switch (TargetDeciderCombo.Selected.Value)
            {
                case TargetDeciderType.ByID:
                    if (TargetDeciderByID_IDTextField.Text.Length == 0)
                        return false;

                    if (!int.TryParse(TargetDeciderByID_IDTextField.Text, out parsed))
                        return false;

                    if (parsed <= 0)
                        return false;
                    break;
                case TargetDeciderType.ByPosition:
                    if (TargetDeciderByPosition_XField.Text.Length == 0)
                        return false;
                    if (TargetDeciderByPosition_YField.Text.Length == 0)
                        return false;

                    if (!int.TryParse(TargetDeciderByPosition_XField.Text, out parsed))
                        return false;
                    if (parsed < 0 || parsed >= sharedState.World.TileWidth)
                        return false;
                    if (!int.TryParse(TargetDeciderByPosition_YField.Text, out parsed))
                        return false;
                    if (parsed < 0 || parsed >= sharedState.World.TileHeight)
                        return false;
                    break;
                case TargetDeciderType.ByRelativePosition:
                    if (TargetDeciderByRelPosition_DXField.Text.Length == 0)
                        return false;
                    if (TargetDeciderByRelPosition_DYField.Text.Length == 0)
                        return false;

                    if (!int.TryParse(TargetDeciderByRelPosition_DXField.Text, out parsed))
                        return false;
                    if (!int.TryParse(TargetDeciderByRelPosition_DYField.Text, out parsed))
                        return false;
                    break;
            }

            return true;
        }

        protected bool IsResultDeciderValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if (ResultDeciderCombo.Selected == null)
                return false;

            int parsed;
            switch (ResultDeciderCombo.Selected.Value)
            {
                case ResultDeciderType.OurItems:
                    if (ResultDeciderOurItems_TypeCheck.Pushed && ResultDeciderOurItems_TypeBox.Selected == null)
                        return false;
                    if (!ResultDeciderOurItems_RepeatRadio.Pushed && !ResultDeciderOurItems_FailRadio.Pushed)
                        return false;
                    if (ResultDeciderOurItems_Field.Text.Length == 0)
                        return false;
                    if (!int.TryParse(ResultDeciderOurItems_Field.Text, out parsed))
                        return false;
                    if (parsed <= 0)
                        return false;
                    break;
                case ResultDeciderType.TheirItems:
                    if (ResultDeciderTheirItems_TypeCheck.Pushed && ResultDeciderTheirItems_TypeBox.Selected == null)
                        return false;
                    if (!ResultDeciderTheirItems_RepeatRadio.Pushed && !ResultDeciderTheirItems_FailRadio.Pushed)
                        return false;
                    if (ResultDeciderTheirItems_Field.Text.Length == 0)
                        return false;
                    if (!int.TryParse(ResultDeciderTheirItems_Field.Text, out parsed))
                        return false;
                    if (parsed <= 0)
                        return false;
                    break;
                case ResultDeciderType.ItemsTransferred:
                    if (ResultDeciderItemsTransfered_TypeCheck.Pushed && ResultDeciderItemsTransfered_TypeBox.Selected == null)
                        return false;
                    if (!ResultDeciderItemsTransfered_RepeatRadio.Pushed && !ResultDeciderItemsTransfered_FailRadio.Pushed)
                        return false;
                    if (ResultDeciderItemsTransfered_Field.Text.Length == 0)
                        return false;
                    if (!int.TryParse(ResultDeciderItemsTransfered_Field.Text, out parsed))
                        return false;
                    if (parsed <= 0)
                        return false;
                    break;
            }

            return true;
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            if (Children.Count != 0)
                return false;

            if (PickupRadio == null)
                return Task != null && Task.IsValid();

            if (!PickupRadio.Pushed && !DropoffRadio.Pushed)
                return false;

            if (!IsTargetValid(sharedState, localState, netContext))
                return false;

            foreach(var restrictor in Restrictors)
            {
                if (!restrictor.IsValid(sharedState, localState, netContext))
                    return false;
            }

            if (!IsResultDeciderValid(sharedState, localState, netContext))
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

            LoadedFromTask = false;

            PickupRadio?.Dispose();
            PickupRadio = null;

            PickupRadioLabel?.Dispose();
            PickupRadioLabel = null;

            DropoffRadio?.Dispose();
            DropoffRadio = null;

            DropoffRadioLabel?.Dispose();
            DropoffRadioLabel = null;
            
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

            ResultDeciderComboLabel?.Dispose();
            ResultDeciderComboLabel = null;

            ResultDeciderCombo?.Dispose();
            ResultDeciderCombo = null;

            ResultDeciderOurItems_Description?.Dispose();
            ResultDeciderOurItems_Description = null;

            ResultDeciderOurItems_TypeCheckLabel?.Dispose();
            ResultDeciderOurItems_TypeCheckLabel = null;

            ResultDeciderOurItems_TypeCheck?.Dispose();
            ResultDeciderOurItems_TypeCheck = null;

            ResultDeciderOurItems_TypeBoxLabel?.Dispose();
            ResultDeciderOurItems_TypeBoxLabel = null;

            ResultDeciderOurItems_TypeBox?.Dispose();
            ResultDeciderOurItems_TypeBox = null;

            ResultDeciderOurItems_RepeatFailGroupLabel?.Dispose();
            ResultDeciderOurItems_RepeatFailGroupLabel = null;

            ResultDeciderOurItems_RepeatRadioLabel?.Dispose();
            ResultDeciderOurItems_RepeatRadioLabel = null;

            ResultDeciderOurItems_RepeatRadio?.Dispose();
            ResultDeciderOurItems_RepeatRadio = null;

            ResultDeciderOurItems_FailRadioLabel?.Dispose();
            ResultDeciderOurItems_FailRadioLabel = null;

            ResultDeciderOurItems_FailRadio?.Dispose();
            ResultDeciderOurItems_FailRadio = null;

            ResultDeciderOurItems_FieldLabel?.Dispose();
            ResultDeciderOurItems_FieldLabel = null;

            ResultDeciderOurItems_Field?.Dispose();
            ResultDeciderOurItems_Field = null;
            
            Restrictors.Clear();
            Components.Clear();
        }
    }
}