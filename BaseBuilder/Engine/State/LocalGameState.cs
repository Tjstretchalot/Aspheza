using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.State
{
    /// <summary>
    /// This describes the portion of the game that is only relevant
    /// to this client. Elements in the local game state are not sent
    /// across the network and do not need to go through the network
    /// to be edited.
    /// </summary>
    public class LocalGameState
    {
        /// <summary>
        /// Where the camera is located.
        /// </summary>
        public Camera Camera;

        public LocalGameState(Camera camera)
        {
            Camera = camera;
        }
    }
}
