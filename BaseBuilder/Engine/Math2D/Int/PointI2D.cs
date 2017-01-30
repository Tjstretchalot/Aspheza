using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math
{
    public class PointI2D
    {
        public int X;
        public int Y;

        public PointI2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static PointI2D operator +(PointI2D p1, PointI2D p2)
        {
            return new PointI2D(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static PointI2D operator -(PointI2D p1, PointI2D p2)
        {
            return new PointI2D(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointI2D operator *(PointI2D p1, int scalar)
        {
            return new PointI2D(p1.X * scalar, p1.Y * scalar);
        }

        public static PointI2D operator *(PointI2D p1, double scalar)
        {
            return new PointI2D((int)(p1.X * scalar), (int)(p1.Y * scalar));
        }

        public static PointI2D operator /(PointI2D p1, int divisor)
        {
            return new PointI2D(p1.X / divisor, p1.Y / divisor);
        }

        public static PointI2D operator /(PointI2D p1, double divisor)
        {
            return new PointI2D((int)(p1.X / divisor), (int)(p1.Y / divisor));
        }

        public static bool operator ==(PointI2D p1, PointI2D p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(PointI2D p1, PointI2D p2)
        {
            return p1.X != p2.X || p1.Y != p2.Y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            var p2 = obj as PointI2D;

            if (p2 == null)
                return false;

            return this == p2;
        }

        public override int GetHashCode()
        {
            int result = 31;

            result = result * 17 + X;
            result = result * 17 + Y;

            return result;
        }
    }
}
