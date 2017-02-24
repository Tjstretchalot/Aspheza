using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Logic.Players;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// This shows the players list when you press P
    /// </summary>
    public class PlayersOverlay : MyGameComponent
    {
        protected List<Player> Players;
        protected bool ShowingPlayers;
        protected Texture2D BackgroundTexture;
        

        public PlayersOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(graphicsDevice.Viewport.Width / 3, graphicsDevice.Viewport.Height / 5), new PointI2D((graphicsDevice.Viewport.Width * 2) / 3, graphicsDevice.Viewport.Height / 3), 4);

            ShowingPlayers = true;
            BackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { new Color(Color.Black, 0.6f) });
        }

        public override void Draw(RenderContext context)
        {
            if(ShowingPlayers && Players != null)
            {
                var font = context.DefaultFont;

                int contentWidth = (int)Math.Ceiling(font.MeasureString("Players:").X) + 25;

                foreach(var pl in Players)
                {
                    contentWidth = (int)Math.Ceiling(Math.Max(contentWidth, font.MeasureString($" - {pl.Name}").X));
                }

                int lineHeight = font.LineSpacing;

                const int xPadding = 5;
                const int yPadding = 3;
                int displWidth = contentWidth + xPadding * 2;
                int displHeight = lineHeight * (Players.Count + 1) + yPadding * 2;

                int screenCenterX = GraphicsDevice.Viewport.Width / 2;
                SpriteBatch.Draw(BackgroundTexture, destinationRectangle: new Rectangle(screenCenterX - displWidth / 2, ScreenLocation.Y, displWidth, displHeight));

                int x = screenCenterX - displWidth / 2 + xPadding;
                int y = ScreenLocation.Y + yPadding;
                SpriteBatch.DrawString(font, "Players:", new Vector2(x, y), Color.White);
                y += lineHeight;
                foreach (var pl in Players)
                {
                    SpriteBatch.DrawString(font, $" - {pl.Name}", new Vector2(x, y), Color.White);
                    y += lineHeight;
                }
            }
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            Players = sharedGameState.Players;
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            ShowingPlayers = current.IsKeyDown(Keys.P);
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();

            BackgroundTexture.Dispose();
            BackgroundTexture = null;
        }
    }
}
