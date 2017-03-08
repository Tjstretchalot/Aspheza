using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Screens.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
     *           Checkbox for allow or deny material
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

    public class GiveItemTaskItem : SimpleTaskItem
    {
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
        


        /// <summary>
        /// Converts the specified task into the task item.
        /// </summary>
        /// <param name="task">The task</param>
        public GiveItemTaskItem(EntityGiveItemTask task) : this()
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task = task;
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Give Item";
        }

        /// <summary>
        /// Creates a give item task item with no child. This will
        /// mean the give item task item is in a bad state.
        /// </summary>
        public GiveItemTaskItem()
        {
            Children = new List<ITaskItem>();

            InspectDescription = _InspectDescription;
            Expandable = false;
            Expanded = false;
            TaskName = "Give Item";
        }

        protected override void CalculateHeightPostButtonsAndInitButtons(RenderContext renderContext, ref int height, int width)
        {
            base.CalculateHeightPostButtonsAndInitButtons(renderContext, ref height, width);
        }

        public override IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext)
        {
            return Children.Count == 0;
        }
    }
}
