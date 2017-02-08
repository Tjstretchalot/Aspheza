using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Buttons;

namespace BaseBuilder.Screens
{
    public class MainMenuScreen : Screen
    {
        private Button TestButton;

        public MainMenuScreen(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            var sourceY = 95;
            var sourceHeight = 50;
            TestButton = new Button("Play Game", "Arial", new Engine.Math2D.PointI2D(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2), "ButtonSmall",
                new Rectangle(112, sourceY, 128, sourceHeight),
                new Rectangle(247, sourceY, 128, sourceHeight),
                new Rectangle(381, sourceY, 128, sourceHeight), "MouseEnter", "MouseLeave", "ButtonPress", "ButtonUnpress");
        }

        public override void Draw()
        {
            graphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            TestButton.Draw(content, graphics, graphicsDevice, spriteBatch);

            spriteBatch.End();
        }

        public override void Update(int deltaMS)
        {
            TestButton.Update(content);
        }
    }
}
