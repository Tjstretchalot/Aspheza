using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math.Double
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
    }
}
