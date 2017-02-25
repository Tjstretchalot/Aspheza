using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.State.Resources
{
    /// <summary>
    /// Manages the resources that the players have. 
    /// </summary>
    /// <remarks>
    /// It's tricky to avoid name conflicts for something like this.
    /// 
    /// Things to keep in mind when designing this class:
    ///   - Resource flow menu (am I net gaining or losing resources?)
    /// </remarks>
    public class MaterialManager
    {
        protected Dictionary<Material, int> MaterialsToQuantities;
        
        /// <summary>
        /// Initializes the material manager with nothing
        /// </summary>
        public MaterialManager()
        {
            MaterialsToQuantities = new Dictionary<Material, int>();
            MaterialsToQuantities[Material.GoldOre] = 0;
        }

        /// <summary>
        /// Initializes the material manager from a message written to with
        /// Write
        /// </summary>
        /// <param name="message">The message to read from</param>
        public MaterialManager(NetIncomingMessage message)
        {
            int numMaterials = message.ReadInt32();
            MaterialsToQuantities = new Dictionary<Material, int>(numMaterials);
            
            for(int i = 0; i < numMaterials; i++)
            {
                int matID = message.ReadInt32();
                int amount = message.ReadInt32();

                MaterialsToQuantities[Material.GetMaterialByID(matID)] = amount;
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(MaterialsToQuantities.Count);

            foreach(var kvp in MaterialsToQuantities)
            {
                message.Write(kvp.Key.ID);
                message.Write(kvp.Value);
            }
        }

        /// <summary>
        /// Tries to spend the specified amount of material. If there is
        /// sufficient quantity to spend the specified amount, the amount is
        /// reducted and the function returns true. If there is insufficient
        /// amount, nothing is deducted and the function returns false
        /// </summary>
        /// <param name="material">The material to spend</param>
        /// <param name="amount">The amount of it to spend</param>
        /// <returns>If resources were spent</returns>
        public bool TrySpend(Material material, int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException($"Can't spend negative amounts of materials; amount={amount}");
            int currentAmount = MaterialsToQuantities[material];

            if (currentAmount < amount)
                return false;

            currentAmount -= amount;
            MaterialsToQuantities[material] = currentAmount;
            return true;
        }

        /// <summary>
        /// Returns true if there is at least amount of material
        /// </summary>
        /// <param name="material">The material</param>
        /// <param name="amount">The amount</param>
        /// <returns>True if at least amount of material</returns>
        public bool HaveAtleast(Material material, int amount)
        {
            return MaterialsToQuantities[material] >= amount;
        }

        /// <summary>
        /// Adds the specified amount of the material
        /// </summary>
        /// <param name="material">The </param>
        /// <param name="amount"></param>
        public void AddMaterial(Material material, int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException($"Can't add negative amounts of material (amount = {amount})");

            MaterialsToQuantities[material] += amount;
        }

        /// <summary>
        /// Gets the amount of the specified material available.
        /// </summary>
        /// <param name="material">The material</param>
        /// <returns>The amount currently available</returns>
        public int AmountOf(Material material)
        {
            return MaterialsToQuantities[material];
        }
    }
}
