using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Pathfinders
{
    /// <summary>
    /// A* Pathfinding (modified for any-size collision meshes)
    /// </summary>
    public class EnhancedAStarPathfinder
    {
        /// <summary>
        /// Calculates a path from start to end for the specified entity in the specified
        /// world. The path will match the grid, it is up to the move order to interpolate 
        /// between-grid locations.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="collisionMesh"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public UnitPath CalculatePath(TileWorld world, Entity entity, PointI2D start, PointI2D end)
        {
            return new UnitPath(); // TODO
        }
    }
}
