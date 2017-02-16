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

            var backButton = UIUtils.CreateButton(new Point(vWidth / 2, (vHeight * 2) / 3), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            
            backButton.OnPressReleased += BackPressed;

            Components.Add(greyPanel);
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
