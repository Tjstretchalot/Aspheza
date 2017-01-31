using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.WorldObject.Entities
{
    /// <summary>
    /// An entity is anything that isn't a tile.
    /// </summary>
    /// <remarks>
    /// Entities have a position and a collision mesh. Like a vertex mesh in 3D games is a collection
    /// of polygons, the CollisionMesh is the 2D equivalent (a collection of lines). If Entities are 
    /// bounded and convex and do not have any round sections, which we will assume is true, a collection
    /// of lines is simply a polygon.
    /// 
    /// Furthermore, entities are able to render themselves.
    /// </remarks>
    public abstract class Entity : Renderable
    {
        /// <summary>
        /// Where the entity is located in the world.
        /// </summary>
        public PointD2D Position { get; protected set; }

        /// <summary>
        /// The polygon that can be used for intersection. Recall this does not itself
        /// reflect the position.
        /// </summary>
        public PolygonD2D CollisionMesh { get; protected set; }

        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft);

        protected void Init(PointD2D position, PolygonD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }
    }
}
