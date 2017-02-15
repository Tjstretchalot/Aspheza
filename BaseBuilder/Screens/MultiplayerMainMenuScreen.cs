using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;

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

            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));
            var lanButton = UIUtils.CreateButton(new Point(vWidth / 2, vHeight / 3), "Play on LAN", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            lanButton.OnPressReleased += LANPressed;

            Components.Add(greyPanel);
            Components.Add(lanButton);
            base.Initialize();
        }

        private void LANPressed(object sender, EventArgs e)
        {

        }
    }
}
