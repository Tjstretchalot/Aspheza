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
    public class AnimationFrame
    {
        string SourceFile;
        Texture2D Texture;
        Rectangle SourceRec;
        PointD2D KeyPixel;
        public int DisplayTime;
        
        public AnimationFrame(string sourceFile, Rectangle sourceRec, PointD2D keyPixel, int displayTime)
        {
            SourceFile = sourceFile;
            SourceRec = sourceRec;
            
            KeyPixel = keyPixel;
            DisplayTime = displayTime;
        }

        public AnimationFrame(string sourceFile, Texture2D texture, Rectangle sourceRec, PointD2D keyPixel, int displayTime)
        {
            SourceFile = sourceFile;
            Texture = texture;
            SourceRec = sourceRec;

            KeyPixel = keyPixel;
            DisplayTime = displayTime;


        }
        
        public Tuple<Texture2D, Rectangle, PointD2D> GetFrameRenderInfo(RenderContext context)
        {
            if (Texture == null)
                Texture = context.Content.Load<Texture2D>(SourceFile);

            return new Tuple<Texture2D, Rectangle, PointD2D>(Texture, SourceRec, KeyPixel);
        }
    }
}
