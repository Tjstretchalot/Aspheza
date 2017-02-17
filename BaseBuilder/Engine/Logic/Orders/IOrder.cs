using BaseBuilder.Engine.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Orders
{
    /// <summary>
    /// An order is something a player does that modifies the game. Orders are 
    /// only allowed to happen at certain periods of time and are assumed
    /// to happen simultaneously. In other words, the game is simulated in
    /// lockstep.
    /// </summary>
    /// <remarks>
    /// An order acts like a game packet but is not used like one. Rather than 
    /// being recieved and handled directly in the networking loop, orders are
    /// recieved and handled inside other packets (using the NetContext).
    /// 
    /// See SyncPacket for an example.
    /// </remarks>
    public interface IOrder : IGamePacket
    {
    }
}
