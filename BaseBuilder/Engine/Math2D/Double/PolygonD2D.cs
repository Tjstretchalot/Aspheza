using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Function signature for projecting something that takes up space onto an axis.
    /// </summary>
    /// <param name="axis">The axis to project onto</param>
    /// <param name="position">The position of this thing being projected, or null for the origin</param>
    /// <returns>The line of this thing projected onto the specified axis</returns>
    public delegate OneDimensionalLine ProjectionFunc(VectorD2D axis, PointD2D position = null);

    /// <summary>
    /// Describes a convex polygon. A polygon itself does not move.
    /// </summary>
    public class PolygonD2D
    {
        /// <summary>
        /// A reference to a unit square top-left (0, 0) center (0.5, 0.5)
        /// </summary>
        public static RectangleD2D UnitSquare;

        static PolygonD2D()
        {
            UnitSquare = new RectangleD2D(1, 1);
        }

        /// <summary>
        /// The vertices of the polygon, ordered such that any adjacent
        /// indexes can be used to create a line (wrapping around).
        /// 
        /// I.e. in a triangle (3 vertices) indexes 0-1 are a line, 1-2 are a line,
        /// and 2-0 are a line.
        /// </summary>
        public List<PointD2D> Vertices { get; protected set; }

        /// <summary>
        /// The lines created from the vertices. 
        /// </summary>
        public List<FiniteLineD2D> Lines { get; protected set; }

        /// <summary>
        /// The list of unique normal vectors to the lines of this polygon.
        /// Two normals are only considered unique if they are not parallel 
        /// to each other.
        /// </summary>
        public List<VectorD2D> UniqueUnitNormals { get; protected set; }
        
        /// <summary>
        /// Lowest y value on this polygon
        /// </summary>
        public double Top { get; protected set; }

        /// <summary>
        /// Highest y value on this polygon
        /// </summary>
        public double Bottom { get; protected set; }

        /// <summary>
        /// Lowest x value on this polygon
        /// </summary>
        public double Left { get; protected set; }

        /// <summary>
        /// Highest x value on this polygon
        /// </summary>
        public double Right { get; protected set; }

        protected PointD2D _Midpoint;

        /// <summary>
        /// The middle point of this polygon.
        /// </summary>
        public PointD2D Midpoint
        {
            get
            {
                if(_Midpoint == null)
                {
                    var sumX = 0.0;
                    var sumY = 0.0;

                    foreach(var vertix in Vertices)
                    {
                        sumX += vertix.X;
                        sumY += vertix.Y;
                    }

                    _Midpoint = new PointD2D(sumX / Vertices.Count, sumY / Vertices.Count);
                }

                return _Midpoint;
            }
        }

        /// <summary>
        /// Initializes this polygon from a list of vertices.
        /// </summary>
        /// <param name="vertices">The ordered vertices</param>
        /// <exception cref="ArgumentNullException">If vertices are null</exception>
        public PolygonD2D(List<PointD2D> vertices)
        {
            Init(vertices);
        }
        
        public PolygonD2D(NetIncomingMessage message)
        {
            var numVertices = message.ReadInt32();

            Vertices = new List<PointD2D>(numVertices);
            for(var i = 0; i < numVertices; i++)
            {
                Vertices.Add(new PointD2D(message));
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(Vertices.Count);
            foreach(var vertix in Vertices)
            {
                vertix.Write(message);
            }
        }

        /// <summary>
        /// Empty constructor for subclasses. Should call Init(Vertices) in the child
        /// constructor!
        /// </summary>
        protected PolygonD2D()
        {
        }

        /// <summary>
        /// Sets up vertices, lines, and unique unit normals.
        /// </summary>
        /// <param name="vertices">The vertices of this polygon</param>
        protected void Init(List<PointD2D> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            Vertices = vertices;
            Lines = new List<FiniteLineD2D>();

            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                Lines.Add(new FiniteLineD2D(Vertices[i], Vertices[i + 1]));
            }

            Lines.Add(new FiniteLineD2D(Vertices[Vertices.Count - 1], Vertices[0]));

            UniqueUnitNormals = new List<VectorD2D>();

            foreach (var line in Lines)
            {
                UniqueUnitNormals.Add(line.Normal.UnitVector);
            }

            Left = double.MaxValue;
            Right = double.MinValue;
            Top = double.MaxValue;
            Bottom = double.MinValue;

            foreach(var vertix in Vertices)
            {
                Left = Math.Min(Left, vertix.X);
                Right = Math.Max(Right, vertix.X);
                Top = Math.Min(Top, vertix.Y);
                Bottom = Math.Max(Bottom, vertix.Y);
            }

            RemoveRedundantNormals(UniqueUnitNormals);
        }

        /// <summary>
        /// Projects this polygon onto the specified axis.
        /// </summary>
        /// <param name="unitAxis">The axis to project this polygon onto.</param>
        /// <param name="myPosition">The position of this polygon, or null for the origin.</param>
        /// <returns>The projection of this polygon along the specified axis.</returns>
        /// <exception cref="ArgumentNullException">If unitAxis is null</exception>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D unitAxis, PointD2D myPosition = null)
        {
            if (unitAxis == null)
                throw new ArgumentNullException(nameof(unitAxis));

            var min = double.MaxValue;
            var max = double.MinValue;

            foreach(var vertix in Vertices)
            {
                var proj = PointD2D.DotProduct(vertix.X, vertix.Y, unitAxis.DeltaX, unitAxis.DeltaY, myPosition, null);

                min = Math.Min(min, proj);
                max = Math.Max(max, proj);
            }

            return new OneDimensionalLine(unitAxis, min, max);
        }

        /// <summary>
        /// Returns true if this polygon contains the specified point. Returns false otherwise.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="myPosition">The position of this polygon, or null if the origin</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns></returns>
        public bool Contains(PointD2D point, PointD2D myPosition = null, bool strict = false)
        {
            foreach (var normal in UniqueUnitNormals)
            {
                var myProjection = ProjectOntoAxis(normal, myPosition);
                var pointProjection = PointD2D.DotProduct(point.X, point.Y, normal.DeltaX, normal.DeltaY);

                if (!myProjection.ContainPoint(pointProjection, !strict))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Equivalent to Intersects(Polygon), except instead of describing the polygon using this class only requires
        /// the minimum information necessary - the ability to project onto an axis, and the potential intersection axes
        /// of the polygon.
        /// </summary>
        /// <remarks>
        /// This still only works for convex polygons.
        /// </remarks>
        /// <param name="projector">Ability to project onto an axis.</param>
        /// <param name="otherNormals">Axes that the polygon can intersect on.</param>
        /// <param name="myPosition">Where this instance is located, null for the origin.</param>
        /// <param name="otherPosition">Where the other polygon is located, null for the origin.</param>
        /// <param name="strict">If touching constitutes intersection.</param>
        /// <returns>True if this polygon intersects the described polygon, false otherwise.</returns>
        public bool Intersects(ProjectionFunc projector, List<VectorD2D> otherNormals, PointD2D myPosition = null, PointD2D otherPosition = null, bool strict = false)
        {
            var normals = new List<VectorD2D>();
            normals.AddRange(UniqueUnitNormals);
            normals.AddRange(otherNormals);
            RemoveRedundantNormals(normals);
            
            foreach (var normal in normals)
            {
                var myProj = ProjectOntoAxis(normal, myPosition);
                var otherProj = projector(normal, otherPosition);

                if (!myProj.Intersects(otherProj, !strict))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this polygon intersects the specified other polygon.
        /// </summary>
        /// <param name="other">The polygon to compare with</param>
        /// <param name="myPosition">Where this polygon is located, null for the origin</param>
        /// <param name="otherPosition">Where the other polygon is located, null for the origin</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True for intersection, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(PolygonD2D other, PointD2D myPosition = null, PointD2D otherPosition = null, bool strict = false)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Intersects(other.ProjectOntoAxis, other.UniqueUnitNormals, myPosition, otherPosition, strict);
        }

        /// <summary>
        /// Equivalent to IntersectionMTV(Polygon), except accepts a function that projects the shape onto an axis
        /// and the potential intersection axes rather than a polygon. This is convienent when creating a polygon
        /// instance is not desirable, such as when the polygon's size changes often.
        /// </summary>
        /// <remarks>
        /// This still only works for convex polygons.
        /// </remarks>
        /// <param name="projector">Projection func.</param>
        /// <param name="otherNormals">The potential intersection axes.</param>
        /// <param name="myPosition">The position of this polygon, null for the origin.</param>
        /// <param name="otherPosition">The position of the described polygon, null for the origin.</param>
        /// <returns></returns>
        public VectorD2D IntersectionMTV(ProjectionFunc projector, List<VectorD2D> otherNormals, PointD2D myPosition = null, PointD2D otherPosition = null)
        {
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));
            if (otherNormals == null)
                throw new ArgumentNullException(nameof(otherNormals));


            var normals = new List<VectorD2D>();
            normals.AddRange(UniqueUnitNormals);
            normals.AddRange(otherNormals);
            RemoveRedundantNormals(normals);

            OneDimensionalLine bestChoice = null;
            foreach (var normal in normals)
            {
                var myProj = ProjectOntoAxis(normal, myPosition);
                var otherProj = projector(normal, otherPosition);

                var intersection = myProj.IntersectionLine(otherProj);

                if (intersection == null)
                    return null; // Polygons are seperated by this axis

                if (bestChoice == null || intersection.Length < bestChoice.Length)
                {
                    bestChoice = intersection;
                }
            }

            return bestChoice.AsFiniteLineD2D().Axis;
        }

        /// <summary>
        /// Determines the minimum translation vector to push THIS INSTANCE in order for this instance to no 
        /// longer intersect the other polygon. If this polygon does not strictly intersect the other polygon,
        /// this simply returns null.
        /// </summary>
        /// <param name="other">The other polygon</param>
        /// <param name="myPosition">Where this polygon is located, null for the origin</param>
        /// <param name="otherPosition">Where the other polygon is located, null for the origin</param>
        /// <returns>The minimum translation vector to be applied on this polygon to stop intersection</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public VectorD2D IntersectionMTV(PolygonD2D other, PointD2D myPosition = null, PointD2D otherPosition = null)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return IntersectionMTV(other.ProjectOntoAxis, other.UniqueUnitNormals, myPosition, otherPosition);
        }

        /// <summary>
        /// Returns a list of points which correspond to the unit squares intersected on a
        /// grid while this polygon is at offset to myPosition.
        /// </summary>
        /// <param name="myPosition">The position of this polygon for this test</param>
        /// <param name="list">
        /// Sets to the list of points of unit squares intersected while this polygon is at myPosition. Any points in the list at the
        /// start of the function are assumed to be reusable.
        /// </param>
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
                for (locationY = minY;  locationY < maxY; locationY++)
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
        /// Returns the list of tiles that are intersected when this polygon moves from no offset to offset,
        /// where is adjacent to the origin (including diagonals)
        /// </summary>
        /// <remarks>
        /// This function should have it's results cached internally and lazy loaded.
        /// </remarks>
        /// <param name="offset">The offset that this polygon is moving to from the origin</param>
        public HashSet<PointI2D> TilesIntersectedWhenMovingFromOriginTo(PointI2D offset)
        {
            if (offset == null)
                throw new ArgumentNullException(nameof(offset));

            HashSet<PointI2D> result;
            if (_TilesIntersectedWhenMovingFromOriginToCache == null)
                _TilesIntersectedWhenMovingFromOriginToCache = new Dictionary<PointI2D, HashSet<PointI2D>>();
            if(!_TilesIntersectedWhenMovingFromOriginToCache.TryGetValue(offset, out result))
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
            PointD2D start = new PointD2D(0, 0);
            HashSet <PointI2D> result = new HashSet<PointI2D>();
            List<PointD2D> lineVertices = new List<PointD2D> { new PointD2D(0,0), new PointD2D(0, 0) };

            foreach (PointD2D vertices in Vertices)
            {
                lineVertices[0] = vertices;
                lineVertices[1].X = vertices.X + offset.X;
                lineVertices[1].Y = vertices.Y + offset.Y;

                var line = new FiniteLineD2D(lineVertices[0], lineVertices[1]);
                
                var list = line.GetTilesIntersected(true);

                foreach (PointI2D point in list)
                {
                    result.Add(point);
                }
            }

            List<PointI2D> list2 = new List<PointI2D>();
            TilesIntersectedAt(start, list2);

            foreach (PointI2D point in list2)
            {
                result.Add(point);
            }
            List<PointI2D> list3 = new List<PointI2D>();
            TilesIntersectedAt(new PointD2D(offset.X, offset.Y), list3);

            foreach (PointI2D point in list3)
            {
                result.Add(point);
            }

            return result;
        }

        /// <summary>
        /// Removes any vectors in the list that are parallel to each other.
        /// </summary>
        /// <param name="normals">The list of vectors</param>
        protected void RemoveRedundantNormals(List<VectorD2D> normals)
        {
            if (normals == null)
                throw new ArgumentNullException(nameof(normals));
            
            // Work from the back to the front because we're removing elements.
            for(int i = normals.Count - 1; i >= 1; i--)
            {
                for(int j = i - 1; j >= 0; j--)
                {
                    if(normals[i].IsParallel(normals[j]))
                    {
                        normals.RemoveAt(i);
                        break;
                    }
                }
            }
        }        
    }
}
