using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities.Animations
{
    public class Animation
    {
        protected List<AnimationFrame> AnimationFrames;
        public int CurrentFrame;

        public Animation(string sourceFile, List<Rectangle> sourceRecs, List<PointD2D> keyPixels, List<int> displayTimes)
        {
            if (sourceRecs.Count != keyPixels.Count || keyPixels.Count != displayTimes.Count)
                throw new Exception("Lists do not match");

            AnimationFrames = new List<AnimationFrame>();
            for (int count = 0; count < keyPixels.Count; count++)
            {
                AnimationFrames.Add(new AnimationFrame(sourceFile, sourceRecs[count], keyPixels[count], displayTimes[count]));
            }
            if (AnimationFrames.Count != keyPixels.Count)
                throw new Exception("Number of animationFrames not correct");
            CurrentFrame = 0;
        }

        public Animation(string sourceFile, List<Tuple<Rectangle, PointD2D, int>> frameInfos)
        {
            for (int count = 0; count < frameInfos.Count; count++)
            {
                AnimationFrames.Add(new AnimationFrame(sourceFile, frameInfos[count].Item1, frameInfos[count].Item2, frameInfos[count].Item3));
            }
            CurrentFrame = 0;
        }

        public int Count()
        {
            return AnimationFrames.Count;
        }

        public int GetFrameTime()
        {
            return AnimationFrames[CurrentFrame].DisplayTime;
        }

        public Tuple<Texture2D, Rectangle, PointD2D> GetFrameRenderInfo(RenderContext context)
        {
            return AnimationFrames[CurrentFrame].GetFrameRenderInfo(context);
        }
    }
}
