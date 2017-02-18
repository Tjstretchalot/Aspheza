﻿using NUnit.Framework;
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

        [Test(Description = "Test that the TilesIntersectedAt handles a unit square correctly")]
        public void TilesIntersectedForUnitSquare()
        {
            var square = PolygonD2D.UnitSquare;

            var tiles = new List<PointI2D>();

            square.TilesIntersectedAt(new PointD2D(0, 0), tiles);

            Assert.AreEqual(1, tiles.Count);
            Assert.AreEqual(0, tiles[0].X);
            Assert.AreEqual(0, tiles[0].Y);

            square.TilesIntersectedAt(new PointD2D(1, 0), tiles);

            Assert.AreEqual(1, tiles.Count);
            Assert.AreEqual(1, tiles[0].X);
            Assert.AreEqual(0, tiles[0].Y);

            square.TilesIntersectedAt(new PointD2D(0, 0.5), tiles);

            Assert.AreEqual(2, tiles.Count);

            var foundOrigin = false;
            var found01 = false;

            foreach (var tile in tiles)
            {
                if (tile.X == 0 && tile.Y == 0)
                {
                    if (foundOrigin)
                        Assert.Fail("Unit square at (0, 0.5) intersects (0, 0) and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (0, 0) twice (no (0, 1))!");
                    foundOrigin = true;
                }
                else if (tile.X == 0 && tile.Y == 1)
                {
                    if (found01)
                        Assert.Fail("Unit square at (0, 0.5) intersects (0, 0) and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (0, 1) twice (no (0, 0))!");
                    found01 = true;
                }else
                {
                    Assert.Fail($"Unit square at (0, 0.5) intersects (0, 0) and (0, 1) but got odd tile from PolygonD2D#TilesIntersectedAt: ({tile.X}, {tile.Y}) (tiles={tiles})");
                }
            }

            square.TilesIntersectedAt(new PointD2D(0, -1), tiles);

            Assert.AreEqual(1, tiles.Count);
            Assert.AreEqual(0, tiles[0].X);
            Assert.AreEqual(-1, tiles[0].Y);

            square.TilesIntersectedAt(new PointD2D(0.5, 0.5), tiles);

            Assert.AreEqual(4, tiles.Count);

            bool[] found = new[] { false, false, false, false };

            foreach(var tile in tiles)
            {
                if(tile.X == 0 && tile.Y == 0)
                {
                    if(found[0])
                        Assert.Fail($"Unit square at (0, 0.5) intersects (0, 0), (1, 0), (1, 1), and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (0, 0) twice! {tiles}");
                    found[0] = true;
                }else if (tile.X == 1 && tile.Y == 0)
                {
                    if (found[1])
                        Assert.Fail($"Unit square at (0, 0.5) intersects (0, 0), (1, 0), (1, 1), and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (1, 0) twice! {tiles}");

                    found[1] = true;
                }
                else if (tile.X == 1 && tile.Y == 1)
                {

                    if (found[2])
                        Assert.Fail($"Unit square at (0, 0.5) intersects (0, 0), (1, 0), (1, 1), and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (1, 1) twice! {tiles}");

                    found[2] = true;
                }
                else if (tile.X == 0 && tile.Y == 1)
                {

                    if (found[3])
                        Assert.Fail($"Unit square at (0, 0.5) intersects (0, 0), (1, 0), (1, 1), and (0, 1) but the result from PolygonD2D#TilesIntersectedAt contained (0, 1) twice! {tiles}");

                    found[3] = true;
                }else
                {
                    Assert.Fail($"Unit square at (0.5, 0.5) intersects (0, 0), (0, 1), (1, 0), and (1, 1) but got odd tile from PolygonD2D#TilesIntersectedAt: ({tile.X}, {tile.Y}) (tiles={tiles})");
                }
            }
        }
    }
}