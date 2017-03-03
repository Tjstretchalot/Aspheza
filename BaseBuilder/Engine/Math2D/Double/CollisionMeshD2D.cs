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
        public List<PolygonD2D> Polygons { get; protected set; }

        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public double Left { get; protected set; }
        public double Right { get; protected set; }
        public double Top { get; protected set; }
        public double Bottom { get; protected set; }

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

            foreach(var poly in polygons)
            {
                Left = Math.Min(Left, poly.Left);
                Right = Math.Max(Right, poly.Right);
                Top = Math.Min(Top, poly.Top);
                Bottom = Math.Max(Bottom, poly.Bottom);
            }

            Width = Right - Left;
            Height = Bottom - Top;
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
    }
}
