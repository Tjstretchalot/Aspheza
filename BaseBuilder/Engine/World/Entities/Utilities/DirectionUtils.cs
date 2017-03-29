using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class DirectionUtils
    {
        /// <summary>
        /// Determines the direction that entity should face to face towards thing
        /// </summary>
        /// <param name="gameState">Gamestate</param>
        /// <param name="entity">Entity</param>
        /// <param name="thing">Thing</param>
        /// <returns>Direction for entity to face thing</returns>
        public static Direction GetDirectionToFace(SharedGameState gameState, MobileEntity entity, Thing thing)
        {
            if (thing.PreferredAdjacentPoints != null)
            {
                var entityLocationAsOffset = (PointI2D)(entity.Position - thing.Position);

                var preferredTuple = thing.PreferredAdjacentPoints.Find((tup) => tup.Item1 == entityLocationAsOffset);
                if (preferredTuple != null)
                    return preferredTuple.Item2;
            }

            if (entity.CollisionMesh.Left + entity.Position.X >= thing.CollisionMesh.Right + thing.Position.X)
            {
                return Direction.Left;
            }
            else if (entity.CollisionMesh.Right + entity.Position.X <= thing.CollisionMesh.Left + thing.Position.X)
            {
                return Direction.Right;
            }
            else if (entity.CollisionMesh.Top + entity.Position.Y >= thing.CollisionMesh.Bottom + thing.Position.Y)
            {
                return Direction.Up;
            }
            else
            {
                return Direction.Down;
            }
            
        }
        
        /// <summary>
        /// Gets the direction that should be faced to face towards the specified offset
        /// </summary>
        /// <param name="dx">Delta x</param>
        /// <param name="dy">Delta y</param>
        /// <returns>Direction to face</returns>
        public static Direction GetDirectionFromOffset(double dx, double dy)
        {
            if (dx < 0)
                return Direction.Left;
            else if (dx > 0)
                return Direction.Right;
            else if (dy < 0)
                return Direction.Up;
            else if (dy > 0)
                return Direction.Down;

            throw new ArgumentException($"Expected offset but got {dx}, {dy}");
        }


        /// <summary>
        /// Sets up the mobile entity to face the target
        /// </summary>
        /// <param name="gameState">Game state</param>
        /// <param name="entity">Mobile entity</param>
        /// <param name="target">Target</param>
        public static void Face(SharedGameState gameState, MobileEntity entity, Entity target)
        {
            var direction = GetDirectionToFace(gameState, entity, target);

            entity.AnimationRenderer.StartAnimation(entity.AnimationRenderer.CurrentAnimationType, direction);
        }
    }
}
