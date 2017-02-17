using BaseBuilder.Engine.Logic.Players;
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

        /// <summary>
        /// The players in the game
        /// </summary>
        public List<Player> Players;

        /// <summary>
        /// The current time in milliseconds since the game started
        /// </summary>
        public int GameTimeMS;

        public SharedGameState(TileWorld world, List<Player> players, int gameTimeMS)
        {
            World = world;
            Players = players;
            GameTimeMS = gameTimeMS;
        }
    }
}
