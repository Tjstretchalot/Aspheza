using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using Lidgren.Network;
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
        public int ID { get; protected set; }
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
        /// Is this entity currently selected
        /// </summary>
        public bool Selected;

        protected Entity(PointD2D position, PolygonD2D collisionMesh, int id)
        {
            Position = position;
            CollisionMesh = collisionMesh;
            ID = id;
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        protected Entity()
        {
        }
        
        public virtual void FromMessage(NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            CollisionMesh = new PolygonD2D(message);
            ID = message.ReadInt32();
        }

        public virtual void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            CollisionMesh.Write(message);
            message.Write(ID);
        }

        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft);

        /// <summary>
        /// Updates this entity in the given context. Note that this should only do visual
        /// related updates, like ticking an animation. 
        /// </summary>
        /// <param name="context"></param>
        public virtual void Update(UpdateContext context)
        {
        }

        protected void Init(PointD2D position, PolygonD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// Returns true if this Entity contains the specified point. Returns false otherwise.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns></returns>
        public bool Contains(PointD2D point, bool strict = false)
        {
            return CollisionMesh.Contains(point, Position, strict);
        }
    }
}
