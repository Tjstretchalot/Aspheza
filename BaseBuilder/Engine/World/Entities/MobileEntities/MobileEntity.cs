using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public abstract class MobileEntity : Entity
    {
        public double SpeedUnitsPerMS;
        public SpriteSheetAnimationRenderer AnimationRenderer { get; protected set; }

        public MobileEntity(PointD2D position, CollisionMeshD2D collisionMesh, int id, double speed) : base(position, collisionMesh, id)
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
