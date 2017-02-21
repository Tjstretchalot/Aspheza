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
        public double SpeedUnitsPerMS;

        public MobileEntity(PointD2D position, PolygonD2D collisionMesh, int id, string spriteName, double speed) : base(position, collisionMesh, id, spriteName)
        {
            SpeedUnitsPerMS = speed;
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public MobileEntity() : base()
        {
        }
    }
}
