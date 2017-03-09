using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens
{
    public class EscapeMenuOverlay : MyGameComponent
    {
        protected GreyPanel Background;
        protected Button QuitButton;
        protected bool Visible;

        public EscapeMenuOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            const int width = 200;
            const int height = 60;
            Init(new Engine.Math2D.PointI2D(graphicsDevice.Viewport.Width / 2 - width / 2, graphicsDevice.Viewport.Height / 2 - height / 2), new Engine.Math2D.PointI2D(width, height), 6);
            Background = new GreyPanel(new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y));
            QuitButton = UIUtils.CreateButton(new Point(Background.Center.X, Background.Center.Y), "Quit Game", UIUtils.ButtonColor.Blue, UIUtils.ButtonSize.Medium);

            QuitButton.PressReleased += QuitGame;
        }

        private void QuitGame(object sender, EventArgs e)
        {
            Game1.Instance.Exit();
        }

        public override void Draw(RenderContext context)
        {
            if (!Visible)
                return;

            Background.Draw(Content, Graphics, GraphicsDevice, SpriteBatch);
            QuitButton.Draw(Content, Graphics, GraphicsDevice, SpriteBatch);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            if (!Visible)
                return;

            Background.Update(Content, timeMS);
            QuitButton.Update(Content, timeMS);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            if(last.IsKeyDown(Keys.Escape) && current.IsKeyUp(Keys.Escape))
            {
                Visible = !Visible;
                return true;
            }

            return false;
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            handled = handled || Visible;
            scrollHandled = scrollHandled || Visible;
        }
    }
}
