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
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public abstract class MobileEntity : Entity
    {
        private AnimationType StartingAnimationType;
        private Direction? StartingDirection;
        
        public double SpeedUnitsPerMS;
        public SpriteSheetAnimationRenderer AnimationRenderer { get; protected set; }

        public MobileEntity(PointD2D position, CollisionMeshD2D collisionMesh, int id, double speed) : base(position, collisionMesh, id)
        {
            SpeedUnitsPerMS = speed;
            StartingAnimationType = AnimationType.Idle;
            StartingDirection = Direction.Down;
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public MobileEntity() : base()
        {
        }

        protected void SyncAnimationFromMessage(NetIncomingMessage message)
        {
            StartingAnimationType = (AnimationType)message.ReadInt32();
            StartingDirection = null;
            if (message.ReadBoolean())
                StartingDirection = (Direction)message.ReadInt32();
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

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if(AnimationRenderer == null)
            {
                InitRenderer(context);

                AnimationRenderer.StartAnimation(StartingAnimationType, StartingDirection);
            }
        }

        protected abstract void InitRenderer(RenderContext context);
    }
}
