using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors
{
    public interface ITransferRestrictor
    {
        void Write(NetOutgoingMessage message);
        void Restrict(SharedGameState sharedGameState, Container from, Entity to, Dictionary<Material, int> maxes);
        void Reset();
    }
}
