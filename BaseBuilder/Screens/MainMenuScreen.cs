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

            multiplayerButton.PressReleased += MultiplayerPressed;

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
