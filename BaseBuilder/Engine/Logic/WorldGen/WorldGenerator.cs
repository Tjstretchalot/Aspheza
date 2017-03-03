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

        public SharedGameState SharedGameState;
        public LocalGameState LocalGameState;

        public WorldGenerator()
        {

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

                    if (x != 128 && x != 129 && x != 130)
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
            TileWorld.AddMobileEntity(new OverseerMage(new PointD2D(wcx + 0, wcy), SharedGameState.GetUniqueEntityID()));

            var tmp = new CaveManWorker(new PointD2D(wcx - 2, wcy + 0), SharedGameState.GetUniqueEntityID());
            tmp.Inventory.AddMaterial(Material.WheatSeed, 1);
            tmp.Inventory.AddMaterial(Material.CarrotSeed, 1);
            TileWorld.AddMobileEntity(tmp);
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx - 2, wcy + 1), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx - 1, wcy + 2), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx + 0, wcy + 2), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx + 1, wcy + 2), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx + 2, wcy + 1), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddMobileEntity(new CaveManWorker(new PointD2D(wcx + 2, wcy + 0), SharedGameState.GetUniqueEntityID()));
        }

        protected void InitBuildings()
        {
            var wcx = TileWorld.TileWidth / 2;
            var wcy = TileWorld.TileHeight / 2;
            TileWorld.AddImmobileEntity(new House(new PointD2D(wcx - 0.5, wcy - 4), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Sign(new PointD2D(wcx + 5, wcy + 1), SharedGameState.GetUniqueEntityID(), "Welcome to the game!"));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 3, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Green));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 4, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Red));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 5, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Pointy, TreeColor.Blue));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 6, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Green));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 7, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Red));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 8, wcy - 4), SharedGameState.GetUniqueEntityID(), TreeSize.Large, TreeStyle.Rounded, TreeColor.Blue));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 3, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Green));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 4, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Red));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 5, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Pointy, TreeColor.Blue));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 6, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Green));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 7, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Red));
            TileWorld.AddImmobileEntity(TreeUtils.InitTree(new PointD2D(wcx + 8, wcy - 1), SharedGameState.GetUniqueEntityID(), TreeSize.Small, TreeStyle.Rounded, TreeColor.Blue));
            TileWorld.AddImmobileEntity(new GoldOre(new PointD2D(wcx - 10, wcy + 5), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Farm(new PointD2D(wcx - 15, wcy + 5), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Farm(new PointD2D(wcx - 15, wcy + 0), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Farm(new PointD2D(wcx - 15, wcy - 5), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new WaterMill(new PointD2D(wcx - 30, wcy + 5), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Bakery(new PointD2D(wcx - 10, wcy - 15), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Tavern(new PointD2D(wcx + 10, wcy - 15), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Library(new PointD2D(wcx + 30, wcy - 15), SharedGameState.GetUniqueEntityID()));
            TileWorld.AddImmobileEntity(new Temple(new PointD2D(wcx + 40, wcy - 15), SharedGameState.GetUniqueEntityID()));
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

            TileWorld.LoadingDone(SharedGameState);
        }
    }
}
