using BaseBuilder.Engine.Math2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Pathfinders
{
    /// <summary>
    /// Describes one node of a path
    /// </summary>
    public class AStarNode
    {
        internal PointI2D Location;
        internal AStarNode Next;
    }
}
