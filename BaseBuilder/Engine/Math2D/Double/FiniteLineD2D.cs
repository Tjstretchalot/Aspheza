using System;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;
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
                    if (EpsilonEqual(End.X, Start.X))
                    {
                         _Slope = double.PositiveInfinity;
                    }else
                    {
                        _Slope = (End.Y - Start.Y) / (End.X - Start.X);
                    }
                }

                return _Slope.Value;
            }
        }

        protected bool? _Vertical;

        /// <summary>
        /// If this line is completely vertical
        /// </summary>
        public bool Vertical
        {
            get
            {
                if (!_Vertical.HasValue)
                    _Vertical = double.IsInfinity(Slope);

                return _Vertical.Value;
            }
        }

        protected bool? _Horizontal;

        /// <summary>
        /// If this line is completely flat
        /// </summary>
        public bool Horizontal
        {
            get
            {
                if (!_Horizontal.HasValue)
                    _Horizontal = EpsilonEqual(Slope, 0);

                return _Horizontal.Value;
            }
        }

        protected double? _YIntercept;

        /// <summary>
        /// The y-intercept of this line. This is calculated regardless of 
        /// if the line would actually intercept the y-axis (i.e., without 
        /// regard to length), but it still may not have a value if slope
        /// is infinite.
        /// </summary>
        public double? YIntercept
        {
            get
            {
                if (Vertical)
                {
                    return null;
                }
                else if (!_YIntercept.HasValue)
                {
                    // y = mx + b; b = y - mx
                    _YIntercept = Start.Y - (Start.X * Slope);
                }

                return _YIntercept;
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
        /// Gets the axis that this line is on. The length is the length
        /// of this line.
        /// </summary>
        public VectorD2D Axis
        {
            get
            {
                if(_Axis == null)
                   _Axis = (End - Start).AsVectorD2D();

                return _Axis;
            }
        }

        protected PointD2D _Midpoint;
        /// <summary>
        /// Returns the midpoint of this line.
        /// </summary>
        public PointD2D Midpoint
        {
            get
            {
                if (_Midpoint == null)
                {
                    _Midpoint = new PointD2D((Start.X + ((End.X - Start.X) / 2)), (Start.Y + ((End.Y - Start.Y) / 2)));
                }

                return _Midpoint;
            }
        }

        protected double _MinX;

        /// <summary>
        /// The smallest x value this line contains.
        /// </summary>
        public double MinX
        {
            get
            {
                if (Start.X > End.X)
                {
                    return End.X;
                }
                return Start.X;
            }
        }

        protected double _MaxX;

        /// <summary>
        /// The largest x value this line contains
        /// </summary>
        public double MaxX
        {
            get
            {
                if (Start.X < End.X)
                {
                    return End.X;
                }
                return Start.X;
            }
        }
        protected double _MinY;

        /// <summary>
        /// The smallest y value this line contains
        /// </summary>
        public double MinY
        {
            get
            {
                if (Start.Y > End.Y)
                {
                    return End.Y;
                }
                return Start.Y;
            }
        }

        protected double _MaxY;

        /// <summary>
        /// The largest y value this line contains
        /// </summary>
        public double MaxY
        {
            get
            {
                if (Start.Y < End.Y)
                {
                    return End.Y;
                }
                return Start.Y;
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
                throw new ArgumentNullException(nameof(start));
            if (end == null)
                throw new ArgumentNullException(nameof(end));

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
            return new FiniteLineD2D(Start.Shift(dx, dy), End.Shift(dx, dy));
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
            var result = Shift(-Midpoint.X, -Midpoint.Y);
            result.Start = result.Start * scalar;
            result.End = result.End * scalar;
            result = Shift(Midpoint.X, Midpoint.Y);

            return result;
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
            return EpsilonEqual(Slope, other.Slope);
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
            return Axis.UnitVector == -(other.Axis.UnitVector);
        }
        
        /// <summary>
        /// Determines if this line contains any points with the specified x.
        /// </summary>
        /// <param name="x">The x</param>
        /// <param name="strict">False if the edges are included, true otherwise</param>
        /// <returns>True if this line contains any points with specified x, false otherwise</returns>
        public bool ContainsX(double x, bool strict = false)
        {
            if(strict)
                return EpsilonGreaterThan(x, MinX) && EpsilonLessThan(x, MaxX);

            return EpsilonGreaterThanOrEqual(x, MinX) && EpsilonLessThanOrEqual(x, MaxX);
        }

        /// <summary>
        /// Determines if this line contains any points with the specified y.
        /// </summary>
        /// <param name="y">The y</param>
        /// <param name="strict">False if the edges are included, true otherwise</param>
        /// <returns>True if this line contains any points with specified y, false otherwise</returns>
        public bool ContainsY(double y, bool strict = false)
        {
            if (strict)
                return EpsilonGreaterThan(y, MinY) && EpsilonLessThan(y, MaxY);

            return EpsilonGreaterThanOrEqual(y, MinY) && EpsilonLessThanOrEqual(y, MaxY);
        }

        /// <summary>
        /// Determines what x position corresponds to the specified
        /// y position on this line. This function is without regard
        /// to the length of the line, however may still not have a
        /// value if the line is horizontal.
        /// </summary>
        /// <param name="y">The y to calculate x at</param>
        /// <returns>X at y</returns>
        public double? XAt(double y)
        {
            if (Vertical)
            {
                return null;
            }
            else
            {
                return ((y - YIntercept) / Slope);
            }
        }

        /// <summary>
        /// Determines what y-position corresponds to the specified
        /// x position on this line. This function is without regard
        /// to the length of the line, however may still not have a 
        /// value if the line is vertical.
        /// </summary>
        /// <param name="x">The x to calculate y at</param>
        /// <returns>Y at x</returns>
        public double? YAt(double x)
        {
            if (Horizontal)
            {
                return null;
            }
            else
            {
                return (Slope * x) + YIntercept;
            }
        }

        /// <summary>
        /// Determines if the specified point is along this line.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>True if the point is along the line, false otherwise</returns>
        public bool Intersects(PointD2D point)
        {
            if (!ContainsX(point.X))
            {
                return false;
            }
            var tmp = YAt(point.X);
            if (tmp == null)
            {
                return ContainsY(point.Y);
            }

            return EpsilonEqual(point.Y, tmp.Value);
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
            if (Vertical && other.Vertical)
            {
                if (!EpsilonEqual(Start.X, other.Start.X))
                    return false;
                if ((!strict && EpsilonGreaterThan(MinY, other.MaxY)) || (strict && EpsilonGreaterThanOrEqual(MinY, other.MaxY)))
                    return false;
                if ((!strict && EpsilonGreaterThan(other.MinY, MaxY)) || (strict && EpsilonGreaterThanOrEqual(other.MinY, MaxY)))
                    return false;
                return true;
            }

            if (Horizontal && other.Horizontal)
            {
                if (!EpsilonEqual(Start.Y, other.Start.Y))
                    return false;
                if ((strict && !EpsilonGreaterThan(MinX, other.MaxX)) || (strict && EpsilonGreaterThanOrEqual(MinX, other.MaxX)))
                    return false;
                if ((strict && !EpsilonGreaterThan(other.MinX, MaxX)) || (strict && EpsilonGreaterThanOrEqual(other.MinX, MaxX)))
                    return false;
                return true;
            }


            if (Vertical)
            {
                if (!other.ContainsX(Start.X, strict))
                    return false;

                if (other.Horizontal)
                    return ContainsY(other.Start.Y, strict);

                var yat = other.YAt(Start.X);
                return ContainsY(yat.Value, strict); // yat must have a value; their line is not horizontal
            }
            else if (other.Horizontal)
                return other.Intersects(this, strict);

            if (Horizontal)
            {
                if (!other.ContainsY(Start.Y, strict))
                    return false;

                if (other.Vertical)
                    return ContainsX(other.Start.X, strict);

                var xat = other.XAt(Start.Y);
                return ContainsX(xat.Value, strict); // xat must have a value; their line is not vertical
            }
            else if (other.Vertical)
                return other.Intersects(this, strict);

            if(IsParallel(other))
            {
                if (!EpsilonEqual(YIntercept.Value, other.YIntercept.Value))
                    return false;

                var usProjected = ProjectOntoAxis(Axis.UnitVector);
                var themProjected = other.ProjectOntoAxis(Axis.UnitVector);
                
                return usProjected.Intersects(themProjected, strict);
            }

            // Two standard non-parallel lines.

            // y = m1x + b1 = m2x + b2
            // m1x + b1 = m2x + b2
            // m1x - m2x = b2 - b1
            // 
            //     b2 - b1
            // x = -------
            //     m1 - m2   

            var x = (other.YIntercept.Value - YIntercept.Value) / (Slope - other.Slope);

            return ContainsX(x, strict) && other.ContainsX(x, strict);
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
        /// Projects this line onto the specified axis.
        /// </summary>
        /// <param name="unitAxis">The axis to project onto</param>
        /// <param name="shift">Optional shift of this line, null for no shift</param>
        /// <returns>The line created when projecting this line onto the specified axis</returns>
        /// <exception cref="ArgumentNullException">If axis is null</exception>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D unitAxis, PointD2D shift = null)
        {
            var start = unitAxis.DeltaX * (Start.X + (shift == null ? 0 : shift.X)) + unitAxis.DeltaY * (Start.Y + (shift == null ? 0 : shift.Y));
            var end = unitAxis.DeltaX * (End.X + (shift == null ? 0 : shift.X)) + unitAxis.DeltaY * (End.Y + (shift == null ? 0 : shift.Y));
            
            return new OneDimensionalLine(unitAxis, start, end);
        }

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
