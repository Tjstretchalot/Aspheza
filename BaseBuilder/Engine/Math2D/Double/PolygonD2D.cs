using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Describes a convex polygon. A polygon itself does not move.
    /// </summary>
    public class PolygonD2D
    {
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

        protected PointD2D _Midpoint;

        /// <summary>
        /// The middle point of this polygon.
        /// </summary>
        public PointD2D Midpoint
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes this polygon from a list of vertices.
        /// </summary>
        /// <param name="vertices"></param>
        public PolygonD2D(List<PointD2D> vertices)
        {
            // TODO init vertices, lines, and unique normals. Use RemoveRedundantNormals (bottom).
        }

        /// <summary>
        /// Projects this polygon onto the specified axis.
        /// </summary>
        /// <param name="unitAxis">The axis to project this polygon onto.</param>
        /// <param name="myPosition">The position of this polygon, or null for the origin.</param>
        /// <returns>The projection of this polygon along the specified axis.</returns>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D unitAxis, PointD2D myPosition = null)
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            foreach(var line in Lines)
            {
                var proj = line.ProjectOntoAxis(unitAxis, myPosition);

                min = Math.Min(min, Math.Min(proj.Start, proj.End));
                max = Math.Max(max, Math.Max(proj.Start, proj.End));
            }

            return new OneDimensionalLine(unitAxis, min, max);
        }

        /// <summary>
        /// Determines if this polygon intersects the specified other polygon.
        /// </summary>
        /// <param name="other">The polygon to compare with</param>
        /// <param name="myPosition">Where this polygon is located, null for the origin</param>
        /// <param name="otherPosition">Where the other polygon is located, null for the origin</param>
        /// <param name="strict">False if touching constitutes intersection, false otherwise.</param>
        /// <returns>True for intersection, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool Intersects(PolygonD2D other, PointD2D myPosition = null, PointD2D otherPosition = null, bool strict = false)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var normals = new List<VectorD2D>();
            normals.AddRange(UniqueUnitNormals);
            normals.AddRange(other.UniqueUnitNormals);
            RemoveRedundantNormals(normals);
            
            foreach (var normal in normals)
            {
                var myProj = ProjectOntoAxis(normal, myPosition);
                var otherProj = other.ProjectOntoAxis(normal, otherPosition);

                if (!myProj.Intersects(otherProj, strict))
                    return false;
            }

            return true;
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

            var normals = new List<VectorD2D>();
            normals.AddRange(UniqueUnitNormals);
            normals.AddRange(other.UniqueUnitNormals);
            RemoveRedundantNormals(normals);

            OneDimensionalLine bestChoice = null;
            foreach(var normal in normals)
            {
                var myProj = ProjectOntoAxis(normal, myPosition);
                var otherProj = other.ProjectOntoAxis(normal, otherPosition);

                var intersection = myProj.IntersectionLine(otherProj);

                if (intersection == null)
                    return null; // Polygons are seperated by this axis

                if(bestChoice == null || intersection.Length < bestChoice.Length)
                {
                    bestChoice = intersection;
                }
            }

            return bestChoice.AsFiniteLineD2D().Axis;
        }
        
        /// <summary>
        /// Removes any vectors in the list that are parallel to each other.
        /// </summary>
        /// <param name="normals">The list of vectors</param>
        protected void RemoveRedundantNormals(List<VectorD2D> normals)
        {
            if (normals == null)
                throw new ArgumentNullException(nameof(normals));
            // TODO
        }        
    }
}
