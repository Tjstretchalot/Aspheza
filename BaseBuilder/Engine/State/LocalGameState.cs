using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Screens.GameScreens;
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

        /// <summary>
        /// The id of the local player
        /// </summary>
        public int LocalPlayerID;

        /// <summary>
        /// The id of the overseer entity
        /// </summary>
        public int OverseerEntityID;

        /// <summary>
        /// The orders that have been issued by the player but have not propagated
        /// to the shared game state yet
        /// </summary>
        public List<IOrder> Orders;

        /// <summary>
        /// The Entity that you are currently selecting
        /// </summary>
        public Entity SelectedEntity;

        /// <summary>
        /// The Entity that you are currently hovering on
        /// </summary>
        public Entity HoveredEntity;

        public bool CollisionDebug;

        /// <summary>
        /// The components on the screen
        /// </summary>
        public List<IMyGameComponent> Components;

        public LocalGameState(Camera camera, int localPlayerID)
        {
            Camera = camera;
            LocalPlayerID = localPlayerID;

            Orders = new List<IOrder>();
            CollisionDebug = false;
        }
    }
}
