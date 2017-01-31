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
    public class FiniteLineD2DTests
    {
        [Test(Description = "Ensures that Intersects returns false if the two lines are parallel and do not intersect")]
        public void IntersectsFalseForParallelNotIntersectTest()
        {
            // Parallel but shifted and stretched.
            var line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(1, 1));
            var line2 = new FiniteLineD2D(new PointD2D(-2, -1), new PointD2D(1, 2));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // Parallel and shifted along axis and stretched
            line2 = new FiniteLineD2D(new PointD2D(-3, -3), new PointD2D(-1, -1));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // Horizontal lines
            line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(1, 0));
            line2 = new FiniteLineD2D(new PointD2D(0, 1), new PointD2D(1, 1));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // Vertical lines
            line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(0, 1));
            line2 = new FiniteLineD2D(new PointD2D(1, 0), new PointD2D(1, 1));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
        }

        [Test(Description = "Ensures that Intersects returns false if two non-parallel lines do not intersect")]
        public void IntersectsFalseForNonParallelNotIntersectTest()
        {
            // Two standard lines
            var line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(1, 1));
            var line2 = new FiniteLineD2D(new PointD2D(2, 2), new PointD2D(1, 3));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One horizontal
            line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(2, 0));
            line2 = new FiniteLineD2D(new PointD2D(1, 1), new PointD2D(2, 2));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One vertical
            line1 = new FiniteLineD2D(new PointD2D(2, 1), new PointD2D(2, 2));
            line2 = new FiniteLineD2D(new PointD2D(3, 3), new PointD2D(4, 1));

            Assert.IsFalse(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsFalse(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
        }
        
        [Test(Description = "Ensure Intersects returns true for parallel, overlapping lines")]
        public void IntersectsTrueForParallelOverlappingLines()
        {
            // Two standard lines
            var line1 = new FiniteLineD2D(new PointD2D(0, 0), new PointD2D(2, 2));
            var line2 = new FiniteLineD2D(new PointD2D(-1, -1), new PointD2D(1, 1));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One line completely overlaps the other line
            line1 = new FiniteLineD2D(new PointD2D(0, 1), new PointD2D(2, 2));
            line2 = new FiniteLineD2D(new PointD2D(-2, 0), new PointD2D(4, 3));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // Both lines horizontal
            line1 = new FiniteLineD2D(new PointD2D(2, 1), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(1, 1), new PointD2D(3, 1));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // Both lines vertical
            line1 = new FiniteLineD2D(new PointD2D(-2, 1), new PointD2D(-2, 3));
            line2 = new FiniteLineD2D(new PointD2D(-2, 2), new PointD2D(-2, 4));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
        }

        [Test(Description = "Ensure that Intersects returns true for non-parallel lines that intersect strictly")]
        public void IntersectsTrueForNonParallelThroughAndThroughLines()
        {
            // Two standard lines
            var line1 = new FiniteLineD2D(new PointD2D(2, 1), new PointD2D(4, 3));
            var line2 = new FiniteLineD2D(new PointD2D(4, 2), new PointD2D(1, 3));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One horizontal
            line1 = new FiniteLineD2D(new PointD2D(-1, 1), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(-2, 2), new PointD2D(3, 0));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One vertical, one horizontal
            line1 = new FiniteLineD2D(new PointD2D(-1, 1), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(3, 2), new PointD2D(3, 0));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);

            // One vertical
            line1 = new FiniteLineD2D(new PointD2D(1, 0), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(3, 2), new PointD2D(3, 0));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
        }

        [Test(Description = "Ensure Intersects handles strict parameter for parallel lines")]
        public void IntersectsHandlesStrictForParallelLines()
        {
            // Two standard lines
            var line1 = new FiniteLineD2D(new PointD2D(2, 1), new PointD2D(4, 3));
            var line2 = new FiniteLineD2D(new PointD2D(4, 3), new PointD2D(5, 4));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);
            
            // Two horizontal lines
            line1 = new FiniteLineD2D(new PointD2D(-1, -1), new PointD2D(2, -1));
            line2 = new FiniteLineD2D(new PointD2D(-2, -1), new PointD2D(-1, -1));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);

            // Two vertical lines
            line1 = new FiniteLineD2D(new PointD2D(3, -1), new PointD2D(3, 1));
            line2 = new FiniteLineD2D(new PointD2D(3, -1), new PointD2D(3, -3));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);
        }

        [Test(Description = "Ensure Intersects handles strict for non-parallel lines")]
        public void IntersectsHandlesStrictForNonParallelLines()
        {
            // Two standard lines
            var line1 = new FiniteLineD2D(new PointD2D(2, 1), new PointD2D(4, 3));
            var line2 = new FiniteLineD2D(new PointD2D(4, 2), new PointD2D(3.5, 2.5));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);

            // One horizontal
            line1 = new FiniteLineD2D(new PointD2D(-1, 1), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(-2, 2), new PointD2D(3, 1));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);

            // One vertical, one horizontal
            line1 = new FiniteLineD2D(new PointD2D(-1, 1), new PointD2D(4, 1));
            line2 = new FiniteLineD2D(new PointD2D(3, 2), new PointD2D(3, 1));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);

            // One vertical
            line1 = new FiniteLineD2D(new PointD2D(1, 0), new PointD2D(3, 1));
            line2 = new FiniteLineD2D(new PointD2D(3, 2), new PointD2D(3, 0));

            Assert.IsTrue(line1.Intersects(line2), "{0}.Intersects({1})", line1, line2);
            Assert.IsTrue(line2.Intersects(line1), "{0}.Intersects({1})", line2, line1);
            Assert.IsFalse(line1.Intersects(line2, strict: true), "{0}.Intersects({1}, strict: true)", line1, line2);
            Assert.IsFalse(line2.Intersects(line1, strict: true), "{0}.Intersects({1}, strict: true)", line2, line1);
        }
    }
}