using BaseBuilder.Engine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class RandomUtils
    {
        /// <summary>
        /// Gets a "random" object which is safe to create over the network, but is not safe to send
        /// over the network.
        /// </summary>
        /// <param name="gameState">The game stae</param>
        /// <returns>The net safe random</returns>
        public static Random GetNetSafeRandom(SharedGameState gameState, int consistentThing)
        {
            return new Random(gameState.GameTimeMS + consistentThing);
        }
    }
}
