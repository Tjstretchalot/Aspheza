using System;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Describes a "one-dimensional line". All lines are one dimensional
    /// in that they have no width, but this specifically refers to a line
    /// that is only on one axis.
    /// </summary>
    /// <remarks>
    /// If you project a normal line onto an axis you get a new line. However,
    /// that line doesn't have the same level of information. The resulting line
    /// can be described by it's distance from the origin along the axis. 
    /// 
    /// I.e. if a line is projected onto an axis &lt;1, 1&gt; resulting in &lt;-0.5, -0.5&gt;
    /// to &lt;0.5, 0.5&gt; this could also be referred to as simply the line along the axis
    /// &lt;1, 1&gt; starting at -(Math.Sqrt(1/2 * 1/2 + 1/2 * 1/2)) to Math.Sqrt(1/2 * 1/2 + 1/2 * 1/2)
    /// which is very easily compared with other lines projected onto that axis.
    /// 
    /// Note you can only compare these lines if they are aligned to the same axis.
    /// </remarks>
    public class OneDimensionalLine
    {
        /// <summary>
        /// The axis that this line is along.
        /// </summary>
        public VectorD2D Axis;

        /// <summary>
        /// The distance from the origin this line starts, along
        /// the axis that created it.
        /// </summary>
        public double Start;

        /// <summary>
        /// The distance from the origin this line ends, along the
        /// axis that created it.
        /// </summary>
        public double End;

        protected double? _Min;

        /// <summary>
        /// The smaller of start or end.
        /// </summary>
        public double Min
        {
            get
            {
                if (!_Min.HasValue)
                    _Min = Math.Min(Start, End);

                return _Min.Value;
            }
        }

        protected double? _Max;

        /// <summary>
        /// The greater of start or end
        /// </summary>
        public double Max
        {
            get
            {
                if (!_Max.HasValue)
                    _Max = Math.Max(Start, End);

                return _Max.Value;
            }
        }

        protected double? _Length;

        public double Length
        {
            get
            {
                if (!_Length.HasValue)
                {
                    _Length = Math.Abs(End - Start);
                }

                return _Length.Value;
            }
        }

        /// <summary>
        /// True if this line is actually just a point, false otherwise
        /// </summary>
        public bool IsPoint
        {
            get
            {
                return Start == End;
            }
        }

        /// <summary>
        /// Initializes the one dimensional line from start to end. 
        /// </summary>
        /// <param name="axis">The axis this line is on</param>
        /// <param name="start">The start of the line</param>
        /// <param name="end">The end of the line</param>
        /// <exception cref="ArgumentNullException">If axis is null</exception>
        public OneDimensionalLine(VectorD2D axis, double start, double end)
        {
            if (axis == null)
                throw new ArgumentNullException(nameof(axis));

            Axis = axis;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Determines if this line intersects the other line. 
        /// </summary>
        /// <param name="other">The line to compare with</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True if the lines intersect, false otherwise</returns>
        /// <exception cref="InvalidProgramException">If this line and other are not on the same axis</exception>
        public bool Intersects(OneDimensionalLine other, bool strict = false)
        {
            if (EpsilonGreaterThan(Min, other.Max) || (strict && Min == other.Max))
                return false;
            if (EpsilonGreaterThan(other.Min, Max) || (strict && other.Min == Max))
                return false;

            return true;
        }

        /// <summary>
        /// Determines if this line contains the point.
        /// </summary>
        /// <param name="point">The point to compare with</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns></returns>
        public bool ContainPoint(double point, bool strict = false)
        {
            
            if (EpsilonGreaterThan(point, Max) || (strict && Max == point))
                return false;
            if (EpsilonLessThan(point, Min) || (strict && Min == point))
                return false;

            return true;

        }

        /// <summary>
        /// Returns the largest line that both this line and the other line 
        /// include, if there is strict intersection between the lines. The
        /// line will be in the direction that this line would need to move
        /// in order for no collision to occur.
        /// 
        /// If the lines do not strictly intersect this returns null.
        /// </summary>
        /// <param name="other">The line to compare with</param>
        /// <returns>The largest line both this line and other include or null if no strict intersection.</returns>
        /// <exception cref="InvalidProgramException">If this line and other are not on the same axis</exception>
        /// <example>
        /// Case 1, partial intersection:
        /// Line1 = [Start = 2, End = 3]
        /// Line2 = [Start = 2.5, End = 4]
        /// 
        /// Line1.IntersectionLine(Line2) = [Start = 3, End = 2.5]
        /// Line2.IntersectionLine(Line1) = [Start = 2.5, End = 3]
        /// 
        /// Case 2, complete intersection, same size:
        /// Line1 = [Start = 4, End = 5]
        /// Line2 = [Start = 4, End = 5]
        /// 
        /// Line1.IntersectionLine(Line2) = [Start = 4, End = 5]
        /// Line2.IntersectionLine(Line1) = [Start = 4, End = 5]
        /// 
        /// Case 3, complete intersection, dissimiliar size
        /// Line1 = [Start = 4, End = 5]
        /// Line2 = [Start = 3, End = 5]
        /// 
        /// Line1.IntersectionLine(Line2) = [Start = 4, End = 5]
        /// Line2.IntersectionLine(Line1) = [Start = 5, End = 4]
        /// </example>
        public OneDimensionalLine IntersectionLine(OneDimensionalLine other)
        {
            if (EpsilonGreaterThanOrEqual(Min, other.Max) || EpsilonLessThanOrEqual(Max, other.Min))
                return null;

            var resultStart = Min;
            var resultEnd = other.Max;
            var resultLen = Math.Abs(resultEnd - resultStart);

            var opt2Start = Max;
            var opt2End = other.Min;
            var opt2Len = Math.Abs(opt2End - opt2Start);

            if(opt2Len < resultLen)
            {
                resultStart = opt2Start;
                resultEnd = opt2End;
                resultLen = opt2Len;
            }
            
            return new OneDimensionalLine(Axis, resultStart, resultEnd); 
        }

        /// <summary>
        /// Converts this one dimensional line representation to the standard
        /// finite line representation.
        /// </summary>
        /// <returns>Finite line representation.</returns>
        /// <exception cref="InvalidOperationException">If IsPoint is true</exception>
        public FiniteLineD2D AsFiniteLineD2D()
        {
            if (IsPoint)
                throw new InvalidOperationException("This line is actually a point - you should have checked using IsPoint. The correct method is AsPointD2D");
            
            var start = new PointD2D(Start * Axis.CosTheta, Start * Axis.SinTheta);
            var end = new PointD2D(End * Axis.CosTheta, End * Axis.SinTheta);

            return new FiniteLineD2D(start, end);
        }

        /// <summary>
        /// Converts this instance into a point. 
        /// </summary>
        /// <returns>The point that this line represents</returns>
        /// <exception cref="InvalidOperationException">If IsPoint is false</exception>
        public PointD2D AsPointD2D()
        {
            if (!IsPoint)
                throw new InvalidOperationException("This line is not a point - you should have checked using IsPoint. The correct method is AsVectorD2D");

            var cosTheta = Math.Cos(Axis.Theta);
            var sinTheta = Math.Sin(Axis.Theta);

            return new PointD2D(Start * cosTheta, Start * sinTheta);
        }

        public override string ToString()
        {
            return $"1DLine [{Start} to {End}]";
        }
    }
}
