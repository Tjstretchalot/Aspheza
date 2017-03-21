using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;

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

        protected void SyncAnimationFromMessage(NetIncomingMessage message)
        {
            var animType = (AnimationType)message.ReadInt32();
            Direction? direction = null;
            if (message.ReadBoolean())
                direction = (Direction)message.ReadInt32();

            AnimationRenderer.StartAnimation(animType, direction);
        }

        protected void WriteAnimationSync(NetOutgoingMessage message)
        {
            message.Write((int)AnimationRenderer.CurrentAnimationType);

            if(AnimationRenderer.CurrentAnimation.Direction.HasValue)
            {
                message.Write(true);
                message.Write((int)AnimationRenderer.CurrentAnimation.Direction);
            }else
            {
                message.Write(false);
            }
        }
    }
}
