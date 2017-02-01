using NUnit.Framework;
using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double.Tests
{
    [TestFixture()]
    public class PolygonD2DTests
    {
        [Test(Description = "Ensures that Contains returns false if the point is not contained within the polygon")]
        public void ContainsFalseForOutsidePointsTest()
        {
            // A triangle and a point that lies outside of said triangle
            var triangle = new PolygonD2D(new List<PointD2D> { new PointD2D(0, 0), new PointD2D(1, 1), new PointD2D(2, 0) });
            var point = new PointD2D(0.5, 1);

            Assert.IsFalse(triangle.Contains(point));

            // Another point that lies outside of the triangle
            point = new PointD2D(2, 2);

            Assert.IsFalse(triangle.Contains(point));

            // Another triangle and a point that lies outside of said triangle
            triangle = new PolygonD2D(new List<PointD2D> { new PointD2D(1, 1), new PointD2D(2, 2), new PointD2D(3, 1) });
            point = new PointD2D(2, 0.5);

            Assert.IsFalse(triangle.Contains(point));

            // Another point that lies outside of the triangle
            point = new PointD2D(1, 2);

            Assert.IsFalse(triangle.Contains(point));
        }

        [Test(Description = "Ensures that Contains returns true if the point is contained within the polygon")]
        public void ContainsTrueForInsidePointsTest()
        {
            // A triangle and a point that lies inside of said triangle
            var triangle = new PolygonD2D(new List<PointD2D> { new PointD2D(0, 0), new PointD2D(1, 1), new PointD2D(2, 0) });
            var point = new PointD2D(1, 0.5);

            Assert.IsTrue(triangle.Contains(point));

            // Another point that lies inside of the triangle
            point = new PointD2D(1.5, 0.25);

            Assert.IsTrue(triangle.Contains(point));

            // Another triangle and a point that lies inside of said triangle
            triangle = new PolygonD2D(new List<PointD2D> { new PointD2D(1, 1), new PointD2D(2, 2), new PointD2D(3, 1) });
            point = new PointD2D(2, 1.9);

            Assert.IsTrue(triangle.Contains(point));

            // Another point that lies inside of the triangle
            point = new PointD2D(1.5, 1.25);

            Assert.IsTrue(triangle.Contains(point));
        }
    }
}