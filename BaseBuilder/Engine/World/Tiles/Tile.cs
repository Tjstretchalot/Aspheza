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
using Lidgren.Network;
using BaseBuilder.Engine.State;

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

        public string HoverText { get { return null; } }

        protected Tile(PointI2D position, RectangleD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// Writes any information besides tile position, type, and collision message to the message to be called
        /// with the (PointI2D position, RectangleD2D collisionMesh, NetIncomingMessage message
        /// </summary>
        /// <param name="message"></param>
        public abstract void Write(NetOutgoingMessage message);

        /// <summary>
        /// Called when the world is done loading.
        /// </summary>
        /// <param name="context">The context</param>
        public virtual void Loaded(SharedGameState gameState)
        {
        }

        /// <summary>
        /// Called if an adjacent tile changed.
        /// </summary>
        /// <param name="context">The update context</param>
        public virtual void AdjacentTileChanged(UpdateContext context)
        {
        }

        public Tile GetTileFromRelative(TileWorld world, int rx, int ry)
        {
            if (!world.ContainsTile(Position.X + rx, Position.Y + ry))
                return null;

            return world.TileAt(Position.X + rx, Position.Y + ry);
        }
        


        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft, Color overlay);

        /// <summary>
        /// Should be called every frame to update animations
        /// </summary>
        /// <param name="context">the context</param>
        public virtual void Update(UpdateContext context) { }
    }
}
