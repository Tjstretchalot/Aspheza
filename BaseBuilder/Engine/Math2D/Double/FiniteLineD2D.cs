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
                    if (End.X - Start.X == 0)
                    {
                        if (Start.Y > End.Y)
                        {
                            _Slope = double.NegativeInfinity;
                            
                        } else if (Start.Y < End.Y)
                        {
                            _Slope = double.PositiveInfinity;
                        }
                    }else
                    {
                        _Slope = (End.Y - Start.Y) / (End.X - Start.X);
                    }
                }

                return _Slope.Value;
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
                if (double.IsInfinity(Slope))
                {
                    return null;
                }
                else if (!_YIntercept.HasValue)
                {
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
            if (Slope == other.Slope)
            {
                return true;
            }
            else if (Slope == -other.Slope)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            if (Slope == other.Slope)
            {
                return true;
            }
            else if(Slope == -other.Slope)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            if (Slope == -other.Slope)
            {
                return true;
            }
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
            if (Slope == -other.Slope)
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Determines if this line contains any points with the specified x.
        /// </summary>
        /// <param name="x">The x</param>
        /// <returns>True if this line contains any points with specified x, false otherwise</returns>
        public bool ContainsX(double x)
        {
            if (Start.X > x && End.X < x)
            {
                return true;
            }
            else if (Start.X < x && End.X > x)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if this line contains any points with the specified y.
        /// </summary>
        /// <param name="y">The y</param>
        /// <returns>True if this line contains any points with specified y, false otherwise</returns>
        public bool ContainsY(double y)
        {
            if (Start.Y > y && End.Y < y)
            {
                return true;
            }
            else if (Start.Y < y && End.Y > y)
            {
                return true;
            }
            return false;
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
            if (double.IsInfinity(Slope))
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
            if (double.IsInfinity(Slope))
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
            var tmp = YAt(point.X);
            if (tmp == null)
            {
                if (point.X == Start.X)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (point.Y == tmp)
            {
                return true;
            }
            return false;
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
            double dx = End.X - Start.X;
            double dy = End.Y - Start.Y;
            double otherdx = other.End.X - other.Start.X;
            double otherdy = other.End.Y - other.Start.Y;
            double rxs = (otherdy * dx) - (otherdx * dy);

            if (Slope == other.Slope || Slope == -other.Slope)
            {

                if (rxs == 0)
                {
                    double qpr = ((Start.X - other.Start.X) * dy) - ((Start.Y - other.Start.Y) * dx);

                    if (qpr == 0)
                    {
                        // collinear
                        double TmpT0 = (((other.Start.X - Start.X) * dx) - ((other.Start.Y - Start.Y) * dy) / LengthSquared);
                        double TmpT1 = (((other.End.X - Start.X) * dx) - ((other.End.Y - Start.Y) * dy) / LengthSquared);

                        if (TmpT0 >= 0 && TmpT0 <= 1)
                        {
                            return true;
                        }
                        else if (TmpT1 >= 0 && TmpT1 <= 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            double TmpT = (((other.Start.X - Start.X) * otherdy) - ((other.Start.Y - Start.Y) * otherdx) / rxs);
            double TmpU = (((Start.X - other.Start.X) * dy) - ((Start.Y - other.Start.Y) * dx) / rxs);

            if (TmpT >= 0 && TmpT <= 0 && TmpU >= 0 && TmpT <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        /// <param name="unitAxis">The axis to project onto</param>
        /// <param name="shift">Optional shift of this line, null for no shift</param>
        /// <returns>The line created when projecting this line onto the specified axis</returns>
        /// <exception cref="ArgumentNullException">If axis is null</exception>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D unitAxis, PointD2D shift = null)
        {
            var start = unitAxis.DeltaX * Start.X + unitAxis.DeltaY * Start.Y;
            var end = unitAxis.DeltaX * End.X + unitAxis.DeltaY * End.Y;

            return new OneDimensionalLine(unitAxis, start, end);
        }

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
