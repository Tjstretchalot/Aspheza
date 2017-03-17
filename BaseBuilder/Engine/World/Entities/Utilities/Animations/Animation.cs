using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities.Animations
{
    public class Animation
    {
        public List<AnimationFrame> Frames { get; protected set; }
        public Direction? Direction { get; protected set; }
        public int CurrentFrame { get; protected set; }

        protected int BeginFrame;
        protected bool Cycling;
        protected int CycleStartFrame;

        protected int TimeUntillNextFrameMS;
        
        public Animation(List<AnimationFrame> frames, Direction? direction, int cycleStartFrame = 0, int beginFrame = 0)
        {
            Frames = frames;
            Direction = direction;

            CycleStartFrame = cycleStartFrame;
            BeginFrame = beginFrame;

            CurrentFrame = BeginFrame;
            Cycling = false;
            TimeUntillNextFrameMS = Frames[CurrentFrame].DisplayTimeMS;
        }

        public void Begin(RenderContext context)
        {
            CurrentFrame = BeginFrame;
            Cycling = false;

            Frames[CurrentFrame].Begin(context);
        }

        public void End(RenderContext context)
        {
        }

        public void Reset()
        {
            CurrentFrame = BeginFrame;
            Cycling = false;
            TimeUntillNextFrameMS = Frames[CurrentFrame].DisplayTimeMS;
        }

        public void Update(int deltaTimeMS)
        {
            TimeUntillNextFrameMS -= deltaTimeMS;
            if (TimeUntillNextFrameMS <= 0)
            {
                CurrentFrame = (CurrentFrame + 1) % Frames.Count;
                if (!Cycling && CurrentFrame == 0)
                    Cycling = true;
                if (CurrentFrame < CycleStartFrame && Cycling)
                    CurrentFrame = CycleStartFrame;

                TimeUntillNextFrameMS = Frames[CurrentFrame].DisplayTimeMS;
            }
        }

        public void Draw(RenderContext context, Color overlay, PointD2D screenTopLeft, int zoomAt1TimeScale)
        {
            Frames[CurrentFrame].Draw(context, overlay, screenTopLeft, zoomAt1TimeScale);
        }
    }
}
