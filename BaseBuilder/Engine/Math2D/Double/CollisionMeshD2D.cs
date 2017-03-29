using BaseBuilder.Engine.Logic;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// A collision mesh is a collection of polygons.
    /// </summary>
    public class CollisionMeshD2D
    {
        public static CollisionMeshD2D UnitSquare;

        static CollisionMeshD2D()
        {
            UnitSquare = new CollisionMeshD2D(new List<PolygonD2D> { PolygonD2D.UnitSquare });
        }

        public List<PolygonD2D> Polygons { get; protected set; }
        public RectangleD2D BoundingBox { get; protected set; }

        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double Left { get; protected set; }
        public double Right { get; protected set; }
        public double Top { get; protected set; }
        public double Bottom { get; protected set; }

        protected List<FiniteLineD2D> _Lines;
        public List<FiniteLineD2D> Lines
        {
            get
            {
                if (_Lines == null)
                {
                    var naive = new List<FiniteLineD2D>();
                    foreach(var poly in Polygons)
                    {
                        foreach (var line in poly.Lines)
                        {
                            //LogicUtils.BinaryInsert(naive, line, (line1, line2) => Math.Sign(line2.LengthSquared - line1.LengthSquared));
                            naive.Add(line);
                        }
                    }

                    for (int outerLineIndex = 0; outerLineIndex < naive.Count; outerLineIndex++)
                    {
                        var outerLine = naive[outerLineIndex];
                        var outerLineAxisProj = outerLine.ProjectOntoAxis(outerLine.Axis.UnitVector);
                        var outerLineNormalProj = outerLine.ProjectOntoAxis(outerLine.Normal.UnitVector);
                        var outerLineNormalPoint = outerLineNormalProj.AsPointD2D();
                        for (int innerLineIndex = outerLineIndex + 1; innerLineIndex < naive.Count; innerLineIndex++)
                        {
                            var innerLine = naive[innerLineIndex];
                            if (!innerLine.Axis.UnitVector.IsParallel(outerLine.Axis.UnitVector))
                                continue;
                            var innerLineNormalProj = innerLine.ProjectOntoAxis(outerLine.Normal.UnitVector);
                            if (outerLineNormalProj.Start != innerLineNormalProj.Start)
                                continue;
                            var innerLineAxisProj = innerLine.ProjectOntoAxis(outerLine.Axis.UnitVector);
                            var intersection = outerLineAxisProj.IntersectionLine(innerLineAxisProj);
                            if (intersection == null)
                                continue;
                            if (outerLineAxisProj.Min != intersection.Min)
                            {
                                var newLine1D = new OneDimensionalLine(intersection.Axis, outerLineAxisProj.Min, intersection.Min);
                                var asLine = newLine1D.AsFiniteLineD2D();
                                asLine = asLine.Shift(outerLineNormalPoint.X, outerLineNormalPoint.Y);
                                naive.Add(asLine);
                            }
                            if (outerLineAxisProj.Max != intersection.Max)
                            {
                                var newLine1D = new OneDimensionalLine(intersection.Axis, intersection.Max, outerLineAxisProj.Max);
                                var asLine = newLine1D.AsFiniteLineD2D();
                                asLine = asLine.Shift(outerLineNormalPoint.X, outerLineNormalPoint.Y);
                                naive.Add(asLine);
                            }

                            if (innerLineAxisProj.Min != intersection.Min)
                            {
                                var newLine1D = new OneDimensionalLine(intersection.Axis, innerLineAxisProj.Min, intersection.Min);
                                var asLine = newLine1D.AsFiniteLineD2D();
                                asLine = asLine.Shift(outerLineNormalPoint.X, outerLineNormalPoint.Y);
                                naive.Add(asLine);
                            }
                            if (innerLineAxisProj.Max != intersection.Max)
                            {
                                var newLine1D = new OneDimensionalLine(intersection.Axis, intersection.Max, innerLineAxisProj.Max);
                                var asLine = newLine1D.AsFiniteLineD2D();
                                asLine = asLine.Shift(outerLineNormalPoint.X, outerLineNormalPoint.Y);
                                naive.Add(asLine);
                            }

                            naive.RemoveAt(innerLineIndex);
                            naive.RemoveAt(outerLineIndex);
                            outerLineIndex--;
                            break;
                        }
                    }
                    _Lines = naive;
                }
                return _Lines;
            }
        }

        protected List<PointI2D> _AdjacentPoints;

        /// <summary>
        /// Gets the list of adjacent points, lazily initialized
        /// </summary>
        public List<PointI2D> AdjacentPoints
        {
            get
            {
                if(_AdjacentPoints == null)
                {
                    _AdjacentPoints = new List<PointI2D>();

                    var floorTop = (int)Math.Floor(Top);
                    var ceilBot = (int)Math.Ceiling(Bottom);
                    var floorLeft = (int)Math.Floor(Left);
                    var ceilRight = (int)Math.Ceiling(Right);
                    for(int y = floorTop - 1; y <= ceilBot + 1; y++)
                    {
                        for(int x = floorLeft - 1; x <= ceilRight + 1; x++)
                        {
                            if (Intersects(UnitSquare, null, new PointD2D(x, y), true))
                                continue;

                            if (!Intersects(UnitSquare, null, new PointD2D(x, y), false) && !MinDistanceShorterThan(UnitSquare, 0.8, null, new PointD2D(x, y)))
                                continue;

                            _AdjacentPoints.Add(new PointI2D(x, y));
                        }
                    }
                }

                return _AdjacentPoints;
            }
        }
        /// <summary>
        /// Create the collision mesh containing the specified polygons
        /// </summary>
        /// <param name="polygons">the polygons</param>
        public CollisionMeshD2D(List<PolygonD2D> polygons)
        {
            Polygons = polygons;

            Left = double.MaxValue;
            Right = double.MinValue;
            Top = double.MaxValue;
            Bottom = double.MinValue;

            foreach (var poly in polygons)
            {
                Left = Math.Min(Left, poly.Left);
                Right = Math.Max(Right, poly.Right);
                Top = Math.Min(Top, poly.Top);
                Bottom = Math.Max(Bottom, poly.Bottom);
            }

            Width = Right - Left;
            Height = Bottom - Top;

            BoundingBox = new RectangleD2D(Width, Height, Left, Top);
        }

        public CollisionMeshD2D(NetIncomingMessage message)
        {
            int numPolys = message.ReadInt32();
            Polygons = new List<PolygonD2D>();

            for (int i = 0; i < numPolys; i++)
            {
                Polygons.Add(new PolygonD2D(message));
            }

            Left = message.ReadDouble();
            Right = message.ReadDouble();
            Top = message.ReadDouble();
            Bottom = message.ReadDouble();
            Width = message.ReadDouble();
            Height = message.ReadDouble();

            BoundingBox = new RectangleD2D(Width, Height, Left, Top);
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(Polygons.Count);

            foreach (var poly in Polygons)
            {
                poly.Write(message);
            }

            message.Write(Left);
            message.Write(Right);
            message.Write(Top);
            message.Write(Bottom);
            message.Write(Width);
            message.Write(Height);
        }

        /// <summary>
        /// Determines if this collision mesh contains the specified point when
        /// it is at the specified position. Always not strict.
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="myPosition">My positiion</param>
        /// <returns>If this collision mesh contains the specified point</returns>
        public bool Contains(PointD2D point, PointD2D myPosition = null)
        {
            return Polygons.Any((p) => p.Contains(point, myPosition, false));
        }
        
        /// <summary>
        /// Calculates the shortest distance that must be traveled for this
        /// collision mesh to get to the other collision mesh. Does not work
        /// if the two collision meshes are intersecting.
        /// </summary>
        /// <param name="other">The other collision mesh</param>
        /// <param name="myPos">My position (or null)</param>
        /// <param name="otherPos">Other position (or null)</param>
        /// <returns></returns>
        public double MinDistanceTo(CollisionMeshD2D other, PointD2D myPos = null, PointD2D otherPos = null)
        {
            double minDistance = double.MaxValue;

            foreach (var poly in Polygons)
            {
                foreach (var poly2 in other.Polygons)
                {
                    var dist = poly.MinDistanceTo(poly2, myPos, otherPos);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                    }
                }
            }

            return minDistance;
        }

        /// <summary>
        /// Determines if the min distance from this polygon to the other polygon is shorter than the specified
        /// maximum minimum distance. This allows for more optimizations than could be done calculating MinDistanceTo.
        /// Does not work if the two collision meshes are intersecting.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="myPos"></param>
        /// <param name="otherPos"></param>
        /// <param name="maxMinDistance"></param>
        /// <returns></returns>
        public bool MinDistanceShorterThan(CollisionMeshD2D other, double maxMinDistance, PointD2D myPos = null, PointD2D otherPos = null)
        {
            foreach (var poly in Polygons)
            {
                foreach (var poly2 in other.Polygons)
                {
                    if (poly.MinDistanceTo(poly2, myPos, otherPos) <= maxMinDistance)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the shortest vector that goes from this collision mesh to the
        /// other collision mesh. Does not work if the two collision meshes are intersecting.
        /// </summary>
        /// <param name="other">The other collision mesh</param>
        /// <param name="myPos">My position</param>
        /// <param name="otherPos">Other position</param>
        /// <returns>Shortest vector from this to other</returns>
        public VectorD2D MinVectorTo(CollisionMeshD2D other, PointD2D myPos = null, PointD2D otherPos = null)
        {
            VectorD2D bestVector = null;

            foreach (var poly in Polygons)
            {
                foreach (var poly2 in other.Polygons)
                {
                    var vec = poly.MinVectorTo(poly2, myPos, otherPos);

                    if (bestVector == null || vec.MagnitudeSquared < bestVector.MagnitudeSquared)
                    {
                        bestVector = vec;
                    }
                }
            }

            return bestVector;
        }

        /// <summary>
        /// Determines if this collision mesh intersects the other collision mesh.
        /// </summary>
        /// <param name="other">The other collision mesh</param>
        /// <param name="myPos">My position</param>
        /// <param name="otherPos">Other position</param>
        /// <param name="strict">If touching constitutes intersection</param>
        /// <returns></returns>
        public bool Intersects(CollisionMeshD2D other, PointD2D myPos = null, PointD2D otherPos = null, bool strict = false)
        {
            foreach (var poly in Polygons)
            {
                foreach (var poly2 in other.Polygons)
                {
                    if (poly.Intersects(poly2, myPos, otherPos, strict))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the minimum translation vector that must be applied to this collision
        /// mesh to cause it not to intersect the other collision mesh. This will only work
        /// if the other collision mesh is only "slightly" inside of this collision mesh.
        /// </summary>
        /// <param name="other">The other collision mesh</param>
        /// <param name="myPos">My position</param>
        /// <param name="otherPos">Other position</param>
        /// <returns>The minimum translation vector to apply to this to prevent intersection.</returns>
        public VectorD2D IntersectionMTV(CollisionMeshD2D other, PointD2D myPos = null, PointD2D otherPos = null)
        {
            VectorD2D result = null;

            foreach (var poly in Polygons)
            {
                foreach (var poly2 in other.Polygons)
                {
                    var tmp = poly.IntersectionMTV(poly2, myPos, otherPos);

                    if (tmp != null)
                    {
                        if (result == null)
                            result = tmp;
                        else
                            result += tmp;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the tiles that are intersected when this collision mesh is at
        /// the specified point. This list will only contain the tiles intersected
        /// by this collision mesh after being called, and any points inside the list
        /// will be reused.
        /// </summary>
        /// <param name="myPos">My position</param>
        /// <param name="list">The list to populate</param>
        public void TilesIntersectedAt(PointD2D myPosition, List<PointI2D> list)
        {
            int locationX = (int)(Math.Floor(Left) + Math.Floor(myPosition.X));
            int maxX = (int)(Math.Ceiling(Right) + Math.Ceiling(myPosition.X));
            int locationY;
            int minY = (int)(Math.Floor(Top) + Math.Floor(myPosition.Y));
            int maxY = (int)(Math.Ceiling(Bottom) + Math.Ceiling(myPosition.Y));
            int listIndex = 0;
            PointD2D location = new PointD2D(0, 0);

            for (; locationX < maxX; locationX++)
            {
                for (locationY = minY; locationY < maxY; locationY++)
                {
                    location.X = locationX;
                    location.Y = locationY;
                    if (Intersects(UnitSquare, myPosition, location, true))
                    {
                        if (listIndex < list.Count)
                        {
                            list[listIndex].X = locationX;
                            list[listIndex].Y = locationY;
                            listIndex++;
                        }
                        else
                        {
                            list.Add(new PointI2D(locationX, locationY));
                            listIndex++;
                        }
                    }
                }
            }
            for (int endOfList = list.Count - 1; endOfList >= listIndex; endOfList--)
            {
                list.RemoveAt(endOfList);
            }
        }

        protected Dictionary<PointI2D, HashSet<PointI2D>> _TilesIntersectedWhenMovingFromOriginToCache;

        /// <summary>
        /// Finds the tiles that will be intersected when this collision mesh moves
        /// from the origin to the specified offset. The result is lazily cached.
        /// </summary>
        /// <param name="offset">The offset</param>
        /// <returns>The tiles intersected by this collision mesh during the move</returns>
        public HashSet<PointI2D> TilesIntersectedWhenMovingFromOriginTo(PointI2D offset)
        {
            if (offset == null)
                throw new ArgumentNullException(nameof(offset));

            HashSet<PointI2D> result;
            if (_TilesIntersectedWhenMovingFromOriginToCache == null)
                _TilesIntersectedWhenMovingFromOriginToCache = new Dictionary<PointI2D, HashSet<PointI2D>>();
            if (!_TilesIntersectedWhenMovingFromOriginToCache.TryGetValue(offset, out result))
            {
                if (offset.X < -1 || offset.X > 1 || offset.Y < -1 || offset.Y > 1)
                    throw new InvalidProgramException($"{offset} is not adjacent to (0, 0)");
                if (offset.X == 0 && offset.Y == 0)
                    throw new InvalidProgramException($"{offset} is not a valid argument to TilesIntersectedWhenMovingFromOriginTo - it implies no movement!");

                result = TilesIntersectedWhenMovingFromOriginToImpl(offset);

                _TilesIntersectedWhenMovingFromOriginToCache.Add(offset, result);
            }

            return result;
        }

        protected HashSet<PointI2D> TilesIntersectedWhenMovingFromOriginToImpl(PointI2D offset)
        {
            var result = new HashSet<PointI2D>();

            foreach (var poly in Polygons)
            {
                foreach (var intr in poly.TilesIntersectedWhenMovingFromOriginTo(offset))
                {
                    result.Add(intr);
                }
            }

            return result;
        }
    }
}
