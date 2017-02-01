using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;

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
        public PointD2D Position;

        /// <summary>
        /// The rectangle that can be used for intersection. Recall this does not itself
        /// reflect the position.
        /// </summary>
        public RectangleD2D CollisionMesh;

        protected Tile(PointD2D position, RectangleD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft);
    }
}
