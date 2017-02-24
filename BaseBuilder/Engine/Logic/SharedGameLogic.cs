using BaseBuilder.Engine.Logic.Players;
using BaseBuilder.Engine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.WorldObject.Entities;

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
        protected ReflectiveOrderHandler OrderHandler;

        public SharedGameLogic()
        {
            OrderHandler = new ReflectiveOrderHandler(this);
        }

        /// <summary>
        /// Simulates the passing of time. This is, in essence, the "Update()" loop of
        /// the game.
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="timeMS"></param>
        public void SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            // DO NOT CLEAR ORDERS - DO NOT CLEAR ORDERS - DO NOT CLEAR ORDERS
            foreach(var player in gameState.Players)
            {
                HandlePlayerOrders(gameState, timeMS, player);
            }

            gameState.World.SimulateTimePassing(gameState, timeMS);

            gameState.GameTimeMS += timeMS;

            for(int i = gameState.RecentMessages.Count - 1; i >= 0; i--)
            {
                var msgTuple = gameState.RecentMessages[i];
                var timestamp = msgTuple.Item2;

                var timeSinceIssued = gameState.GameTimeMS - timestamp;

                if(timeSinceIssued > 10000)
                {
                    gameState.RecentMessages.RemoveAt(i);
                }
            }
        }

        public void HandlePlayerOrders(SharedGameState gameState, int timeMS, Player player)
        {
            foreach(var order in player.CurrentOrders)
            {
                HandlePlayerOrders(gameState, timeMS, player, order);
            }
        }

        private void HandlePlayerOrders(SharedGameState gameState, int timeMS, Player player, IOrder order)
        {
            OrderHandler.BroadcastOrder(gameState, player, order);
        }

        [OrderHandler(typeof(MoveOrder))]
        public void OnMoveOrder(SharedGameState gameState, Player player, MoveOrder order)
        {
            MobileEntity entity = gameState.World.MobileEntities.Find((me) => me.ID == order.EntityID);
            if(entity == null)
            {
                throw new InvalidProgramException("move order on null entity? race condition?");
            }
            
            entity.QueueTask(new EntityMoveTask(entity, order.End));
        }

        [OrderHandler(typeof(CancelTasksOrder))]
        public void OnCancelTasksOrder(SharedGameState gameState, Player player, CancelTasksOrder order)
        {
            Entity entity = gameState.World.MobileEntities.Find((me) => me.ID == order.EntityID);
            if (entity == null)
                entity = gameState.World.ImmobileEntities.Find((me) => me.ID == order.EntityID);

            if (entity == null)
                throw new InvalidProgramException("cancel tasks order on null entity?");

            entity.ClearTasks();
        }

        [OrderHandler(typeof(IssueMessageOrder))]
        public void OnIssueMessageOrder(SharedGameState gameState, Player player, IssueMessageOrder order)
        {
            gameState.RecentMessages.Add(Tuple.Create(order.Message, gameState.GameTimeMS));
        }

        [OrderHandler(typeof(ChangeNameOrder))]
        public void OnChangeNameOrder(SharedGameState gameState, Player player, ChangeNameOrder order)
        {
            var playerToChangeName = gameState.GetPlayerByID(order.PlayerID);

            playerToChangeName.Name = order.NewName;
        }

        [OrderHandler(typeof(BuildOrder))]
        public void OnBuildOrder(SharedGameState gameState, Player player, BuildOrder order)
        {
            gameState.World.AddImmobileEntity(order.Entity);
        }
    }
}
