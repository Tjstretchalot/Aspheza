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

            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));
            var multiplayerButton = UIUtils.CreateButton(new Point(vWidth / 2, vHeight / 3), "Multiplayer", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            multiplayerButton.OnPressReleased += MultiplayerPressed;

            Components.Add(greyPanel);
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
