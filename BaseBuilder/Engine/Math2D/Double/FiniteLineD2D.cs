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

        protected VectorD2D _Axis;

        /// <summary>
        /// Gets the axis that this line is on. The length is arbitrary.
        /// </summary>
        public VectorD2D Axis
        {
            get
            {
                return null; // TODO
            }
        }

        /// <summary>
        /// Returns the midpoint of this line.
        /// </summary>
        public PointD2D Midpoint
        {
            get
            {
                return null; // TODO
            }
        }

        /// <summary>
        /// Creates a new finite line starting at start and ending at end.
        /// </summary>
        /// <param name="start">Start of the line.</param>
        /// <param name="end">End of the line.</param>
        /// <exception cref="ArgumentNullException">If start or end is null</exception>
        public FiniteLineD2D(PointD2D start, PointD2D end)
        {
            if(start == end)
                throw new InvalidProgramException($"A line requires two unique points, but the two points given are identical as {start}");
            if (start == null)
                throw new ArgumentNullException($"Start is null (start={start}, end={end})");
            if (end == null)
                throw new ArgumentNullException($"End is null (start={start}, end={end})");

            Start = start;
            End = end;
        }

        /// <summary>
        /// Returns the line created by shifting this line the specified amount
        /// </summary>
        /// <param name="dx">The shift in x</param>
        /// <param name="dy">The shift in y</param>
        /// <returns>A new line created by shifting this line the specified amount</returns>
        public FiniteLineD2D Shift(double dx, double dy)
        {
            return null; // TODO
        }

        /// <summary>
        /// Returns the line created by stretching this line by the specified
        /// scalar without moving the midpoint.
        /// </summary>
        /// <param name="scalar">Multiplier that the resulting lines length is compared to this length</param>
        /// <returns>A new line created by stretching this line by the specified scalar</returns>
        /// <exception cref="InvalidProgramException">If scalar is 0</exception>
        public FiniteLineD2D Stretch(double scalar)
        {
            return null; // TODO
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
        public bool IsParallel(FiniteLineD2D other)
        {
            return false; // TODO
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
        public bool IsAntiParallel(FiniteLineD2D other)
        {
            return false; // TODO
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
        /// Determines if this finite line intersects the specified other finite
        /// line.
        /// </summary>
        /// <param name="other">The line to compare with.</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True on intersection, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(FiniteLineD2D other, bool strict = false)
        {
            return false; // TODO
        }

        /// <summary>
        /// Determines the longest line that both this line and other both include if
        /// the lines strictly intersect. Otherwise returns null.
        /// </summary>
        /// <remarks>
        /// Since two lines can only have an intersection line if they are aligned to 
        /// the same axis, a OneDimensionalLine can be returned. If the actual location
        /// of the intersection line is important, a OneDimensionalLine can be
        /// converted to a FiniteLine using OneDimensionalLine#AsFiniteLineD2D.
        /// </remarks>
        /// <param name="other">The line to compare with</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public OneDimensionalLine IntersectionLine(FiniteLineD2D other)
        {
            return null; // TODO
        }

        /// <summary>
        /// Determines if this finite line intersects the specified other infinite line.
        /// </summary>
        /// <param name="other">The infinite line to compare with.</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True on intersection, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(InfiniteLineD2D other, bool strict = false)
        {
            return false; // TODO
        }

        /// <summary>
        /// Projects this line onto the specified axis.
        /// </summary>
        /// <param name="axis">The axis to project onto</param>
        /// <returns>The line created when projecting this line onto the specified axis</returns>
        /// <exception cref="ArgumentNullException">If axis is null</exception>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D axis)
        {
            return null; // TODO
        }

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
