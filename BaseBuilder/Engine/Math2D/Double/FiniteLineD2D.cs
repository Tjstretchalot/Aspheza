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
        /// Calculates the shortest vector from this line to the specified point. Does not work
        /// if this line intersects the point.
        /// </summary>
        /// <param name="other">The point</param>
        /// <param name="myPosition">My position</param>
        /// <param name="otherPosition">Other position</param>
        /// <returns>Shortest vector from this line to other, ties broken arbitrarily</returns>
        public VectorD2D MinVectorTo(PointD2D other, PointD2D myPosition, PointD2D otherPosition)
        {
            /*
             * Method:
             *   Project the point onto this axis -> One dimensional point on axis
             *   Project this line onto this axis -> One dimensional line on axis
             *   
             *   If point < Min, the shortest distance is from the Min to the point
             *   If point > Max, the shortest distance is from the Max to the point
             *   
             *   Project this line onto the normal of this line -> One dimensional point on axis
             *   Project the point onto the normal of this line -> One dimensional point on axis
             *   
             *   Return the distance between the two
             */

            var usProjOnAxis = ProjectOntoAxis(Axis.UnitVector, myPosition);
            var pointProjOnAxis = other.DotProduct(Axis.UnitVector.DeltaX, Axis.UnitVector.DeltaY, otherPosition.X, otherPosition.Y);

            var minPoint = (usProjOnAxis.Start == usProjOnAxis.Min) ? Start : End;
            var maxPoint = (usProjOnAxis.Start == usProjOnAxis.Max) ? Start : End;

            if(pointProjOnAxis < usProjOnAxis.Min)
            {
                // from us to them = them - us
                return new VectorD2D((other.X + otherPosition.X) - (minPoint.X + myPosition.X), (other.Y + otherPosition.Y) - (minPoint.Y + myPosition.Y));
            }

            if(pointProjOnAxis > usProjOnAxis.Max)
            {
                return new VectorD2D((other.X + otherPosition.X) - (maxPoint.X + myPosition.X), (other.Y + otherPosition.Y) - (maxPoint.Y + myPosition.Y));
            }

            // projecting this onto the normal axis is the same as projecting any point on this line to the normal axis
            var usProjOnNormal = Start.DotProduct(Normal.UnitVector.DeltaX, Normal.UnitVector.DeltaY, myPosition.X, myPosition.Y);
            var pointProjOnNormal = other.DotProduct(Normal.UnitVector.DeltaX, Normal.UnitVector.DeltaY, otherPosition.X, otherPosition.Y);

            // Slow version:
            //   var tmp = new OneDimensionalLine(Normal, usProjOnAxis, pointProjOnAxis).AsFiniteLineD2D();
            //   return (tmp.End - tmp.Start).AsVectorD2D()

            return new VectorD2D((pointProjOnNormal - usProjOnNormal) * Normal.CosTheta, (pointProjOnNormal - usProjOnNormal) * Normal.SinTheta);
        }

        /// <summary>
        /// Returns the shortest vector that connects the two lines. Does not work if the two lines intersect.
        /// </summary>
        /// <param name="other">The other line</param>
        /// <param name="myPosition">Where this line is located</param>
        /// <param name="otherPosition">Where the other line is located</param>
        /// <returns>The shortest vector between this line and the other line</returns>
        public VectorD2D MinVectorTo(FiniteLineD2D other, PointD2D myPosition, PointD2D otherPosition)
        {
            // Performance improvements are possible - a lot of stuff here is recalculated (particularly projections)
            // which can be reused for each of the 4 checks
            var shortest = MinVectorTo(other.Start, myPosition, otherPosition);

            var tmp = MinVectorTo(other.End, myPosition, otherPosition);
            if (tmp.MagnitudeSquared < shortest.MagnitudeSquared)
                shortest = tmp;

            tmp = other.MinVectorTo(Start, otherPosition, myPosition);
            if (tmp.MagnitudeSquared < shortest.MagnitudeSquared)
                shortest = tmp;

            tmp = other.MinVectorTo(End, otherPosition, myPosition);
            if (tmp.MagnitudeSquared < shortest.MagnitudeSquared)
                shortest = tmp;

            return shortest;
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
            if (Horizontal)
            {
                return null;
            }
            else
            {
                if (Vertical)
                    return Start.X; // this makes it act the same as YAt when slope is 0

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
            if (Vertical)
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
        /// <param name="shiftMe">The shift that should be applied to this line for intersection purposes. Null if origin</param>
        /// <param name="shiftOther">The shift that should be applied to the other line for intersection purposes. Null if origin</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True on intersection, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(FiniteLineD2D other, PointD2D shiftMe = null, PointD2D shiftOther = null, bool strict = false)
        {
            if (shiftMe != null || shiftOther != null)
            {
                var me = (shiftMe == null ? this : Shift(shiftMe.X, shiftMe.Y));
                var them = (shiftOther == null ? other : other.Shift(shiftOther.X, shiftOther.Y));

                return me.Intersects(them);
            }

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
                return other.Intersects(this, strict: strict);

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
                return other.Intersects(this, strict: strict);

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
            throw new NotImplementedException("This has not been implemented yet."); // TODO
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
            return ProjectLineOntoAxis(Start.X, Start.Y, End.X, End.Y, unitAxis, shift);
        }

        /// <summary>
        /// Convienence function that doesn't require many non-primitives.
        /// </summary>
        /// <param name="startX">Start x of the line.</param>
        /// <param name="startY">Start y of the line.</param>
        /// <param name="endX">End x of the line.</param>
        /// <param name="endY">End y of the line.</param>
        /// <param name="axis">The unit axis to project onto</param>
        /// <param name="shift">Optional shift of the line</param>
        /// <returns>The projection of the line from (startX, startY) to (endX, endY) on the axis &lt;axisDX, axisDY&gt;</returns>
        public static OneDimensionalLine ProjectLineOntoAxis(double startX, double startY, double endX, double endY, VectorD2D axis, PointD2D shift = null)
        {
            var start = axis.DeltaX * (startX + (shift == null ? 0 : shift.X)) + axis.DeltaY * (startY + (shift == null ? 0 : shift.Y));
            var end = axis.DeltaX * (endX + (shift == null ? 0 : shift.X)) + axis.DeltaY * (endY + (shift == null ? 0 : shift.Y));

            return new OneDimensionalLine(axis, start, end);
        }

        /// <summary>
        /// Returns the list of locations that this line intersects
        /// </summary>
        /// <returns>Tiles intersected by this line</returns>
        public HashSet<PointI2D> GetTilesIntersected(bool strict = false)
        {
            var points = new HashSet<PointI2D>();

            var currentPoint = new PointD2D(Start.X, Start.Y);

            if (strict && Horizontal && (currentPoint.Y == (int)currentPoint.Y))
                return points;

            if (strict && Vertical && (currentPoint.X == (int)currentPoint.X))
                return points;

            if (strict)
            {
                while (currentPoint.X == (int)currentPoint.X || currentPoint.Y == (int)currentPoint.Y)
                {
                    const double dist = 0.05;
                    currentPoint += Axis.UnitVector.Scale(dist).AsPointD2D();
                }
            }

            bool incrementingX = End.X > Start.X;
            bool incrementingY = End.Y > Start.Y;
            while (true)
            {
                int currentTileX;
                if (!strict || incrementingX || currentPoint.X != (int)currentPoint.X)
                    currentTileX = (int)Math.Floor(currentPoint.X);
                else
                    currentTileX = (int)currentPoint.X - 1;

                int currentTileY;
                if(!strict || incrementingY || currentPoint.Y != (int)currentPoint.Y)
                    currentTileY = (int)Math.Floor(currentPoint.Y);
                else
                    currentTileY = (int)currentPoint.Y - 1;

                var currentTile = new PointI2D(currentTileX, currentTileY);

                if(!strict)
                {
                    if (currentPoint.X == (int)currentPoint.X)
                    {
                        points.Add(new PointI2D(currentTile.X - 1, currentTile.Y));
                    }

                    if (currentPoint.Y == (int)currentPoint.Y)
                    {
                        points.Add(new PointI2D(currentTile.X, currentTile.Y - 1));
                    }

                    if (currentPoint.X == (int)currentPoint.X && currentPoint.Y == (int)currentPoint.Y)
                    {
                        points.Add(new PointI2D(currentTile.X - 1, currentTile.Y - 1));
                    }
                }

                points.Add(currentTile);

                double distSq = double.MaxValue;
                PointD2D nextPoint = null;
                if (!Horizontal)
                {
                    int nextY;
                    bool done;
                    if (incrementingY)
                    {
                        nextY = (int)Math.Floor(currentPoint.Y) + 1;
                        done = nextY > MaxY || (strict && nextY == MaxY);
                    }
                    else
                    {
                        nextY = (int)Math.Ceiling(currentPoint.Y) - 1;
                        done = nextY < MinY || (strict && nextY == MinY);
                    }

                    if (!done)
                    {
                        var xAtNextY = XAt(nextY).Value;

                        var nextYPoint = new PointD2D(xAtNextY, nextY);
                        var distToNextYPointSq = (nextYPoint - currentPoint).AsVectorD2D().MagnitudeSquared;

                        if (distToNextYPointSq < distSq)
                        {
                            distSq = distToNextYPointSq;
                            nextPoint = nextYPoint;
                        }
                    }
                }

                if(!Vertical)
                {
                    int nextX;
                    bool done;
                    if (incrementingX)
                    {
                        nextX = (int)Math.Floor(currentPoint.X) + 1;
                        done = nextX > MaxX || (strict && nextX == MaxX);
                    }
                    else
                    {
                        nextX = (int)Math.Ceiling(currentPoint.X) - 1;
                        done = nextX < MinX || (strict && nextX == MinX);
                    }


                    if (!done)
                    {
                        var yAtNextX = YAt(nextX).Value;

                        var nextXPoint = new PointD2D(nextX, yAtNextX);
                        var distToNextXPointSq = (nextXPoint - currentPoint).AsVectorD2D().MagnitudeSquared;

                        if(distToNextXPointSq < distSq)
                        {
                            distSq = distToNextXPointSq;
                            nextPoint = nextXPoint;
                        }
                    }
                }


                if (nextPoint == null)
                {
                    return points;
                }
                    
                
                currentPoint = nextPoint;
            }
        }

        public override string ToString()
        {
            return $"Finite Line [{Start} to {End}]";
        }
    }
}
