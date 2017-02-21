using BaseBuilder.Engine.Math2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Pathfinders
{
    internal class WorkingAStarNode
    {
        public WorkingAStarNode Parent;

        public PointI2D Location;

        /// <summary>
        /// higher scores are worse
        /// </summary>
        public double HeurScoreToDestination;
        public double ScoreFromStartToHere;

        public double CombinedScore
        {
            get
            {
                return HeurScoreToDestination + ScoreFromStartToHere;
            }
        }

        public WorkingAStarNode(WorkingAStarNode parent, PointI2D loc, double heurScoreToDest, double scoreFromStartToHere)
        {
            Parent = parent;
            Location = loc;
            HeurScoreToDestination = heurScoreToDest;
            ScoreFromStartToHere = scoreFromStartToHere;
        }
    }
}
