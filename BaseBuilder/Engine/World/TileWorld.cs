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
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;

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
        public List<Tile> Tiles;

        public List<MobileEntity> MobileEntities;

        public List<ImmobileEntity> ImmobileEntities;
        
        protected Dictionary<Entity, List<Tile>> EntityToTiles;
        protected Dictionary<Tile, List<Entity>> TileToEntities;

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

            EntityToTiles = new Dictionary<Entity, List<Tile>>();
            TileToEntities = new Dictionary<Tile, List<Entity>>();
            foreach (var tile in Tiles)
            {
                TileToEntities.Add(tile, new List<Entity>());
            }
        }

        /// <summary>
        /// Adds a mobile entity to the world
        /// </summary>
        /// <param name="entity">Mobile entity to add to world</param>
        public void AddMobileEntity(MobileEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            MobileEntities.Add(entity);
            AddTileCollisions(entity);
        }

        /// <summary>
        /// Adds an immobile entity to the world
        /// </summary>
        /// <param name="entity">Immobile entity to add to world</param>
        public void AddImmobileEntity(ImmobileEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            ImmobileEntities.Add(entity);
            AddTileCollisions(entity);
        }
        

        public void AddTileCollisions(Entity ent)
        {
            EntityToTiles.Add(ent, new List<Tile>());

            UpdateTileCollisions(ent);
        }

        protected bool ResolveEntityCollisions(Entity ent, List<PointI2D> newEntTiles)
        {
            foreach(var tilePos in newEntTiles)
            {
                var tile = TileAt(tilePos.X, tilePos.Y);
                var entities = TileToEntities[tile];

                foreach(var colliding in entities)
                {
                    if (colliding == ent)
                        continue;

                    var mtv = ent.CollisionMesh.IntersectionMTV(colliding.CollisionMesh, ent.Position, colliding.Position);

                    if(mtv != null)
                    {
                        ent.Position.X += mtv.DeltaX;
                        ent.Position.Y += mtv.DeltaY;
                        ent.TilesIntersectedAt(newEntTiles);
                        ResolveEntityCollisions(ent, newEntTiles);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the tile collisions for the specified entity. This is required any time an entities
        /// position changes.
        /// </summary>
        /// <param name="ent">The entity that moved</param>
        /// <returns>True if this function moved the entity to resolve a collision, false otherwise.</returns>
        public bool UpdateTileCollisions(Entity ent)
        {
            List<PointI2D> tilesLocation = new List<PointI2D>();
            List<Tile> newTiles = new List<Tile>();
            ent.TilesIntersectedAt(tilesLocation);

            var result = ResolveEntityCollisions(ent, tilesLocation);

            var oldTiles = EntityToTiles[ent];
            var oldTileCount = oldTiles.Count;

            newTiles.AddRange(tilesLocation.Select((t) => TileAt(t.X, t.Y)));

            foreach (var tile in newTiles)
            {
                if(!oldTiles.Contains(tile))
                {
                    oldTiles.Add(tile);
                    TileToEntities[tile].Add(ent);
                }
            }
            for (var loc = oldTileCount - 1; loc >= 0; loc--)
            {
                if(!newTiles.Contains(oldTiles[loc]))
                {
                    TileToEntities[oldTiles[loc]].Remove(ent);
                    oldTiles.RemoveAt(loc);
                }
            }

            return result;
        }

        public bool IsPassable(PointI2D loc, MobileEntity ent)
        {
            return IsPassable(loc.X, loc.Y, ent);
        }

        public bool IsPassable(int locX, int locY, MobileEntity ent)
        {
            var entities = TileToEntities[TileAt(locX, locY)];
            return !entities.Any((e) => e != ent);
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
            Vector2 cSize = Vector2.Zero;
            if (context.CollisionDebug)
            {
                cSize = context.DebugFont.MeasureString("C");
            }
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

            point.Y = startingTop;
            point.X = startingLeft;

            for (int x = leftMostVisibleTileX; x <= rightMostVisibleTileX; x++)
            {
                for (int y = topMostVisibleTileY; y <= bottomMostVisibleTileY; y++)
                {
                    if (context.CollisionDebug && TileToEntities[TileAt(x, y)].Count > 0)
                    {
                        context.SpriteBatch.DrawString(context.DebugFont, "C", new Vector2((int)(point.X + 0.5 * context.Camera.Zoom - cSize.X / 2), (int)(point.Y + 0.5 * context.Camera.Zoom - cSize.Y / 2)), Color.Red);
                    }
                    point.Y += context.Camera.Zoom;
                }
                point.Y = startingTop;
                point.X += context.Camera.Zoom;
            }

            //context.SpriteBatch.DrawString(context.DebugFont, $"tiles: ({leftMostVisibleTileX}, {topMostVisibleTileY}) to ({rightMostVisibleTileX}, {bottomMostVisibleTileY}); starting = {startingLeft}, {startingTop}", new Microsoft.Xna.Framework.Vector2(5, 25), Color.White);
        }


        public void SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            foreach(var mobileEntity in MobileEntities)
            {
                mobileEntity.SimulateTimePassing(gameState, timeMS);
            }

            foreach(var immobileEntity in ImmobileEntities)
            {
                immobileEntity.SimulateTimePassing(gameState, timeMS);
            }
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
        /// Updates the world in the given context. This is for purely visual updates that aren't
        /// yet rendered on the screen - like ticking an animation.
        /// </summary>
        /// <param name="context">The context</param>
        public void Update(UpdateContext context)
        {
            foreach(var e in MobileEntities)
            {
                e.Update(context);
            }

            foreach(var e in ImmobileEntities)
            {
                e.Update(context);
            }
        }

        /// <summary>
        /// Determines if there is a tile at the specified position.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>True if this world has a tile at that position, false otherwise</returns>
        public bool ContainsTile(int x, int y)
        {
            return x >= 0 && x < TileWidth && y >= 0 && y < TileHeight;
        }

        public Tile TileAt(int x, int y)
        {
            return Tiles[x + y * TileWidth];
        }

        /// <summary>
        /// Replaces the worlds tile at tile.Position with tile.
        /// </summary>
        /// <param name="tile">The tile to set</param>
        public void SetTile(UpdateContext context, Tile tile)
        {
            Tiles[tile.Position.X + tile.Position.Y * TileWidth] = tile;

            for(int dx = -1; dx <= 1; dx++)
            {
                for(int dy = -1; dy <= 1; dy++)
                {
                    var x = tile.Position.X + dx;
                    var y = tile.Position.Y + dy;

                    if (ContainsTile(x, y))
                        TileAt(x, y).AdjacentTileChanged(context);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">The location to see if falls within an entity</param>
        /// <returns></returns>
        public Entity GetMobileEntityAtLocation(PointD2D location)
        {
            foreach (var entity in MobileEntities)
            {
                if (entity.Contains(location))
                {
                    return entity;
                }
            }
            return null;
        }
    }
}
