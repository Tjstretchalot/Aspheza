using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    public class PointD2D
    {
        public double X;
        public double Y;

        public PointD2D(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        public static PointD2D operator +(PointD2D p1, PointD2D p2)
        {
            return new PointD2D(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointD2D operator -(PointD2D p1, PointD2D p2)
        {
            return new PointD2D(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointD2D operator *(PointD2D p1, double scalar)
        {
            return new PointD2D(p1.X * scalar, p1.Y * scalar);
        }

        public static PointD2D operator /(PointD2D p1, double divisor)
        {
            return new PointD2D(p1.X / divisor, p1.Y / divisor);
        }

        public static bool operator ==(PointD2D p1, PointD2D p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(PointD2D p1, PointD2D p2)
        {
            return p1.X != p2.X || p1.Y != p2.Y;
        }

        /// <summary>
        /// Returns the vector representation of this point. 
        /// </summary>
        /// <returns>This point as a vector.</returns>
        public VectorD2D AsVectorD2D()
        {
            return new VectorD2D(X, Y);
        }

        /// <summary>
        /// Returns the point created by shifting this point x, y.
        /// </summary>
        /// <remarks>
        /// If you want to shift by another point, simply add the points together.
        /// </remarks>
        /// <param name="dx">The x to shift this point by</param>
        /// <param name="dy">The y to shift this point by</param>
        /// <returns>The shifted point</returns>
        public PointD2D Shift(double dx, double dy)
        {
            return new PointD2D((X + dx), (Y + dy));
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            var p2 = obj as PointD2D;

            if (p2 == null)
                return false;

            return this == p2;
        }

        public override int GetHashCode()
        {
            int result = 31;

            result = result * 17 + X.GetHashCode();
            result = result * 17 + Y.GetHashCode();

            return result;
        }
    }
}
