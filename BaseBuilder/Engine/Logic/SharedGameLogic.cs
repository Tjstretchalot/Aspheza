using BaseBuilder.Engine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic
{
    /// <summary>
    /// The game logic will move from one game state to another game state consistently.
    /// 
    /// If given the same SharedGameState and delta, the GameLogic should always create
    /// the same resulting game state.
    /// </summary>
    public class SharedGameLogic
    {
        /// <summary>
        /// Simulates the passing of time. This is, in essence, the "Update()" loop of
        /// the game.
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="timeMS"></param>
        public void SimulateTimePassing(SharedGameState gameState, int timeMS)
        { }
    }
}
