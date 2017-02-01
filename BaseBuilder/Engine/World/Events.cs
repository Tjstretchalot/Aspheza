using BaseBuilder.Engine.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World
{
    /// <summary>
    /// Handles a world event.
    /// </summary>
    /// <param name="context">The current update context</param>
    public delegate void WorldEventHandler(UpdateContext context);
}
