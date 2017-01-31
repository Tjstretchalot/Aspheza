using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.Utility;

namespace BaseBuilder.Engine.World
{ 
    /// <summary>
    /// <para>
    /// The world consists of a rectangular tile map on which entities and environments are placed.
    /// </para>
    /// 
    /// <para>
    /// The world is optimized for (relatively) slow entity movement in return for (relatively) fast
    /// collision detection. That is to say, all tile-collisions are determined and updated whenever
    /// entities or environments move such that they do not need to be calculated again until the entity
    /// or environment moves.
    /// </para>
    /// </summary>
    public class World
    {
        public int TileWidth { get; }
        public int TileHeight { get; }

        /// <summary>
        /// Index 0-TileWidth-1 is the first row left-right, etc.
        /// </summary>
        protected List<Tile> Tiles;

        protected List<MobileEntity> MobileEntities;
        protected List<ImmobileEntity> ImmobileEntities;

        /// <summary>
        /// Initializes a new world that is the specified number of tiles wide and 
        /// specified number of tiles tall.
        /// </summary>
        /// <param name="width">Width in tiles.</param>
        /// <param name="height">Height in tiles.</param>
        public World(int width, int height, List<Tile> tiles)
        {
            if (tiles == null)
                throw new ArgumentNullException(nameof(tiles));
            if (width * height != tiles.Count)
                throw new InvalidProgramException($"World of width {width} and height {height} should have {width * height} tiles, but it has {tiles.Count} tiles!");

            TileWidth = width;
            TileHeight = height;
            
            Tiles = tiles;
            MobileEntities = new List<MobileEntity>();
            ImmobileEntities = new List<ImmobileEntity>();
        }

        /// <summary>
        /// Renders the world in the given context.
        /// </summary>
        /// <param name="context">Render context</param>
        /// <param name="boundsScreen">The screen bounds of the world.</param>
        /// <param name="cameraScreen">The camera position (top-left)</param>
        public void Render(RenderContext context, RectangleD2D boundsScreen, PointD2D cameraScreen)
        {
            var deltaScreenX = boundsScreen.Left - cameraScreen.X;
            var deltaScreenY = boundsScreen.Top - cameraScreen.Y;
            
            int leftMostVisibleTileX = (int)(cameraScreen.X / CameraZoom.SCREEN_OVER_WORLD);
            int topMostVisibleTileY = (int)(cameraScreen.Y / CameraZoom.SCREEN_OVER_WORLD);
            int rightMostVisibleTileX = (int)Math.Ceiling((cameraScreen.X + boundsScreen.Width) / CameraZoom.SCREEN_OVER_WORLD);
            int bottomMostVisibleTileY = (int)Math.Ceiling((cameraScreen.Y + boundsScreen.Height) / CameraZoom.SCREEN_OVER_WORLD);

            var point = new PointD2D(deltaScreenX, deltaScreenY);
            for(int x = leftMostVisibleTileX; x <= rightMostVisibleTileX; x++)
            {
                for(int y = topMostVisibleTileY; y <= bottomMostVisibleTileY; y++)
                {
                    TileAt(x, y).Render(context, point);
                    point.Y += CameraZoom.SCREEN_OVER_WORLD;
                }

                point.Y = deltaScreenY;
                point.X += CameraZoom.SCREEN_OVER_WORLD;
            }

            foreach(var immobile in ImmobileEntities)
            {
                point.X = immobile.Position.X + deltaScreenX;
                point.Y = immobile.Position.Y + deltaScreenY;

                immobile.Render(context, point);
            }

            foreach(var mobile in MobileEntities)
            {
                point.X = mobile.Position.X + deltaScreenX;
                point.Y = mobile.Position.Y + deltaScreenY;

                mobile.Render(context, point);
            }
        }

        public Tile TileAt(int x, int y)
        {
            return Tiles[x + y * TileHeight];
        }
    }
}
