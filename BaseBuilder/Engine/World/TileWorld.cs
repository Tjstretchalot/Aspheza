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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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
    public class TileWorld
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
        public TileWorld(int width, int height, List<Tile> tiles)
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
        public void Render(RenderContext context)
        {
            int leftMostVisibleTileX = (int)(context.Camera.WorldTopLeft.X);
            int topMostVisibleTileY = (int)(context.Camera.WorldTopLeft.Y);
            int rightMostVisibleTileX = (int)Math.Ceiling(context.Camera.WorldTopLeft.X + context.Camera.VisibleWorldWidth);
            int bottomMostVisibleTileY = (int)Math.Ceiling(context.Camera.WorldTopLeft.Y + context.Camera.VisibleWorldHeight);

            double startingLeft, startingTop;
            context.Camera.PixelLocationOfWorld(leftMostVisibleTileX, topMostVisibleTileY, out startingLeft, out startingTop);

            startingLeft = Math.Round(startingLeft);
            startingTop = Math.Round(startingTop);
            var point = new PointD2D(startingLeft, startingTop);
            for(int x = leftMostVisibleTileX; x <= rightMostVisibleTileX; x++)
            {
                for(int y = topMostVisibleTileY; y <= bottomMostVisibleTileY; y++)
                {
                    TileAt(x, y).Render(context, point);
                    point.Y += context.Camera.Zoom;
                }

                point.Y = startingTop;
                point.X += context.Camera.Zoom;
            }

            foreach(var immobile in ImmobileEntities)
            {
                context.Camera.PixelLocationOfWorld(immobile.Position.X, immobile.Position.Y, out point.X, out point.Y);

                immobile.Render(context, point);
            }

            foreach(var mobile in MobileEntities)
            {
                context.Camera.PixelLocationOfWorld(mobile.Position.X, mobile.Position.Y, out point.X, out point.Y);

                mobile.Render(context, point);
            }

            context.SpriteBatch.DrawString(context.Content.Load<SpriteFont>("Arial"), $"tiles: ({leftMostVisibleTileX}, {topMostVisibleTileY}) to ({rightMostVisibleTileX}, {bottomMostVisibleTileY}); starting = {startingLeft}, {startingTop}", new Microsoft.Xna.Framework.Vector2(5, 25), Color.White);
        }

        /// <summary>
        /// Called when loading is complete.
        /// </summary>
        /// <param name="context">The context</param>
        public void LoadingDone(UpdateContext context)
        {
            foreach(var tile in Tiles)
            {
                tile.Loaded(context);
            }
        }

        /// <summary>
        /// Updates the world in the given context
        /// </summary>
        /// <param name="context">The context</param>
        public void Update(UpdateContext context)
        {

        }

        public Tile TileAt(int x, int y)
        {
            return Tiles[x + y * TileHeight];
        }
    }
}
