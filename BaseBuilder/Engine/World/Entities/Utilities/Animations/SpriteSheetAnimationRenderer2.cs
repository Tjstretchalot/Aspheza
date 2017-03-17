using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class SpriteSheetAnimationRenderer2
    {
        protected Dictionary<AnimationType, List<Animation2>> TypeToAnimationList;
        
        protected bool StartAnimationBool;
        public Animation2 CurrentAnimation;
        public AnimationType CurrentAnimationType;
        
        public SpriteSheetAnimationRenderer2(Dictionary<AnimationType, List<Animation2>> typeToAnimationList)
        {
            CurrentAnimationType = AnimationType.Idle;

            TypeToAnimationList = typeToAnimationList;

            StartAnimationBool = false;
            CurrentAnimation = TypeToAnimationList[AnimationType.Idle][0];
        }

        public void StartAnimation(AnimationType animationType, Direction? direction)
        {
            CurrentAnimationType = animationType;
            var animationList = TypeToAnimationList[animationType];

            if (animationList.Count == 1)
                CurrentAnimation = animationList[0];
            else
            {
                CurrentAnimation = animationList.Find((anim) => anim.Direction == direction);
            }

            StartAnimationBool = true;
        }

        public void EndAnimation()
        {
            if (CurrentAnimation == null)
                return;

            var direction = CurrentAnimation.Direction;

            // End animation?
            CurrentAnimation.Reset();

            StartAnimation(AnimationType.Idle, direction);
        }
        
        public void Update(int deltaTimeMS)
        {
            CurrentAnimation.Update(deltaTimeMS);
        }

        public void Render(RenderContext context, Color overlay, PointD2D screenTopLeft, int zoomAt1TimeScale)
        {
            if (StartAnimationBool)
            {
                CurrentAnimation.Begin(context);
                StartAnimationBool = false;
            }

            CurrentAnimation.Draw(context, overlay, screenTopLeft, zoomAt1TimeScale);
        }
    }
}
