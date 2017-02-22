using BaseBuilder.Engine.Math2D;
using Lidgren.Network;
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
    public class UnitPath
    {
        private AStarNode CurrentNode;

        internal UnitPath(AStarNode first)
        {
            CurrentNode = first;
        }

        public UnitPath(NetIncomingMessage message)
        {
            bool current = message.ReadBoolean();
            if(current)
                CurrentNode = new AStarNode(message);
        }

        public void Write(NetOutgoingMessage message)
        {
            if (CurrentNode == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                CurrentNode.Write(message);
            }

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
