using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.Transitions;

namespace BaseBuilder.Screens
{
    public class LANSetupGameScreen : ComponentScreen
    {
        public LANSetupGameScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));
            var hostButton = UIUtils.CreateButton(new Point(vWidth / 2, vHeight / 3), "Host Game", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
            var findButton = UIUtils.CreateButton(new Point(vWidth / 2, (vHeight / 3) + hostButton.Size.Y + 15), "Find Game", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            var backButton = UIUtils.CreateButton(new Point(vWidth / 2, (vHeight * 2) / 3), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);

            hostButton.OnPressReleased += HostPressed;
            findButton.OnPressReleased += FindPressed;
            backButton.OnPressReleased += BackPressed;

            Components.Add(greyPanel);
            Components.Add(hostButton);
            Components.Add(findButton);
            Components.Add(backButton);
            base.Initialize();
        }

        private void HostPressed(object sender, EventArgs e)
        {
            var newScreen = new LANHostGameScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }

        private void FindPressed(object sender, EventArgs e)
        {
            var newScreen = new LANConnectToGameScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }

        private void BackPressed(object sender, EventArgs e)
        {
            var newScreen = new MultiplayerMainMenuScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }
    }
}
