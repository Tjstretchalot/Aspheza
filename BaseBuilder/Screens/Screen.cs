using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens
{
    public abstract class Screen : IScreen
    {
        protected IScreenManager screenManager;
        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        protected GraphicsDevice graphicsDevice;
        protected SpriteBatch spriteBatch;

        protected Screen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.screenManager = screenManager;
            this.content = content;
            this.graphics = graphics;
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
        }

        public abstract void Draw();
        public abstract void Update(int deltaMS);
        public abstract void Dispose();
    }
}
