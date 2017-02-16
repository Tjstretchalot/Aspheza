using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Player
{
    /// <summary>
    /// Describes a person in the game and the orders that they have issued.
    /// </summary>
    /// <remarks>
    /// The game is in lockstep; the order queue will be sent over the network
    /// at specific intervals, then the game will simulate the passing of time
    /// with those speciifed orders, repeat.
    /// </remarks>
    public class Player
    {
        /// <summary>
        /// The name of the player, or username. How they are identified
        /// to other players
        /// </summary>
        public string Name;

        /// <summary>
        /// The unique int that identifies this player to the game.
        /// </summary>
        public int ID;

        
    }
}
