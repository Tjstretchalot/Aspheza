﻿using System;
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
        public List<VectorD2D> UniqueNormals { get; protected set; }

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
        /// <param name="axis">The axis to project this polygon onto.</param>
        /// <returns>The projection of this polygon along the specified axis.</returns>
        public OneDimensionalLine ProjectOntoAxis(VectorD2D axis)
        {
            return null; // TODO
        }

        /// <summary>
        /// Determines if this polygon intersects the specified other polygon.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="myPosition"></param>
        /// <param name="otherPosition"></param>
        /// <returns></returns>
        public bool Intersects(PolygonD2D other, PointD2D myPosition = null, PointD2D otherPosition = null)
        {
            return false; // TODO
        }
        
        /// <summary>
        /// Removes any vectors in the list that are parallel to each other.
        /// </summary>
        /// <param name="normals">The list of vectors</param>
        protected void RemoveRedundantNormals(List<VectorD2D> normals)
        {
            // TODO
        }        
    }
}
