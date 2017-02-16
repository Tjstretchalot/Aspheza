using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Transitions
{
    /// <summary>
    /// A transition that goes from screen1 to black to screen2
    /// </summary>
    public class FadeThroughBlackTransition : IScreenTransition
    {
        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        protected GraphicsDevice graphicsDevice;
        protected SpriteBatch spriteBatch;

        protected float progress = 0;

        protected IScreen screen1;
        protected IScreen screen2;

        protected Rectangle screenRect;
        protected Texture2D blackPixel;

        // Skipping a few frames allows the screenshots to render. Failure to do so causes the screen to blink black.
        private int skippedFramesCounter;

        public FadeThroughBlackTransition(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, IScreen screen1, IScreen screen2)
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

            blackPixel = new Texture2D(graphicsDevice, 1, 1);
            blackPixel.SetData(new[] { Color.Black });
        }

        public void Draw()
        {
            if(progress < 0.5)
            {
                // overlay screen1 with 0% black to 100% black from 0 to 0.5
                float alpha = (progress / 0.5f);

                screen1.Draw();

                spriteBatch.Begin();

                spriteBatch.Draw(blackPixel, screenRect, new Color(0, 0, 0, alpha));

                spriteBatch.End();
            }else
            {
                // overlay screen2 with 100% black to 0% black from 0.5 to 1
                float alpha = ((1 - progress) / 0.5f);
                
                screen2.Draw();

                spriteBatch.Begin();

                spriteBatch.Draw(blackPixel, screenRect, new Color(0, 0, 0, alpha));

                spriteBatch.End();
            }
        }

        public void Update(double progress)
        {
            this.progress = (float) progress;
        }

        public void Finished()
        {
            blackPixel?.Dispose();

            blackPixel = null;
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
