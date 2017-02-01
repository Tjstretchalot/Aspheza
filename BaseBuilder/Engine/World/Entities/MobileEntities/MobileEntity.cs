using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public abstract class MobileEntity : SpriteEntity
    {
        public MobileEntity(PointD2D position, PolygonD2D collisionMesh, string spriteName) : base(position, collisionMesh, spriteName)
        {
        }
    }
}
