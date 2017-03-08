using BaseBuilder.Engine.State.Resources;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace BaseBuilder.Engine.World.Entities
{
    /// <summary>
    /// Describes the inventory of an entity. Not all entities have an inventory.
    /// Unlike for the material manager, entity inventories are position-aware. That
    /// is to say, index 0 might be empty and index 1 might have something.
    /// </summary>
    public class EntityInventory
    {
        /// <summary>
        /// Contains the data behind the inventory. int is the number of Material.
        /// </summary>
        protected Tuple<Material, int>[] Inventory;

        /// <summary>
        /// The inventory determines the max stack count, that way you can have 
        /// dudes that are better at holding certain materials than others.
        /// 
        /// If a material is not in this dictionary, a limit of DefaultStackSize is assumed.
        /// </summary>
        protected Dictionary<Material, int> MaterialsToMaxStackCount;

        /// <summary>
        /// Called when material is added to this inventory
        /// </summary>
        public event EventHandler OnMaterialAdded;

        /// <summary>
        /// Called when material is removed from this inventory
        /// </summary>
        public event EventHandler OnMaterialRemoved;

        /// <summary>
        /// Returns true if the specified material is allowed in this inventory
        /// right now
        /// </summary>
        protected Func<Material, bool> AcceptsMaterialFunc;

        /// <summary>
        /// Stack size to assume if none given
        /// </summary>
        protected int DefaultStackSize;

        /// <summary>
        /// Initializes an empty inventory with the specifeid number of
        /// max items
        /// </summary>
        /// <param name="maxItems">Maximum number of items</param>
        /// <param name="acceptsMatFunc">If this inv</param>
        /// <exception cref="ArgumentOutOfRangeException">If maxItems is not strictly positive</exception>
        public EntityInventory(int maxItems, Func<Material, bool> acceptsMatFunc = null)
        {
            if (maxItems <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxItems), maxItems, "Max items must be strictly positive");

            Inventory = new Tuple<Material, int>[maxItems];
            MaterialsToMaxStackCount = new Dictionary<Material, int>();
            AcceptsMaterialFunc = (acceptsMatFunc == null ? (m) => true : acceptsMatFunc);
            DefaultStackSize = 1;
        }

        /// <summary>
        /// Loads an inventory from the incoming message
        /// </summary>
        /// <param name="message">The message</param>
        /// <exception cref="ArgumentNullException">If message is null</exception>
        public EntityInventory(NetIncomingMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            int maxItems = message.ReadInt32();

            Inventory = new Tuple<Material, int>[maxItems];
            for (int i = 0; i < maxItems; i++)
            {
                if (message.ReadBoolean())
                {
                    int matID = message.ReadInt32();
                    int count = message.ReadInt32();

                    Inventory[i] = Tuple.Create(Material.GetMaterialByID(matID), count);
                }
            }

            int numMatsMaxStackCount = message.ReadInt32();
            MaterialsToMaxStackCount = new Dictionary<Material, int>(numMatsMaxStackCount);

            for (int i = 0; i < numMatsMaxStackCount; i++)
            {
                int matID = message.ReadInt32();
                int max = message.ReadInt32();

                MaterialsToMaxStackCount.Add(Material.GetMaterialByID(matID), max);
            }

            DefaultStackSize = message.ReadInt32();
        }

        /// <summary>
        /// Writes the inventory to the message
        /// </summary>
        /// <param name="message">The message to write to</param>
        /// <exception cref="ArgumentNullException">If message is null</exception>
        public void Write(NetOutgoingMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.Write(Inventory.Length);

            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] == null)
                {
                    message.Write(false);
                } else
                {
                    message.Write(true);
                    message.Write(Inventory[i].Item1.ID);
                    message.Write(Inventory[i].Item2);
                }
            }

            message.Write(MaterialsToMaxStackCount.Count);

            foreach (var kvp in MaterialsToMaxStackCount)
            {
                message.Write(kvp.Key.ID);
                message.Write(kvp.Value);
            }

            message.Write(DefaultStackSize);
        }

        /// <summary>
        /// Sets the default stack size - the stack size that is assumed if none is set.
        /// </summary>
        /// <param name="defaultStackSize">New default stack size</param>
        public void SetDefaultStackSize(int defaultStackSize)
        {
            if (defaultStackSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultStackSize), defaultStackSize, "Default stack size must be strictly positive");

            DefaultStackSize = defaultStackSize;
        }

        /// <summary>
        /// Gets the maximum stack size for the specified material in this inventory.
        /// </summary>
        /// <param name="material">The material</param>
        /// <returns>Max stack size of material in this inventory</returns>
        /// <exception cref="ArgumentNullException">If material is null</exception>
        public int GetStackSizeFor(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            int result;
            if (MaterialsToMaxStackCount.TryGetValue(material, out result))
                return result;
            return DefaultStackSize;
        }

        /// <summary>
        /// Sets the maximum stack size for the specified material in this inventory.
        /// </summary>
        /// <param name="material">The material</param>
        /// <param name="amount">The stack size</param>
        /// <exception cref="ArgumentNullException">If material is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If amount is not strictly positive</exception>
        public void SetStackSizeFor(Material material, int amount)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be strictly positive");

            if (amount == 1)
            {
                if (MaterialsToMaxStackCount.ContainsKey(material))
                    MaterialsToMaxStackCount.Remove(material);
            } else
            {
                MaterialsToMaxStackCount[material] = amount;
            }
        }

        /// <summary>
        /// Returns the material and amount of material at the specified index, or null
        /// if there is nothing at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>Material and amount at index or null if nothing</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is less than 0 or greater than/equal to max size</exception>
        public Tuple<Material, int> MaterialAt(int index)
        {
            if (index < 0 || index > Inventory.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be 0 <= index <= {Inventory.Length}");

            return Inventory[index];
        }

        /// <summary>
        /// Removes the material at the specified index. Does nothing if there is nothing
        /// at that index.
        /// </summary>
        /// <param name="index">The index</param>
        public void RemoveMaterialAt(int index)
        {
            if (index < 0 || index > Inventory.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be 0 <= index <= {Inventory.Length}");

            Inventory[index] = null;
        }

        /// <summary>
        /// Returns true if there is enough room in this inventory to add the specified
        /// amount of material, otherwise returns false.
        /// </summary>
        /// <param name="material">The material to add</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>If there is room for amount of material</returns>
        /// <exception cref="ArgumentNullException">If material is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If amount is not strictly positive</exception>
        public bool HaveRoomFor(Material material, int amount)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be strictly positive");

            if (!AcceptsMaterialFunc(material))
                return false;

            int spacesFound = 0;

            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] == null)
                {
                    spacesFound += GetStackSizeFor(material);

                    if (spacesFound >= amount)
                        return true;
                } else if (Inventory[i].Item1 == material)
                {
                    spacesFound += GetStackSizeFor(material) - Inventory[i].Item2;

                    if (spacesFound >= amount)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if there is room in this inventory for all of the specified
        /// materials. Must not contain any duplicate materials.
        /// </summary>
        /// <param name="materials">The materials</param>
        /// <returns>If there is room in this inventory for all of the materials</returns>
        public bool HaveRoomFor(params Tuple<Material, int>[] materials)
        {
            if (materials.Length == 0)
                return true;

#if DEBUG
            for(int i = 0; i < materials.Length; i++)
            {
                for(int j = i + 1; j < materials.Length; j++)
                {
                    if (materials[i].Item1 == materials[j].Item1)
                    {
                        var matStr = string.Join(", ", new List<Tuple<Material, int>>(materials).ConvertAll((tup) => $"{tup.Item1}x{tup.Item2}"));
                        throw new InvalidOperationException($"Duplicate materials! materials={matStr}");
                    }
                }
            }
#endif

            var usedIndexes = new HashSet<int>();

            foreach(var mat in materials)
            {
                int spacesFound = 0;

                for(int i = 0; i < Inventory.Length; i++)
                {
                    if (usedIndexes.Contains(i))
                        continue;

                    if (Inventory[i] == null)
                        continue;

                    if(Inventory[i].Item1 == mat.Item1)
                    {
                        spacesFound += GetStackSizeFor(mat.Item1) - Inventory[i].Item2;

                        if (spacesFound >= mat.Item2)
                            break;
                    }
                }

                if (spacesFound >= mat.Item2)
                    continue;

                for(int i = 0; i < Inventory.Length; i++)
                {
                    if (usedIndexes.Contains(i))
                        continue;

                    if (Inventory[i] == null)
                    {
                        usedIndexes.Add(i);
                        spacesFound += GetStackSizeFor(mat.Item1);

                        if (spacesFound >= mat.Item2)
                            break;
                    }
                }

                if (spacesFound < mat.Item2)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a Dictionary of material to amount held for this container
        /// </summary>
        /// <returns></returns>
        public Dictionary<Material, int> GetSimplifiedInvitory()
        {
            var result = new Dictionary<Material, int>();

            int amount;
            foreach (Material mat in Enum.GetValues(typeof(Material)))
            {
                amount = GetAmountOf(mat);
                if (amount > 0)
                    result.Add(mat, amount);
            }

            return result;
        }

        /// <summary>
        /// Determines the amount of the specified material in this inventory
        /// </summary>
        /// <param name="material">The material</param>
        /// <returns>Amount of material in this inventory</returns>
        /// <exception cref="ArgumentNullException">If material is not strictly positive</exception>
        public int GetAmountOf(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            int amount = 0;

            for(int i = 0; i < Inventory.Length; i++)
            {
                if(Inventory[i] != null && Inventory[i].Item1 == material)
                {
                    amount += Inventory[i].Item2;
                }
            }

            return amount;
        }

        /// <summary>
        /// Adds amount of material to this inventory, filling stacks first then
        /// empty spaces in ascending order from 0. If there is not enough room, it 
        /// adds as much as possible and returns the number added.
        /// </summary>
        /// <param name="material">The material to add</param>
        /// <param name="maxAmount">Max amount of material to add</param>
        /// <exception cref="ArgumentNullException">If material is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If amount is not strictly positive</exception>
        /// <returns>The amount of material added to this inventory</returns>
        public int AddMaterial(Material material, int maxAmount)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (maxAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAmount), maxAmount, "Max amount must be strictly positive");

            if (!AcceptsMaterialFunc(material))
                return 0;

            // Add to existing stacks

            int amountAdded = 0;
            int stackSize = GetStackSizeFor(material);
            if (stackSize > 1)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    if(Inventory[i] != null && Inventory[i].Item1 == material)
                    {
                        int spaceAvailable = stackSize - Inventory[i].Item2;
                        if (spaceAvailable == 0)
                            continue;

                        if(spaceAvailable + amountAdded >= maxAmount)
                        {
                            // There is enough space
                            int amountToAdd = maxAmount - amountAdded;
                            Inventory[i] = Tuple.Create(material, Inventory[i].Item2 + amountToAdd);
                            OnMaterialAdded?.Invoke(this, EventArgs.Empty);
                            return maxAmount;
                        }else // if(spaceAvailable + amountAdded < amount)
                        {
                            // There isn't enough space
                            Inventory[i] = Tuple.Create(material, stackSize);
                            amountAdded += spaceAvailable;
                        }
                    }
                }
            }

            // Add to empty slots

            for(int i = 0; i < Inventory.Length; i++)
            {
                if(Inventory[i] == null)
                {
                    if(amountAdded + stackSize >= maxAmount)
                    {
                        // There is enough space
                        int amountToAdd = maxAmount - amountAdded;
                        Inventory[i] = Tuple.Create(material, amountToAdd);
                        OnMaterialAdded?.Invoke(this, EventArgs.Empty);
                        return maxAmount;
                    }else
                    {
                        // There isn't enough space
                        Inventory[i] = Tuple.Create(material, stackSize);
                        amountAdded += stackSize;
                    }
                }
            }

            OnMaterialAdded?.Invoke(this, EventArgs.Empty);
            return amountAdded;
        }

        /// <summary>
        /// Removes at most maxAmount material from this inventory and returns the
        /// actual amount removed. Incomplete stacks are preferred over complete
        /// ones.
        /// </summary>
        /// <param name="material">The material to remove</param>
        /// <param name="maxAmount">The maximum amount to remove</param>
        /// <returns>The amount of material removed</returns>
        /// <exception cref="ArgumentNullException">If material is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If max amount is not strictly positive</exception>
        public int RemoveMaterial(Material material, int maxAmount)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (maxAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxAmount), maxAmount, "Maximum amount must be strictly positive");

            int amountRemoved = 0;
            int stackSize = GetStackSizeFor(material);
            
            // Incomplete stacks
            if (stackSize > 1)
            {
                for (int i = 0; i < Inventory.Length; i++)
                {
                    if (Inventory[i] != null && Inventory[i].Item1 == material)
                    {
                        if(Inventory[i].Item2 < stackSize)
                        {
                            if(Inventory[i].Item2 + amountRemoved > maxAmount)
                            {
                                // There is more than we need here
                                int amountToRemove = maxAmount - amountRemoved;
                                Inventory[i] = Tuple.Create(material, Inventory[i].Item2 - amountToRemove);
                                OnMaterialRemoved?.Invoke(this, EventArgs.Empty);
                                return maxAmount;
                            }else if(Inventory[i].Item2 + amountRemoved == maxAmount)
                            {
                                // There is exactly enough here
                                Inventory[i] = null;
                                return maxAmount;
                            }else // if(Inventory[i].Item2 + amountRemoved < maxAmount)
                            {
                                // There is not enough here
                                amountRemoved += Inventory[i].Item2;
                                Inventory[i] = null;
                            }
                        }
                    }
                }
            }

            // Complete stacks
            for (int i = 0; i < Inventory.Length; i++)
            {
                if(Inventory[i] != null && Inventory[i].Item1 == material)
                {
                    if (Inventory[i].Item2 + amountRemoved > maxAmount)
                    {
                        // There is more than we need here
                        int amountToRemove = maxAmount - amountRemoved;
                        Inventory[i] = Tuple.Create(material, Inventory[i].Item2 - amountToRemove);
                        OnMaterialRemoved?.Invoke(this, EventArgs.Empty);
                        return maxAmount;
                    }
                    else if (Inventory[i].Item2 + amountRemoved == maxAmount)
                    {
                        // There is exactly enough here
                        Inventory[i] = null;
                        return maxAmount;
                    }
                    else // if(Inventory[i].Item2 + amountRemoved < maxAmount)
                    {
                        // There is not enough here
                        amountRemoved += Inventory[i].Item2;
                        Inventory[i] = null;
                    }
                }
            }

            OnMaterialRemoved?.Invoke(this, EventArgs.Empty);
            return amountRemoved;
        }
    }
}