using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State.Resources;
using static BaseBuilder.Engine.World.Entities.EntityInventory;
using Lidgren.Network;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    /// <summary>
    /// Base class for harvestable entities. Allows the subclass to choose any number and type
    /// of requirements to result in any number or type of results, and allows multiple recipes.
    /// </summary>
    public abstract class HarvestableEntity : ImmobileEntity, Container, Harvestable
    {
        /// <summary>
        /// The inventory that inputs go into
        /// </summary>
        public EntityInventory Inventory { get; set; }

        /// <summary>
        /// The inventory that outputs go into
        /// </summary>
        protected EntityInventory OutputInventory;

        /// <summary>
        /// The recipes that are allowed.
        /// </summary>
        protected List<HarvestableRecipe> Recipes;

        protected HarvestableRecipe _CurrentRecipe;

        /// <summary>
        /// The current recipe that we are building
        /// </summary>
        protected HarvestableRecipe CurrentRecipe
        {
            get
            {
                return _CurrentRecipe;
            }

            set
            {
                if(_CurrentRecipe != value)
                {
                    _CurrentRecipe = value;
                    OnRecipeChanged();
                }
            }
        }

        /// <summary>
        /// Called after CurrentRecipe changes
        /// </summary>
        public event EventHandler RecipeChanged;

        /// <summary>
        /// A craftspeed of 1 implies the recipe will take its CraftTime
        /// to craft. 2 is half the time, 3 is one third, ect.
        /// </summary>
        public double CraftSpeed;

        /// <summary>
        /// The time for this recipe to be completed in milliseconds. In 
        /// order to ensure accuracy, this is an actual time in milliseconds,
        /// which is found by taking the recipe time and dividing by the
        /// craft speed, then rounding.
        /// </summary>
        public int TimeUntilThisRecipeCraftedMS;

        public override string HoverText
        {
            get
            {
                var first = true;
                var result = new StringBuilder("Recipes:");
                
                foreach(var recipe in Recipes)
                {
                    result.AppendLine().Append("  ");
                    recipe.PrettyToString(result);
                }
                
                result.AppendLine().Append("Current Inputs: ");

                var simplInputs = Inventory.GetSimplifiedInventory();

                first = true;
                foreach(var kvp in simplInputs)
                {
                    if (first)
                        first = false;
                    else
                        result.Append(", ");

                    result.Append(kvp.Key.Name);
                    if (kvp.Value != 1)
                        result.Append("x").Append(kvp.Value);
                }

                result.AppendLine().Append("Current Outputs: ");

                var simplOutputs = OutputInventory.GetSimplifiedInventory();

                first = true;
                foreach (var kvp in simplOutputs)
                {
                    if (first)
                        first = false;
                    else
                        result.Append(", ");

                    result.Append(kvp.Key.Name);
                    if (kvp.Value != 1)
                        result.Append("x").Append(kvp.Value);
                }

                result.AppendLine().Append("Current Recipe: ");

                if (CurrentRecipe == null)
                    result.Append("None");
                else
                {
                    CurrentRecipe.PrettyToString(result);
                }
                return result.ToString();
            }
        }
        /// <summary>
        /// To be used with FromMessage
        /// </summary>
        protected HarvestableEntity()
        {
        }

        /// <summary>
        /// Used when created locally, sets position, collision mesh, id, and recipes.
        /// </summary>
        /// <param name="position">Where this harvestable entity is located</param>
        /// <param name="collisionMesh">The collision mesh</param>
        /// <param name="id">The ID</param>
        /// <param name="recipes">The recipe</param>
        protected HarvestableEntity(PointD2D position, CollisionMeshD2D collisionMesh, int id, List<HarvestableRecipe> recipes) : base(position, collisionMesh, id)
        {
            Recipes = recipes;
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(CurrentRecipe != null)
            {
                TimeUntilThisRecipeCraftedMS -= timeMS;

                if(TimeUntilThisRecipeCraftedMS <= 0)
                {
                    if(!CurrentRecipe.HasRoomForOutputs(OutputInventory))
                    {
                        CurrentRecipe = null;
                    }else if(!CurrentRecipe.MatchesThisRecipe(Inventory))
                    {
                        CurrentRecipe = null;
                    }else
                    {
                        CurrentRecipe.Craft(Inventory, OutputInventory);
                        RecheckRecipe();
                    }
                }
            }
        }
        /// <summary>
        /// Syncs the current craft from the network. Must be called
        /// after initializing recipes.
        /// </summary>
        /// <param name="message">The incoming message</param>
        protected void CurrentCraftFromMessage(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
            {
                int currRecipeIndex = message.ReadInt32();

                CurrentRecipe = Recipes[currRecipeIndex];

                TimeUntilThisRecipeCraftedMS = message.ReadInt32();
            }
        }

        /// <summary>
        /// Writes the current craft to the message
        /// </summary>
        /// <param name="message">The message</param>
        protected void WriteCurrentCraft(NetOutgoingMessage message)
        {
            if(CurrentRecipe == null)
            {
                message.Write(false);
            }else
            {
                message.Write(Recipes.FindIndex((r) => ReferenceEquals(r, CurrentRecipe)));
                message.Write(TimeUntilThisRecipeCraftedMS);
            }
        }
        /// <summary>
        /// Initializes the listeners that are required to make the recipes work
        /// </summary>
        protected virtual void InitRecipeListeners()
        {
            Inventory.AcceptsMaterialFunc = AllowedAmountOfInput;
            Inventory.OnMaterialAdded += OnMaterialAdded;
        }

        /// <summary>
        /// Determines how much of the specified material, up to amount, can be
        /// allowed in this inventory right now. This is calculated to ensure both
        /// that the material, when added to our inventory, matches at least one recipe
        /// AND would not cause the inventory to have multiple stacks of the material.
        /// </summary>
        /// <param name="material">The material</param>
        /// <param name="amount">Max to return</param>
        /// <returns></returns>
        private int AllowedAmountOfInput(Material material, int amount)
        {
            var asTuple = Tuple.Create(material, amount);
            if (CurrentRecipe == null)
            {
                bool foundMatchingRecipe = false;
                foreach (var recipe in Recipes)
                {
                    if (recipe.MightBuildToThisRecipe(Inventory, asTuple))
                    {
                        foundMatchingRecipe = true;
                        break;
                    }
                }

                if (!foundMatchingRecipe)
                    return 0;
            }else
            {
                if (!CurrentRecipe.Inputs.Any((tup) => tup.Item1 == material))
                    return 0;
            }

            return Math.Min(amount, Inventory.GetStackSizeFor(material) - Inventory.GetAmountOf(material));
        }

        /// <summary>
        /// Updates the current recipe if there is no recipe and we now meet the 
        /// requirements for one.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">Args</param>
        private void OnMaterialAdded(object sender, InventoryChangedEventArgs args)
        {
            RecheckRecipe();
        }

        private void RecheckRecipe()
        {
            // This could be made a lot faster, but it probably doesn't matter
            var newRecipe = Recipes.Find((rcp) => rcp.MatchesThisRecipe(Inventory));

            if (newRecipe != CurrentRecipe)
            {
                CurrentRecipe = newRecipe;
                if(CurrentRecipe != null)
                    TimeUntilThisRecipeCraftedMS = (int)Math.Round(CurrentRecipe.CraftTime / CraftSpeed);
            }
        }

        /// <summary>
        /// Invokes RecipeChanged
        /// </summary>
        protected virtual void OnRecipeChanged()
        {
            RecipeChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return OutputInventory.GetCount() > 0;
        }

        public string GetHarvestNamePretty()
        {
            StringBuilder result = new StringBuilder("");

            bool first = true;
            for (int i = 0; i < OutputInventory.Slots; i++) {
                var matTup = OutputInventory.MaterialAt(i);
                if (matTup == null)
                    continue;

                if (first)
                    first = false;
                else
                    result.Append(", ");

                result.Append(matTup.Item1.Name).Append('x').Append(matTup.Item2);
            }

            if (first)
                return "Nothing";

            return result.ToString();
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            for(int i = 0; i < OutputInventory.Slots; i++)
            {
                var matAt = OutputInventory.MaterialAt(i);

                var amtTransfered = reciever.Inventory.AddMaterial(matAt.Item1, matAt.Item2);
                OutputInventory.RemoveMaterial(matAt.Item1, amtTransfered);
            }
        }
    }
}
