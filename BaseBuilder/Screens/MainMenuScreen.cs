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
            var GreyPanel = new GreyPanel(new Rectangle(graphicsDevice.Viewport.Width / 2 - 200, graphicsDevice.Viewport.Height / 2 - 150, 400, 300));
            Components.Add(GreyPanel);

            var TestButton = UIUtils.CreateButton(new PointI2D(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2), "Blue Button", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            TestButton.OnPressReleased += TestButtonPressReleased;
            Components.Add(TestButton);

            var TestField = UIUtils.CreateTextField(new PointI2D(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2 - 100));
            TestField.Text = "text field text";
            Components.Add(TestField);

            base.Initialize();
        }

        private void TestButtonPressReleased(object sender, EventArgs args)
        {
            var button = sender as Button;
            if (button.Text.Equals("Grey Button"))
            {
                button.Text = "Blue Button";
                UIUtils.SetButton(button, UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
            }
            else if (button.Text.Equals("Blue Button"))
            {
                button.Text = "Green Button";
                UIUtils.SetButton(button, UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            }
            else if (button.Text.Equals("Green Button"))
            {
                button.Text = "Red Button";
                UIUtils.SetButton(button, UIUtils.ButtonColor.Red, UIUtils.ButtonSize.Medium);
            }
            else if (button.Text.Equals("Red Button"))
            {
                button.Text = "Yellow Button";
                UIUtils.SetButton(button, UIUtils.ButtonColor.Yellow, UIUtils.ButtonSize.Medium);
            }
            else if (button.Text.Equals("Yellow Button"))
            {
                button.Text = "Grey Button";
                UIUtils.SetButton(button, UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            }
        }
    }
}
