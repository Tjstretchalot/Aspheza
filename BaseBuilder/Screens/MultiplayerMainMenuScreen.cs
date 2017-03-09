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
    public class MultiplayerMainMenuScreen : ComponentScreen
    {
        public MultiplayerMainMenuScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }
        
        protected override void Initialize()
        {
            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            var myWidth = (int)Math.Min(vWidth * 0.8, 800);
            var myHeight = (int)Math.Min(vHeight * 0.8, 600);

            var greyPanel = new GreyPanel(new Rectangle(vWidth / 2 - myWidth / 2, vHeight / 2 - myHeight / 2, myWidth, myHeight));
            var lanButton = UIUtils.CreateButton(new Point(greyPanel.Center.X, greyPanel.Center.Y - greyPanel.Size.Y / 6), "Play on LAN", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);
            var backButton = UIUtils.CreateButton(new Point(greyPanel.Center.X, greyPanel.Center.Y + greyPanel.Size.Y / 6), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);

            lanButton.PressReleased += LANPressed;
            backButton.PressReleased += BackPressed;


            Components.Add(greyPanel);
            Components.Add(lanButton);
            Components.Add(backButton);
            base.Initialize();
        }

        private void LANPressed(object sender, EventArgs e)
        {
            var newScreen = new LANSetupGameScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }

        private void BackPressed(object sender, EventArgs e)
        {
            var newScreen = new MainMenuScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }
    }
}
