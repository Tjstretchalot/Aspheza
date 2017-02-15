using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.Transitions
{
    public class FadeTransition : IScreenTransition
    {
        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        protected GraphicsDevice graphicsDevice;
        protected SpriteBatch spriteBatch;

        protected float progress = 0;
        protected float firstProgress = 0;

        protected IScreen screen1;
        protected IScreen screen2;

        protected Texture2D input1;
        protected Texture2D input2;

        protected Rectangle screenRect;

        // Skipping a few frames allows the screenshots to render. Failure to do so causes the screen to blink black.
        private int skippedFramesCounter;

        public FadeTransition(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, IScreen screen1, IScreen screen2)
        {
            this.content = content;
            this.graphics = graphics;
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            this.screen1 = screen1;
            this.screen2 = screen2;


            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            screenRect = new Rectangle(0, 0, vWidth, vHeight);
        }

        public void Draw()
        {
            if(input1 == null)
            {
                input1 = RenderScreenToTexture(screen1);

                screen1.Draw();
                return;
            }

            if(input2 == null)
            {
                input2 = RenderScreenToTexture(screen2);
                screen1.Draw();
                return;
            }

            if(skippedFramesCounter < 2)
            {
                screen1.Draw();
                skippedFramesCounter++;

                if(skippedFramesCounter == 2)
                {
                    firstProgress = progress;
                }

                return;
            }

            spriteBatch.Begin();

            var realProgress = (progress - firstProgress) / (1 - firstProgress);
            
            spriteBatch.Draw(input1, screenRect, Color.White * (1 - realProgress));
            spriteBatch.Draw(input2, screenRect, Color.White * realProgress);

            spriteBatch.End();
        }

        public void Update(double progress)
        {
            this.progress = (float)progress;
        }


        /// <summary>
        /// Renders the screen onto a texture and returns that texture.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="content"></param>
        /// <param name="graphics"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        protected Texture2D RenderScreenToTexture(IScreen screen)
        {
            // adapted from http://community.monogame.net/t/how-to-make-screenshot/1742
            int w, h;
            w = graphicsDevice.PresentationParameters.BackBufferWidth;
            h = graphicsDevice.PresentationParameters.BackBufferHeight;
            RenderTarget2D screenshot = new RenderTarget2D(graphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None);
            graphicsDevice.SetRenderTarget(screenshot);
            screen.Draw();
            graphicsDevice.Present();
            graphicsDevice.SetRenderTarget(null);
            return screenshot;
        }
    }
}
