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
    /// Describes one node of a path
    /// </summary>
    public class AStarNode
    {
        internal PointI2D Location;
        internal AStarNode Next;

        public AStarNode(NetIncomingMessage message)
        {
            Location = new PointI2D(message);
            bool next = message.ReadBoolean();

            if(next)
            {
                Next = new AStarNode(message);
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            Location.Write(message);
            if (Next == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                Next.Write(message);
            }
        }

        public AStarNode()
        {
        }
    }
}
