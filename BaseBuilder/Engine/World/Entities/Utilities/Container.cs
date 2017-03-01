using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    /// <summary>
    /// Describes something which has an inventory
    /// </summary>
    public interface Container : Thing
    {
        EntityInventory Inventory { get; set; }
    }
}
