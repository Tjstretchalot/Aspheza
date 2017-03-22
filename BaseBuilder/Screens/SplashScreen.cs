using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Transitions;
using BaseBuilder.Screens.GameScreens;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Logic.WorldGen;

namespace BaseBuilder.Screens
{
    public class SplashScreen : Screen
    {
        protected enum SplashState
        {
            FadingIn, Waiting, FadingOut
        }

        protected SplashState CurrState;
        protected double CurrentAlpha;
        protected int Timer;

        protected Rectangle BackgroundRect;
        protected Rectangle LogoRect;

        protected Texture2D Background;
        protected Texture2D Logo;

        public SplashScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
            BackgroundRect = new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
            
            Background = new Texture2D(graphicsDevice, 1, 1);
            Background.SetData(new[] { Color.White });

            Logo = content.Load<Texture2D>("logo");

            LogoRect = new Rectangle(BackgroundRect.Center.X - Logo.Bounds.Width / 2, BackgroundRect.Center.Y - Logo.Bounds.Height / 2, Logo.Bounds.Width, Logo.Bounds.Height);

            CurrentAlpha = 0;
            CurrState = SplashState.FadingIn;
        }

        public override void Draw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Background, destinationRectangle: BackgroundRect);
            spriteBatch.Draw(Logo, destinationRectangle: LogoRect, color: new Color(Color.White, (int)Math.Round(CurrentAlpha * 255)));
            spriteBatch.End();   
        }

        public override void Update(int deltaMS)
        {
            switch(CurrState)
            {
                case SplashState.FadingIn:
                    CurrentAlpha += (0.0005 * deltaMS);

                    if(CurrentAlpha >= 1)
                    {
                        CurrentAlpha = 1;
                        Timer = 2000;
                        CurrState = SplashState.Waiting;
                    }
                    break;
                case SplashState.Waiting:
                    Timer -= deltaMS;

                    if(Timer <= 0)
                    {
                        CurrState = SplashState.FadingOut;
                    }
                    break;
                case SplashState.FadingOut:
                    /*var newScreen = new MainMenuScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
                    newScreen.Update(0);
                    screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 1000);
                    */SkipToHostGame();
                    break;
            }
        }

        protected void SkipToHostGame()
        {
            var generator = new WorldGenerator();
            generator.Create(graphicsDevice);
            var localGameState = generator.LocalGameState;
            var sharedGameState = generator.SharedGameState;
            sharedGameState.Players[0].Name = "Host";

            var sharedGameLogic = new SharedGameLogic();

            var serverConnection = new ServerGameConnection(localGameState, sharedGameState, sharedGameLogic, 5178);

            var localGameLogic = new LocalGameLogic(content, graphics, graphicsDevice, spriteBatch);
            serverConnection.BeginListening();

            var gameScreen = new GameScreen(content, graphics, graphicsDevice, spriteBatch, localGameLogic, sharedGameState, localGameState, serverConnection);
            gameScreen.Update(0);
            screenManager.TransitionTo(gameScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, gameScreen), 750);
        }

        public override void Dispose()
        {
            Background.Dispose();
            Background = null;
        }
    }
}
