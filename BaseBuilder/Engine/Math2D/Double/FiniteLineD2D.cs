using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
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
        public PointD2D Start { get; protected set; }

        /// <summary>
        /// Where this line ends.
        /// </summary>
        public PointD2D End { get; protected set; }

        protected double? _Slope;

        /// <summary>
        /// The slope of the line (rise over run)
        /// </summary>
        public double Slope
        {
            get
            {
                if (!_Slope.HasValue)
                {
                    _Slope = (End.Y - Start.Y) / (End.X - Start.Y);
                }

                return _Slope.Value;
            }
        }

        protected double? _LengthSquared;

        /// <summary>
        /// The length of the line (squared).
        /// </summary>
        public double LengthSquared
        {
            get
            {
                if (!_LengthSquared.HasValue)
                {
                    _LengthSquared = (((End.X - Start.X) * (End.X - Start.X)) + ((End.Y - Start.Y) * (End.Y - Start.Y)));
                }

                return _LengthSquared.Value;
            }
        }

        protected double? _Length;

        /// <summary>
        /// The length of the line.
        /// </summary>
        public double Length
        {
            get
            {
                if (!_Length.HasValue)
                {
                    _Length = Math.Sqrt(LengthSquared);
                }

                return _Length.Value;
            }
        }

        protected VectorD2D _Normal;

        /// <summary>
        /// Returns a vector that is perpendicular/normal/orthogonal
        /// to this line. The length of the resulting vector is 
        /// arbitrary.
        /// </summary>
        public VectorD2D Normal
        {
            get
            {
                if (_Normal == null)
                {
                    _Normal = new VectorD2D(-(End.Y - Start.Y), (End.X - Start.X));
                }

                return _Normal;
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

        /// <summary>
        /// Determines if this finite line intersects the specified other finite
        /// line.
        /// </summary>
        /// <param name="other">The line to compare with.</param>
        /// <param name="strict">True if touching constitutes intersection, false otherwise.</param>
        /// <returns></returns>
        public bool Intersects(FiniteLineD2D other, bool strict = false)
        {
            return false; // TODO
        }

        /// <summary>
        /// Determines if this finite line intersects the specified other infinite line.
        /// </summary>
        /// <param name="other">The infinite line to compare with.</param>
        /// <param name="strict">True if touching constitutes intersection, false otherwise.</param>
        /// <returns></returns>
        public bool Intersects(InfiniteLineD2D other, bool strict = false)
        {
            return false; // TODO
        }

        

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
