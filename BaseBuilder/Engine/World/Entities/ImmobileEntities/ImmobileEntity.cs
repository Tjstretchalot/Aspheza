using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public abstract class ImmobileEntity : Entity
    {
        public virtual string UnbuiltHoverText { get { return null; }  }

        public ImmobileEntity(PointD2D position, PolygonD2D collisionMesh, int id) : base(position, collisionMesh, id)
        {
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public ImmobileEntity() : base()
        {
        }
    }
}
