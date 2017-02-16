using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.Transitions;
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
    public class LANHostGameScreen : ComponentScreen
    {

        public LANHostGameScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));

            var serverNameField = UIUtils.CreateTextField(new Point(vWidth / 2, vHeight / 3));
            var serverNameFieldText = new Text(new Point(0, 0), "Server Name", content.Load<SpriteFont>("Arial"), Color.Black);

            serverNameFieldText.Center = new Point(serverNameField.Center.X - serverNameField.Size.X / 2 + serverNameFieldText.Size.X / 2, serverNameField.Center.Y - serverNameField.Size.Y / 2 - serverNameFieldText.Size.Y / 2 - 5);

            var startButton = UIUtils.CreateButton(new Point(vWidth / 3, (vHeight * 2) / 3), "Start", UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            var backButton = UIUtils.CreateButton(new Point((vWidth * 2) / 3, (vHeight * 2) / 3), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            
            backButton.OnPressReleased += BackPressed;

            Components.Add(greyPanel);
            Components.Add(serverNameField);
            Components.Add(serverNameFieldText);
            Components.Add(startButton);
            Components.Add(backButton);
            base.Initialize();
        }
        
        private void BackPressed(object sender, EventArgs e)
        {
            var newScreen = new LANSetupGameScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }
    }
}
