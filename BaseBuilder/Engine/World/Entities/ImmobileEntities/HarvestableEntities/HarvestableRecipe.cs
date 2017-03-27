using BaseBuilder.Engine.State.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    /// <summary>
    /// Describes a recipe in a harvestable entity. Recipes are always
    /// shapeless
    /// </summary>
    public class HarvestableRecipe
    {
        /// <summary>
        /// What goes in to the recipe
        /// </summary>
        public List<Tuple<Material, int>> Inputs;

        /// <summary>
        /// What comes out of the recipe
        /// </summary>
        public List<Tuple<Material, int>> Outputs;

        /// <summary>
        /// How long this recipe takes to craft
        /// </summary>
        public int CraftTime;

        /// <summary>
        /// Initializes a new recipe with the specified inputs and outputs
        /// </summary>
        /// <param name="inputs">The inputs</param>
        /// <param name="outputs">The outputs</param>
        public HarvestableRecipe(List<Tuple<Material, int>> inputs, List<Tuple<Material, int>> outputs, int craftTime)
        {
#if DEBUG
            for(int i = 0; i < inputs.Count; i++)
            {
                for(int j = i + 1; j < inputs.Count; j++)
                {
                    if(inputs[i].Item1 == inputs[j].Item1)
                    {
                        throw new ArgumentException($"Inputs contains duplicate material types! inputs={inputs}, inputs[{i}].Item1 = inputs[{j}].Item1 = {inputs[i].Item1}", nameof(inputs));
                    }
                }
            }

            for(int i = 0; i < outputs.Count; i++)
            {
                for(int j = i + 1; j < outputs.Count; j++)
                {
                    if(outputs[i].Item1 == outputs[j].Item1)
                    {
                        throw new ArgumentException($"Outputs contains duplicate material types! outputs={outputs}, outputs[{i}].Item1 = outputs[{j}].Item1 = {outputs[i].Item1}", nameof(outputs));
                    }
                }
            }
#endif
            Inputs = inputs;
            Outputs = outputs;
            CraftTime = craftTime;
        }

        /// <summary>
        /// Determines if, when added to the existing inventory, the newItem might
        /// build into this recipe. That means that the inventory has no items that
        /// aren't required for this recipe and that the new item is also in Inputs.
        /// </summary>
        /// <param name="inventory">The input inventory</param>
        /// <param name="newItem">The item trying to be added</param>
        /// <returns>If that item helps build this recipe</returns>
        public bool MightBuildToThisRecipe(EntityInventory inventory, Tuple<Material, int> newItem)
        {
            for(int i = 0; i < inventory.Slots; i++)
            {
                var matAt = inventory.MaterialAt(i);
                if (matAt != null && !Inputs.Any((inputTup) => inputTup.Item1 == matAt.Item1))
                    return false;
            }

            return Inputs.Any((inputTup) => inputTup.Item1 == newItem.Item1);
        }

        /// <summary>
        /// Appends a pretty version of this recipe to the specified
        /// string builder
        /// </summary>
        /// <param name="builder">The string builder</param>
        public void PrettyToString(StringBuilder builder)
        {
            var first = true;
            foreach (var inp in Inputs)
            {
                if (first)
                    first = false;
                else
                    builder.Append(", ");
                builder.Append(inp.Item1.Name);

                if (inp.Item2 != 1)
                    builder.Append('x').Append(inp.Item2);
            }
            builder.Append(" => ");
            first = true;
            foreach (var outp in Outputs)
            {
                if (first)
                    first = false;
                else
                    builder.Append(", ");
                builder.Append(outp.Item1.Name);

                if (outp.Item2 != 1)
                    builder.Append('x').Append(outp.Item2);
            }
        }

        /// <summary>
        /// Determines if the specified inventory contains everything for this 
        /// recipe and nothing not matching this recipe
        /// </summary>
        /// <param name="inventory">The inventory</param>
        /// <returns>If this recipe can be applied to it</returns>
        public bool MatchesThisRecipe(EntityInventory inventory)
        {
            // Two checks: That the inventory doesn't have anything extra,
            // that the inventory has everything required.

            // Step 1: Nothing extra
            for(int i = 0; i < inventory.Slots; i++)
            {
                var matAt = inventory.MaterialAt(i);

                if (matAt != null && !Inputs.Any((inputTup) => inputTup.Item1 == matAt.Item1))
                    return false;
            }

            // Step 2: Everything required
            foreach(var input in Inputs)
            {
                var amt = inventory.GetAmountOf(input.Item1);

                if (amt < input.Item2)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the specified inventory has room for all the outputs
        /// of this recipe.
        /// </summary>
        /// <param name="inventory">The inventory</param>
        /// <returns>If it has room for the outputs</returns>
        public bool HasRoomForOutputs(EntityInventory inventory)
        {
            return inventory.HaveRoomFor(Outputs.ToArray());
        }

        /// <summary>
        /// Removes the inputs from input and places the outputs
        /// in output.
        /// </summary>
        /// <param name="input">Input inventory</param>
        /// <param name="output">Output inventory</param>
        public void Craft(EntityInventory input, EntityInventory output)
        {
            foreach(var inputTup in Inputs)
            {
                input.RemoveMaterial(inputTup.Item1, inputTup.Item2);
            }

            foreach(var outputTup in Outputs)
            {
                output.AddMaterial(outputTup.Item1, outputTup.Item2);
            }
        }
    }
}
