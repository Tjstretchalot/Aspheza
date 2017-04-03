using BaseBuilder.Engine.Logic.Players;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities;

namespace BaseBuilder.Engine.Logic.WorldGen
{
    // TODO Generation options
    /// <summary>
    /// Creates simple worlds. Used by creating a new world generator, calling create, and then accessing
    /// the public members.
    /// </summary>
    public class WorldGenerator
    {
        protected TileWorld TileWorld;
        protected List<Player> Players;
        protected int LocalPlayerID;

        protected Random RanGen;

        public SharedGameState SharedGameState;
        public LocalGameState LocalGameState;

        public WorldGenerator()
        {
            RanGen = new Random();
        }

        /// <summary>
        /// Creates a tile world. LoadingDone should be called on it before rendering/updating.
        /// </summary>
        /// <returns>A tile world 120x80</returns>
        protected virtual TileWorld CreateWorld()
        {
            const int width = 300;
            const int height = 200;

            var random = new Random();
            var tiles = new List<Tile>(width * height);

            var tileCollisionMesh = PolygonD2D.UnitSquare;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var point = new PointI2D(x, y);

                    if (x != 200 && x != 201 && x != 202)
                        tiles.Add(new GrassTile(point, tileCollisionMesh));
                    else
                        tiles.Add(new WaterTile(point, tileCollisionMesh, Direction.Down));

                }
            }

            return new TileWorld(width, height, tiles);
        }

        protected virtual List<Player> InitPlayers()
        {
            LocalPlayerID = 1;

            var result = new List<Player>();
            result.Add(new Player(LocalPlayerID, "Host"));
            return result;
        }

        protected void InitOverseers()
        {
            var wcx = TileWorld.TileWidth / 2;
            var wcy = TileWorld.TileHeight / 2;
            var tmp1 = new OverseerMage(new PointD2D(wcx - 0, wcy + 0), SharedGameState.EntityIDCounter++);
#if DEBUG
            tmp1.Inventory.AddMaterial(Material.Wheat, 6);
#endif
            TileWorld.AddMobileEntityImpl(tmp1);

            var tmp = new CaveManWorker(new PointD2D(wcx - 2, wcy + 0), SharedGameState.EntityIDCounter++);
            tmp.Inventory.AddMaterial(Material.WheatSeed, 1);
            tmp.Inventory.AddMaterial(Material.CarrotSeed, 1);
            tmp.Inventory.AddMaterial(Material.Sugarcane, 1);
            // Rum ingredients
            //tmp.Inventory.AddMaterial(Material.Sugarcane, 3);

            // Bread ingredients
            //tmp.Inventory.AddMaterial(Material.Wheat, 1);
            //tmp.Inventory.AddMaterial(Material.Sugar, 1);
            //tmp.Inventory.AddMaterial(Material.Egg, 1);

            // Coop ingredients
            tmp.Inventory.AddMaterial(Material.Chicken, 1);

            TileWorld.AddMobileEntityImpl(tmp);
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx - 2, wcy + 1), SharedGameState.EntityIDCounter++));
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx - 1, wcy + 2), SharedGameState.EntityIDCounter++));
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx + 0, wcy + 2), SharedGameState.EntityIDCounter++));
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx + 1, wcy + 2), SharedGameState.EntityIDCounter++));
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx + 2, wcy + 1), SharedGameState.EntityIDCounter++));
            TileWorld.AddMobileEntityImpl(new CaveManWorker(new PointD2D(wcx + 2, wcy + 0), SharedGameState.EntityIDCounter++));
        }

        protected void InitBuildings()
        {
            var wcx = TileWorld.TileWidth / 2;
            var wcy = TileWorld.TileHeight / 2;

            TileWorld.AddImmobileEntityImpl(new House(new PointD2D(wcx - 0.5, wcy - 4), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new Sign(new PointD2D(wcx + 5, wcy + 1), SharedGameState.EntityIDCounter++, "Welcome to the game!"));
            TileWorld.AddImmobileEntityImpl(new GoldOre(new PointD2D(wcx - 10, wcy + 5), SharedGameState.EntityIDCounter++));
            /*
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 3, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Green));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 4, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Red));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 5, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Blue));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 6, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Green));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 7, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Red));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 8, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Blue));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 3, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Green));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 4, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Red));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 5, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Blue));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 6, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Green));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 7, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Red));
            TileWorld.AddImmobileEntityImpl(TreeUtils.InitTree(new PointD2D(wcx + 8, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Blue));
            
            TileWorld.AddImmobileEntityImpl(new Farm(new PointD2D(wcx - 15, wcy + 5), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new Farm(new PointD2D(wcx - 15, wcy + 0), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new Farm(new PointD2D(wcx - 15, wcy - 5), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new WaterMill2(new PointD2D(wcx - 30, wcy + 5), SharedGameState.EntityIDCounter++));*/
            //TileWorld.AddImmobileEntityImpl(new Bakery(new PointD2D(wcx - 10, wcy - 15), SharedGameState.EntityIDCounter++));
            //TileWorld.AddImmobileEntityImpl(new Tavern2(new PointD2D(wcx + 10, wcy - 15), SharedGameState.EntityIDCounter++));
            /*TileWorld.AddImmobileEntityImpl(new Library(new PointD2D(wcx + 30, wcy - 15), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new Temple(new PointD2D(wcx + 40, wcy - 15), SharedGameState.EntityIDCounter++));
            TileWorld.AddImmobileEntityImpl(new LumberMill(new PointD2D(wcx + 10, wcy - 0), SharedGameState.EntityIDCounter++));
            */
        }

        private void TrySpawnTree(PointI2D pos, ref ImmobileEntity nextTree)
        {
            TreeSize size;
            TreeStyle style;
            TreeColor color;

            var tilePositions = new List<PointI2D>();

            bool badSpot = false;
            foreach (var confl in TileWorld.GetEntitiesAtLocation(nextTree.CollisionMesh, pos))
            {
                badSpot = true;
                break;
            }
            if (badSpot)
                return;

            nextTree.CollisionMesh.TilesIntersectedAt(pos, tilePositions);
            foreach (var tilePos in tilePositions)
            {
                if (!TileWorld.TileAt(tilePos.X, tilePos.Y).Ground)
                {
                    badSpot = true;
                    break;
                }
            }
            if (badSpot)
                return;

            nextTree.Position = new PointD2D(pos.X, pos.Y);
            TileWorld.AddImmobileEntityImpl(nextTree);

            var tmp = RanGen.Next(3);
            switch (tmp)
            {
                case 0:
                    size = TreeSize.Large;
                    break;
                case 1:
                    size = TreeSize.Small;
                    break;
                case 2:
                    size = TreeSize.Sapling;
                    break;
                default:
                    throw new InvalidProgramException();
            }

            tmp = RanGen.Next(2);
            switch (tmp)
            {
                case 0:
                    style = TreeStyle.Pointy;
                    break;
                case 1:
                    style = TreeStyle.Rounded;
                    break;
                default:
                    throw new InvalidProgramException();
            }

            tmp = RanGen.Next(100);
            if (tmp < 85)
                color = TreeColor.Green;
            else if (tmp < 95)
                color = TreeColor.Red;
            else
                color = TreeColor.Blue;

            nextTree = new Tree(new PointD2D(0, 0), SharedGameState.EntityIDCounter++, size, style, color);
        }
        
        protected void SpawnTreeEpitrochoid(PointI2D pos, int x, int y, ref ImmobileEntity nextTree, double a, double b, double c, double scale)
        {
            for (var t = -10.0; t <= 10.0; t += 0.01)
            {
                var tx2 = (a + b) * Math.Cos(t) - c * Math.Cos(((a / b) + 1) * t);
                var ty2 = (a + b) * Math.Sin(t) - c * Math.Sin(((a / b) + 1) * t);

                var x2 = (tx2 * scale) + x;
                var y2 = (ty2 * scale) + y;

                if (x2 < 5 || y2 < 5 || x2 >= TileWorld.TileWidth - 5 || y2 >= TileWorld.TileHeight - 5)
                    continue;

                pos.X = (int)Math.Round(x2);
                pos.Y = (int)Math.Round(y2);
                TrySpawnTree(pos, ref nextTree);
            }
        }

        protected void SpawnTreeEllipse(PointI2D pos, int cx, int cy, ref ImmobileEntity nextTree, int width, int height)
        {
            width = width / 2;
            height = height / 2;
            int hh = height * height;
            int ww = width * width;
            int hhww = hh * ww;
            int x0 = width;
            int dx = 0;

            // do the horizontal diameter
            for (int x = -width; x <= width; x++)
            {
                pos.X = cx + x;
                pos.Y = cy;
                if (RanGen.NextDouble() < 0.2)
                    TrySpawnTree(pos, ref nextTree);
            }

            // now do both halves at the same time, away from the diameter
            for (int y = 1; y <= height; y++)
            {
                int x1 = x0 - (dx - 1);  // try slopes of dx - 1 or more
                for (; x1 > 0; x1--)
                    if (x1 * x1 * hh + y * y * ww <= hhww)
                        break;
                dx = x0 - x1;  // current approximation of the slope
                x0 = x1;

                for (int x = -x0; x <= x0; x++)
                {
                    pos.X = cx + x;
                    pos.Y = cy - y;
                    if (RanGen.NextDouble() < 0.2)
                        TrySpawnTree(pos, ref nextTree);

                    pos.X = cx + x;
                    pos.Y = cy + y;
                    if (RanGen.NextDouble() < 0.2)
                        TrySpawnTree(pos, ref nextTree);
                }
            }
        }

        protected void SpawnTreeSetup1(PointI2D pos, int x, int y, double desWidth, double desHeight, ref ImmobileEntity nextTree)
        {
            var a = 1.0;
            var b = 3.0 / 5.0;
            var c = 1.2;
            var scale = desWidth / 7.0;

            SpawnTreeEpitrochoid(pos, x, y, ref nextTree, a, b, c, scale);
        }
        
        protected void InitTrees()
        {
            const int forestAvgWidth = 20;
            const int forestAvgHeight = 20;

            var wcx = TileWorld.TileWidth / 2;
            var wcy = TileWorld.TileHeight / 2;
            
            ImmobileEntity nextTree = new Tree(new PointD2D(wcx + 3, wcy - 4), SharedGameState.EntityIDCounter++, TreeSize.Large, TreeStyle.Pointy, TreeColor.Green);
            var pos = new PointI2D(0, 0);
            
            for (int y = forestAvgHeight; y < TileWorld.TileHeight - forestAvgHeight; y++)
            {
                for (int x = forestAvgWidth; x < TileWorld.TileWidth - forestAvgWidth; x++)
                {
                    var distToCenterManh = Math.Abs(x - wcx) + Math.Abs(y - wcy);
                    if (distToCenterManh < 50)
                        continue;

                    if (RanGen.NextDouble() < 0.001)
                    {
                        var desiredWidth = (forestAvgWidth / 2 + RanGen.Next(forestAvgWidth));
                        var desiredHeight = (forestAvgHeight / 2 + RanGen.Next(forestAvgHeight));

                        SpawnTreeEllipse(pos, x, y, ref nextTree, desiredWidth, desiredHeight);
                    }
                }
            }
        }
        
        public void Create(GraphicsDevice graphicsDevice)
        {
            var screenSize = graphicsDevice.Viewport;
            
            TileWorld = CreateWorld();
            Players = InitPlayers();

            SharedGameState = new SharedGameState(TileWorld, Players, 0);
            LocalGameState = new LocalGameState(new Camera(new PointD2D(0, 0), new RectangleD2D(screenSize.Width, screenSize.Height), 8), LocalPlayerID);
            
            InitOverseers();
            InitBuildings();
            InitTrees();

            TileWorld.LoadingDone(SharedGameState);
        }
    }
}
