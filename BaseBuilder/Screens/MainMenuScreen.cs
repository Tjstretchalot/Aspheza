using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens
{
    public class MainMenuScreen : Screen
    {
        public MainMenuScreen(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
        }

        public override void Draw()
        {
            graphicsDevice.Clear(Color.Black);
        }

        public override void Update(int deltaMS)
        {
        }
    }
}
