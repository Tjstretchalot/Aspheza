using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Describes an infinite line with two points that it goes
    /// through.
    /// </summary>
    public class InfiniteLineD2D 
    {
        /// <summary>
        /// One point that this line touches.
        /// </summary>
        public PointD2D Point1 { get; protected set; }

        /// <summary>
        /// Another point that this line touches.
        /// </summary>
        public PointD2D Point2 { get; protected set; }
        
        protected double? _Slope;

        /// <summary>
        /// The slope of the line (rise over run)
        /// </summary>
        public double Slope
        {
            get
            {
                if(!_Slope.HasValue)
                {
                    _Slope = (Point2.Y - Point1.Y) / (Point2.X - Point1.Y);
                }

                return _Slope.Value;
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
                    _Normal = new VectorD2D(-(Point2.Y - Point1.Y), (Point2.X - Point1.X));
                }

                return _Normal; 
            }
        }

        /// <summary>
        /// Creates a new infinite line that goes through both
        /// p1 and p2.
        /// </summary>
        /// <param name="p1">A point on the line.</param>
        /// <param name="p2">A different point on the line.</param>
        public InfiniteLineD2D(PointD2D p1, PointD2D p2)
        {
            Point1 = p1;
            Point2 = p2;
        }

        public override string ToString()
        {
            return $"Inf.Line [through {Point1} and {Point2}]";
        }
    }
}
