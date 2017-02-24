using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Engine.World.Tiles
{
    /// <summary>
    /// The background of the world consists of tiles. Unlike entities, tiles
    /// must use rectangles as their bounding box.
    /// </summary>
    public abstract class Tile : Renderable
    {
        /// <summary>
        /// Where this tile is located
        /// </summary>
        public PointI2D Position;

        /// <summary>
        /// The rectangle that can be used for intersection. Recall this does not itself
        /// reflect the position.
        /// </summary>
        public RectangleD2D CollisionMesh;
        
        protected Tile(PointI2D position, RectangleD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// Called when the world is done loading.
        /// </summary>
        /// <param name="context">The context</param>
        public virtual void Loaded(UpdateContext context)
        {
        }

        /// <summary>
        /// Called if an adjacent tile changed.
        /// </summary>
        /// <param name="context">The update context</param>
        public virtual void AdjacentTileChanged(UpdateContext context)
        {
        }

        public Tile GetTileFromRelative(UpdateContext context, int rx, int ry)
        {
            if (!context.World.ContainsTile(Position.X + rx, Position.Y + ry))
                return null;

            return context.World.TileAt(Position.X + rx, Position.Y + ry);
        }
        


        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft, Color overlay);
    }
}
