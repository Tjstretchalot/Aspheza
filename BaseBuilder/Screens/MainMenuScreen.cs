using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Screens
{
    public class MainMenuScreen : ComponentScreen
    {
        public MainMenuScreen(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var TestButton = ButtonUtils.CreateSmallButton(new PointI2D(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2), "Test Button");

            TestButton.OnPressReleased += TestButtonPressReleased;
            Components.Add(TestButton);

            base.Initialize();
        }

        private void TestButtonPressReleased(object sender, EventArgs args)
        {
            Console.WriteLine("test button pressed");

            var button = sender as Button;
            if(button.Text.Equals("Pressed"))
            {
                button.Text = "Test Button";
            }else
            {
                button.Text = "Pressed";
            }
        }
    }
}
