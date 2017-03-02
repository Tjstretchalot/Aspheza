using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class DirectionUtils
    {
        /// <summary>
        /// Sets up the specified entity to face towards the thing
        /// </summary>
        /// <param name="gameState">The game staet</param>
        /// <param name="entity">The entity to turn</param>
        /// <param name="thing">The thing to face</param>
        public static void Face(SharedGameState gameState, MobileEntity entity, Thing thing)
        {
            if (entity.CollisionMesh.Left + entity.Position.X >= thing.CollisionMesh.Right + thing.Position.X)
            {
                entity.OnMove(gameState, 0, -1, 0);
            }
            else if (entity.CollisionMesh.Right + entity.Position.X <= thing.CollisionMesh.Left + thing.Position.X)
            {
                entity.OnMove(gameState, 0, 1, 0);
            }
            else if (entity.CollisionMesh.Top + entity.Position.Y >= thing.CollisionMesh.Bottom + thing.Position.Y)
            {
                entity.OnMove(gameState, 0, 0, -1);
            }
            else
            {
                entity.OnMove(gameState, 0, 0, 1);
            }
            entity.OnStop(gameState);
        }
    }
}
