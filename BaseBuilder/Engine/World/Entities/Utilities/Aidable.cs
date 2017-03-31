using BaseBuilder.Engine.World.Entities.MobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    /// <summary>
    /// Something is aidable if a worker can interact with it to do work
    /// </summary>
    public interface Aidable : Thing
    {
        /// <summary>
        /// If this aidable requires aid right now
        /// </summary>
        bool NeedAid { get; }

        /// <summary>
        /// How much aid time is required for the next interesting 
        /// thing to happen.
        /// </summary>
        int AidTimeToNextMS { get; }
        
        /// <summary>
        /// Can be called when a specific aider aids this aidable
        /// over aidTimeMS milliseconds
        /// </summary>
        /// <param name="aider">The aider</param>
        /// <param name="aidTime">How much time was donated</param>
        void Aid(MobileEntity aider, int aidTimeMS);
    }
}
