using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math.Double
{
    /// <summary>
    /// Describes a line, with a start and an end. Should be treated
    /// as immutable.
    /// </summary>
    public class FiniteLineD2D
    {
        /// <summary>
        /// Where this line starts. 
        /// </summary>
        public PointD2D Start { get; }

        /// <summary>
        /// Where this line ends.
        /// </summary>
        public PointD2D End { get; }

        protected double _Slope;

        /// <summary>
        /// The slope of the line (rise over run)
        /// </summary>
        public double Slope
        {
            get
            {
                return 0; // TODO
            }
        }

        protected double _LengthSquared;

        /// <summary>
        /// The length of the line (squared).
        /// </summary>
        public double LengthSquared
        {
            get
            {
                return 0; // TODO
            }
        }

        protected double _Length;

        /// <summary>
        /// The length of the line.
        /// </summary>
        public double Length
        {
            get
            {
                return 0; // TODO
            }
        }

        protected InfiniteLineD2D _Normal;

        /// <summary>
        /// Returns a line that runs perpendicular/normal/orthogonal
        /// to this line.
        /// </summary>
        public InfiniteLineD2D Normal
        {
            get
            {
                return null; // TODO
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public FiniteLineD2D(PointD2D start, PointD2D end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
