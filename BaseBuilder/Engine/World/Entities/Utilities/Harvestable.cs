using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    /// <summary>
    /// Describes something which can be harvested
    /// </summary>
    public interface Harvestable : Thing
    {
        /// <summary>
        /// Determines if this thing is ready to harvest
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <returns>if this is ready to harvest</returns>
        bool ReadyToHarvest(SharedGameState sharedGameState);

        /// <summary>
        /// The pretty name for what you can harvest out of this
        /// </summary>
        /// <returns>the pretty name for what you can harvest</returns>
        string GetHarvestNamePretty();

        /// <summary>
        /// Tries to harvest this harvestable into the specified container.
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="reciever">The recieving container</param>
        void TryHarvest(SharedGameState sharedGameState, Container reciever);
    }
}
