using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    public class RectangleD2D : PolygonD2D
    {
        public double Height { get; protected set; }

        public double Width { get; protected set; }

        /// <summary>
        /// Creates a new rectangle with the specified width and height.
        /// </summary>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="x">Optional x offset of the rectangle. Consider passing shift to the appropriate functions instead.</param>
        /// <param name="y">Optional y offset of the rectangle. Consider passing shift to the appropriate functions instead.</param>
        public RectangleD2D(double width, double height, double x = 0, double y = 0) : base()
        {
            Init(new List<PointD2D>
            {
                new PointD2D(x, y),             new PointD2D(x + width, y),
                new PointD2D(x, y + height),    new PointD2D(x + width, y + height)
            });

            Height = height;
            Width = width;
        }
    }
}
