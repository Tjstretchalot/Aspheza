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
    public interface Order
    {
    }
}
