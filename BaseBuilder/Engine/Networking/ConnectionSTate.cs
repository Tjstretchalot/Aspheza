using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Networking
{
    /// <summary>
    /// Describes the state of a game connection
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// This game state implies no activity over the network is happening. This
        /// occurs to allow some time for the player to make actions.
        /// 
        /// Orders should be placed in the current order queue during this process.
        /// 
        /// During the waiting state players may connect and be synced.
        /// </summary>
        Waiting,

        /// <summary>
        /// During the synchronization process every client will send his local orders
        /// to the server, and the server will distribute all of the orders to each client.
        /// 
        /// When switching from waiting to syncing, each player will, upon recieving the SyncStart
        /// packet, will cache the current orders the player has issued such that no more orders
        /// will go into this sync, then send those orders to the server.
        /// 
        /// Players which connect during this state will wait until the next Waiting state
        /// </summary>
        Syncing,

        /// <summary>
        /// Once all players agree on what orders to use for the tick, each player will simulate 
        /// the game for an agreed period of time. Once simulating is complete, the connection
        /// will return to the waiting time period.
        /// 
        /// Players which connect during this state will wait until the next Waiting state
        /// </summary>
        Simulating,
    }
}
