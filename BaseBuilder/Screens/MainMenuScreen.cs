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
using BaseBuilder.Screens.Transitions;

namespace BaseBuilder.Screens
{
    public class MainMenuScreen : ComponentScreen
    {
        public MainMenuScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            var myWidth = (int)Math.Min(vWidth * 0.8, 800);
            var myHeight = (int)Math.Min(vHeight * 0.8, 600);

            var greyPanel = new GreyPanel(new Rectangle(vWidth / 2 - myWidth / 2, vHeight / 2 - myHeight / 2, myWidth, myHeight));
            var multiplayerButton = UIUtils.CreateButton(new Point(greyPanel.Center.X, greyPanel.Center.Y - greyPanel.Size.Y / 6), "Multiplayer", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            multiplayerButton.OnPressReleased += MultiplayerPressed;
            
            var testing = new ComboBox(new List<ComboBoxItem> {
                new ComboBoxItem("Bitter-Regular", "Item 1"),
                new ComboBoxItem("Bitter-Regular", "Item 2"),
                new ComboBoxItem("Bitter-Regular", "Item 4"),
                new ComboBoxItem("Bitter-Regular", "Item 5"),
                new ComboBoxItem("Bitter-Regular", "Item 6"),
                new ComboBoxItem("Bitter-Regular", "Item 7"),
                new ComboBoxItem("Bitter-Regular", "Item 8"),
            }, new Point(300, 50));
            testing.EnsureInitialized(content, graphics, graphicsDevice);
            testing.Center = new Point(vWidth / 2 - testing.Size.X / 2, multiplayerButton.Center.Y - multiplayerButton.Center.Y / 2 - testing.Size.Y / 2 - 5);

            Components.Add(greyPanel);
            Components.Add(testing);
            Components.Add(multiplayerButton);
            base.Initialize();
        }

        private void MultiplayerPressed(object sender, EventArgs e)
        {
            var newScreen = new MultiplayerMainMenuScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }
    }
}
