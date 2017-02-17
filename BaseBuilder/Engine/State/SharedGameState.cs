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

        /// <summary>
        /// 
        /// </summary>
        public int EntityIDCounter;

        public int GetEnitityID()
        {
            return EntityIDCounter++;
        }

        public SharedGameState(TileWorld world, List<Player> players, int gameTimeMS)
        {
            World = world;
            Players = players;
            GameTimeMS = gameTimeMS;
        }

        /// <summary>
        /// Returns the player with the specified id, if there is one. Otherwise
        /// returns null.
        /// </summary>
        /// <param name="id">ID to search for</param>
        /// <returns>The player with that id</returns>
        public Player GetPlayerByID(int id)
        {
            return Players.Find((p) => p.ID == id);
        }
    }
}
