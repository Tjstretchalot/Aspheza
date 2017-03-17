using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Context
{
    /// <summary>
    /// Everything you need to update stuff.
    /// </summary>
    public struct UpdateContext
    {
        public SharedGameState SharedState;
        public LocalGameState LocalState;
        public int ElapsedMS;
    }
}
