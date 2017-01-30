using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Describes an infinite line with two points that it goes
    /// through.
    /// </summary>
    public class InfiniteLineD2D 
    {
        /// <summary>
        /// One point that this line touches.
        /// </summary>
        public PointD2D Point1 { get; protected set; }

        /// <summary>
        /// Another point that this line touches.
        /// </summary>
        public PointD2D Point2 { get; protected set; }
        
        protected double? _Slope;

        /// <summary>
        /// The slope of the line (rise over run)
        /// </summary>
        public double Slope
        {
            get
            {
                if(!_Slope.HasValue)
                {
                    _Slope = (Point2.Y - Point1.Y) / (Point2.X - Point1.Y);
                }

                return _Slope.Value;
            }
        }

        protected double? _YIntercept;

        /// <summary>
        /// Calculates the y-intercept of the line, if the slope is finite.
        /// </summary>
        public double? YIntercept
        {
            get
            {
                return 0;
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
                    _Normal = new VectorD2D(-(Point2.Y - Point1.Y), (Point2.X - Point1.X));
                }

                return _Normal; 
            }
        }

        /// <summary>
        /// Creates a new infinite line that goes through both
        /// p1 and p2.
        /// </summary>
        /// <param name="p1">A point on the line.</param>
        /// <param name="p2">A different point on the line.</param>
        /// <exception cref="InvalidProgramException">If the points are equal.</exception>
        /// <exception cref="ArgumentNullException">If either point is null.</exception>
        public InfiniteLineD2D(PointD2D p1, PointD2D p2)
        {
            if (p1 == p2)
                throw new InvalidProgramException($"A line requires two unique points, but the two points given are identical as {p1}");
            if (p1 == null)
                throw new ArgumentNullException($"Point1 is null (Point1={Point1}, Point2={Point2})");
            if (p2 == null)
                throw new ArgumentNullException($"Point2 is null (Point1={Point1}, Point2={Point2})");

            Point1 = p1;
            Point2 = p2;
        }

        /// <summary>
        /// Returns if this line is parallel to the other line. Two
        /// lines that are going in opposite directions ARE considered
        /// parallel. That is to say, the line going from (0, 0) to (0, 1)
        /// is parallel with the line going from (1, 1) to (1, 0)
        /// </summary>
        /// <param name="other">Line to compare with.</param>
        /// <returns>If this line is parallel with the other line.</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool IsParallel(InfiniteLineD2D other)
        {
            return false;
        }

        /// <summary>
        /// Returns if this line is strictly antiparallel with the other
        /// line. A line is only anti-parallel if it is ALSO parallel
        /// with the other line, but is "going in the other direction".
        /// </summary>
        /// <param name="other">Line to compare with.</param>
        /// <returns>If this line is anti-parallel with the other line</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool IsAntiParallel(InfiniteLineD2D other)
        {
            return false; // TODO
        }

        /// <summary>
        /// Determines if the specified point is along this line.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>True if the point is along the line, false otherwise</returns>
        public bool Intersects(PointD2D point)
        {
            return false; // TODO
        }

        /// <summary>
        /// Determines if this infinite line intersects the specified other infinite line.
        /// </summary>
        /// <param name="other">The infinite line to compare with.</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True if this line intersects the other, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(InfiniteLineD2D other, bool strict = false)
        {
            return false; // TODO
        }

        public override string ToString()
        {
            return $"Inf.Line [through {Point1} and {Point2}]";
        }
    }
}
