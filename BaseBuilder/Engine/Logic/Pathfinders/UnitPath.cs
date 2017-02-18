using BaseBuilder.Engine.Math2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Pathfinders
{
    /// <summary>
    /// Describes a path for an element with a particular collision mesh going
    /// from one location to another. The majority of the time the unit path will
    /// be created once and not reused.
    /// </summary>
    /// <remarks>
    /// Room is left open to optimize this 
    /// </remarks>
    public struct UnitPath
    {
        internal AStarNode CurrentNode;

        internal UnitPath(AStarNode first)
        {
            CurrentNode = first;
        }

        /// <summary>
        /// The current point on the path that the unit should be going towards
        /// </summary>
        /// <returns></returns>
        public PointI2D GetCurrent()
        {
            return CurrentNode.Location;
        }

        /// <summary>
        /// Attempts to get the next target for the result from GetCurrent(). Returns
        /// true if a new point was found, returns false if the path is finished.
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {
            CurrentNode = CurrentNode.Next;

            return CurrentNode != null;
        }
    }
}
