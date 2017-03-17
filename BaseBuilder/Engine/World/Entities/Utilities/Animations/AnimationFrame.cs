using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        protected Texture2D Texture;
        protected SoundEffect SoundEffect;
        public Rectangle SourceRec;
        public PointD2D TopLeftDif;
        public int DisplayTimeMS;

        public AnimationFrame(Texture2D texture, SoundEffect soundEffect, Rectangle sourceRec, PointD2D topLeftDif, int displayTimeMS)
        {
            Texture = texture;
            SoundEffect = soundEffect;
            SourceRec = sourceRec;
            TopLeftDif = topLeftDif;
            DisplayTimeMS = displayTimeMS;
        }

        public void Begin(RenderContext context)
        {
            //play sound
        }

        public void End(RenderContext context)
        {
        }

        public void Draw(RenderContext context, Color overlay, PointD2D screenTopLeft, int zoomAt1TimeScale)
        {
            if (Texture == null)
                throw new Exception("Not texture in AnimationFrame2");

            var worldWidth = SourceRec.Width / (32.0 / zoomAt1TimeScale);
            var worldHeight = SourceRec.Height / (32.0 / zoomAt1TimeScale);
            var endX = screenTopLeft.X - ((TopLeftDif.X / (32.0 / zoomAt1TimeScale)) * context.Camera.Zoom) - (worldWidth * ((SourceRec.Width - 32) / SourceRec.Width) * context.Camera.Zoom);
            var endY = screenTopLeft.Y - ((TopLeftDif.Y / (32.0 / zoomAt1TimeScale)) * context.Camera.Zoom) - (worldHeight * ((SourceRec.Height - 32) / SourceRec.Height) * context.Camera.Zoom);

            context.SpriteBatch.Draw(Texture,
                sourceRectangle: SourceRec,
                destinationRectangle: new Rectangle(
                    (int)(endX), (int)(endY),
                    (int)(worldWidth * context.Camera.Zoom), (int)(worldHeight * context.Camera.Zoom)
                    ),
                color: overlay);
        }
    }
}
