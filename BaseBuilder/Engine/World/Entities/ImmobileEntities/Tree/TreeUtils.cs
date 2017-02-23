using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree
{
    public class TreeUtils
    {

        private static Dictionary<Tuple<TreeSize, TreeStyle, TreeColor>, Tuple<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>> TreeTypeToConstructorParams;
        
        static TreeUtils()
        {
            var largeMesh = new RectangleD2D(1, 2);
            var smallMesh = new RectangleD2D(1, 1);

            TreeTypeToConstructorParams = new Dictionary<Tuple<TreeSize, TreeStyle, TreeColor>, Tuple<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>>()
            {
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Green), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(272, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Red), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(289, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Blue), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(306, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Green), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(221, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Red), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(238, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Blue), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(255, 153, 16, 16), new PointD2D(0, 0)) }) },
                
                
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Green), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(272, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(272, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Red), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(289, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(289, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Blue), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(306, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(306, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Green), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(221, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(221, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Red), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(238, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(238, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Blue), Tuple.Create<PolygonD2D, List<Tuple<Rectangle, PointD2D>>>(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(255, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(255, 187, 16, 16), new PointD2D(0, 1)) }) },
                };
            
        }

        /// <summary>
        /// Create a new tree of specified size, style, and color at specified position
        /// </summary>
        /// <param name="position">Top left point for the tree</param>
        /// <param name="id">Unique entity id</param>
        /// <param name="size">Size of the tree to be created; large or small</param>
        /// <param name="style">Style of tree to be created; pointy top or rounded top</param>
        /// <param name="color">Color of the tree to be created; green, red, or blue</param>
        /// <returns></returns>
        public static ImmobileEntity InitTree(PointD2D position, int id, TreeSize size, TreeStyle style, TreeColor color)
        {
            var tree = new List<Tuple<Rectangle, PointD2D>>();
            PolygonD2D collisionMesh;

            var tuple = GetCollisionMesh(size, style, color);
            tree = tuple.Item2;
            collisionMesh = tuple.Item1;

            return new Tree(position, collisionMesh, id, "roguelikeSheet_transparent", tree, size, style, color);
        }

        public static Tuple<PolygonD2D, List<Tuple<Rectangle, PointD2D>>> GetCollisionMesh(TreeSize size, TreeStyle style, TreeColor color)
        {
            return TreeTypeToConstructorParams[Tuple.Create(size, style, color)];
        }
    }
}
