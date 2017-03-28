using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace BaseBuilder.Engine.Math2D.Double
{
    public class PointD2D
    {
        public static PointD2D Origin;
        
        static PointD2D()
        {
            Origin = new PointD2D(0, 0);
        }

        public double X;
        public double Y;

        public PointD2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public PointD2D(NetIncomingMessage message)
        {
            X = message.ReadDouble();
            Y = message.ReadDouble();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(X);
            message.Write(Y);
        }

        public static PointD2D operator +(PointD2D p1, PointD2D p2)
        {
            return new PointD2D(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointD2D operator +(PointD2D p1, VectorD2D v2)
        {
            return new PointD2D(p1.X + v2.DeltaX, p1.Y + v2.DeltaY);
        }

        public static PointD2D operator -(PointD2D p1, PointD2D p2)
        {
            return new PointD2D(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointD2D operator -(PointD2D p1, VectorD2D v2)
        {
            return new PointD2D(p1.X - v2.DeltaX, p1.Y - v2.DeltaY);
        }

        public static PointD2D operator *(PointD2D p1, double scalar)
        {
            return new PointD2D(p1.X * scalar, p1.Y * scalar);
        }

        public static PointD2D operator /(PointD2D p1, double divisor)
        {
            return new PointD2D(p1.X / divisor, p1.Y / divisor);
        }

        public static PointD2D operator -(PointD2D p)
        {
            return new PointD2D(-p.X, -p.Y);
        }

        public static bool operator ==(PointD2D p1, PointD2D p2)
        {
            if (ReferenceEquals(p1, null) || ReferenceEquals(p2, null))
                return ReferenceEquals(p1, p2);

            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(PointD2D p1, PointD2D p2)
        {
            if (ReferenceEquals(p1, null) || ReferenceEquals(p2, null))
                return !ReferenceEquals(p1, p2);

            return p1.X != p2.X || p1.Y != p2.Y;
        }

        public static explicit operator PointI2D(PointD2D p)
        {
            if (ReferenceEquals(p, null))
                return null;

            return new PointI2D((int)p.X, (int)p.Y);
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

        /// <summary>
        /// Returns the dot product of this point with the thing specified by (x, y)
        /// </summary>
        /// <param name="x">The x</param>
        /// <param name="y">The y</param>
        /// <param name="shiftX">Added to my x</param>
        /// <param name="shiftY">Added to my y</param>
        /// <returns>this . (x, y)</returns>
        public double DotProduct(double x, double y, double shiftX = 0, double shiftY = 0)
        {
            return (X + shiftX) * x + (Y + shiftY) * y;
        }

        /// <summary>
        /// Returns the dot product of this point with the other point
        /// </summary>
        /// <param name="other">The other point</param>
        /// <param name="shift">The shift to my position or null for no shift</param>
        /// <returns>(this + shift) . other</returns>
        public double DotProduct(PointD2D other, PointD2D shift = null)
        {
            return DotProduct(other.X, other.Y, (shift == null ? 0 : shift.X), (shift == null ? 0 : shift.Y));
        }
        
        /// <summary>
        /// Returns the dot product of (x1 + shift1.X, y1 + shift1.Y) and (x2, shift2.X, y2 + shift2.Y). For shifts, null
        /// is assumed to mean the origin.
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">X2</param>
        /// <param name="y2">Y2</param>
        /// <param name="shift1">Shift of (x1, y1) or null</param>
        /// <param name="shift2">Shift of (x2, y2) or null</param>
        /// <returns></returns>
        public static double DotProduct(double x1, double y1, double x2, double y2, PointD2D shift1, PointD2D shift2)
        {
            x1 = x1 + (shift1 == null ? 0 : shift1.X);
            x2 = x2 + (shift2 == null ? 0 : shift2.X);

            y1 = y1 + (shift1 == null ? 0 : shift1.Y);
            y2 = y2 + (shift2 == null ? 0 : shift2.Y);

            return x1 * x2 + y1 * y2;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            var p2 = obj as PointD2D;

            if (ReferenceEquals(p2, null))
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
