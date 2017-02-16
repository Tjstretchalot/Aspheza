using BaseBuilder.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.State
{
    /// <summary>
    /// This describes the game as is "consistent" across each client
    /// in the game (as close as you can get with latency). Anything
    /// that modifies parts of the shared game state must go through
    /// networking.
    /// </summary>
    public class SharedGameState
    {
        /// <summary>
        /// The world in which the players are in
        /// </summary>
        public TileWorld World;
    }
}
