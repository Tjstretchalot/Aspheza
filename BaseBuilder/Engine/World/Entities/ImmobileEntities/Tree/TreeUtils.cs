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
        private static Tuple<CollisionMeshD2D, List<Tuple<Rectangle, PointD2D>>> SaplingConstructorParams;
        private static Dictionary<Tuple<TreeSize, TreeStyle, TreeColor>, Tuple<CollisionMeshD2D, List<Tuple<Rectangle, PointD2D>>>> TreeTypeToConstructorParams;
        
        static TreeUtils()
        {
            var largeMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 2) });
            var smallMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });

            SaplingConstructorParams = Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(66, 0, 32, 32), new PointD2D(0, 0)) });
            TreeTypeToConstructorParams = new Dictionary<Tuple<TreeSize, TreeStyle, TreeColor>, Tuple<CollisionMeshD2D, List<Tuple<Rectangle, PointD2D>>>>()
            {
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Green), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(272, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Red), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(289, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Pointy, TreeColor.Blue), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(306, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Green), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(221, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Red), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(238, 153, 16, 16), new PointD2D(0, 0)) }) },
                { Tuple.Create(TreeSize.Small, TreeStyle.Rounded, TreeColor.Blue), Tuple.Create(smallMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(255, 153, 16, 16), new PointD2D(0, 0)) }) },
                
                
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Green), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(272, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(272, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Red), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(289, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(289, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Pointy, TreeColor.Blue), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(306, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(306, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Green), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(221, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(221, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Red), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(238, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(238, 187, 16, 16), new PointD2D(0, 1)) }) },
                { Tuple.Create(TreeSize.Large, TreeStyle.Rounded, TreeColor.Blue), Tuple.Create(largeMesh, new List<Tuple<Rectangle, PointD2D>> { Tuple.Create(new Rectangle(255, 170, 16, 16), new PointD2D(0, 0)), Tuple.Create(new Rectangle(255, 187, 16, 16), new PointD2D(0, 1)) }) },
                };
            
        }

        public static Tuple<CollisionMeshD2D, List<Tuple<Rectangle, PointD2D>>> GetCollisionMesh(TreeSize size, TreeStyle style, TreeColor color)
        {
            if (size == TreeSize.Sapling)
                return SaplingConstructorParams;

            return TreeTypeToConstructorParams[Tuple.Create(size, style, color)];
        }
    }
}
