using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public abstract class ImmobileEntity : Entity
    {
        public virtual string UnbuiltHoverText { get { return null; }  }

        public ImmobileEntity(PointD2D position, CollisionMeshD2D collisionMesh, int id) : base(position, collisionMesh, id)
        {
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public ImmobileEntity() : base()
        {
        }

        /// <summary>
        /// Gets the renderable that should be used while this immobile
        /// entity is building. Should contain Idle, Unbuilt, UnbuiltThirty,
        /// UnbuiltSixty, and UnbuiltNinety
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <returns>In progress renderable</returns>
        public abstract SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content);
    }
}
