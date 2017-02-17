using BaseBuilder.Engine.Logic.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Players
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

        /// <summary>
        /// The orders that the player has issued that have not been resolved yet. This should
        /// only be modified by the network; orders will propagate from the local game state to
        /// the player (which is part of the shared game state) via the network.
        /// </summary>
        public List<IOrder> CurrentOrders;

        /// <summary>
        /// If this player has recieved orders since the last SyncStart packet
        /// </summary>
        public bool OrdersRecieved;

        public Player(int id, string name)
        {
            Name = name;
            ID = id;

            CurrentOrders = new List<IOrder>();
        }
    }
}
