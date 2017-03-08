using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class Transferer
    {
        public Dictionary<Material, int> Transfer(SharedGameState sharedGameState, Container from, Entity to, Dictionary<Material, int> maxes)
        {
            Dictionary<Material, int> transfered = new Dictionary<Material, int>();
            var _to = (Container)to;

            foreach (var tuple in maxes)
            {
                transfered.Add(tuple.Key, _to.Inventory.AddMaterial(tuple.Key, tuple.Value));
                from.Inventory.RemoveMaterial(tuple.Key, transfered[tuple.Key]);
            }

            return transfered;
        }
    }
}
