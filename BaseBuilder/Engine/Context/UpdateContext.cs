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
        public TileWorld World;
        public int ElapsedMS;
    }
}
