using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.Logic.WorldGen;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.GameScreens;
using BaseBuilder.Screens.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;

namespace BaseBuilder.Screens
{
    public class LANHostGameScreen : ComponentScreen
    {
        protected TextField ServerNameField;
        protected TextField PortField;

        public LANHostGameScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;
            
            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));

            ServerNameField = UIUtils.CreateTextField(new Point(vWidth / 3, vHeight / 3), new Point(vWidth / 4, 30));
            var serverNameFieldText = new Text(new Point(0, 0), "Server Name", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            serverNameFieldText.Center = new Point(ServerNameField.Center.X - ServerNameField.Size.X / 2 + serverNameFieldText.Size.X / 2, ServerNameField.Center.Y - ServerNameField.Size.Y / 2 - serverNameFieldText.Size.Y / 2 - 5);

            PortField = UIUtils.CreateTextField(new Point((vWidth * 2) / 3, vHeight / 3), new Point(vWidth / 4, 30));
            PortField.Text = "5175";
            var portFieldText = new Text(new Point(0, 0), "Port", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            portFieldText.Center = new Point(PortField.Center.X - PortField.Size.X / 2 + portFieldText.Size.X / 2, PortField.Center.Y - PortField.Size.Y / 2 - portFieldText.Size.Y / 2 - 5);

            var startButton = UIUtils.CreateButton(new Point(vWidth / 3, (vHeight * 2) / 3), "Start", UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            var backButton = UIUtils.CreateButton(new Point((vWidth * 2) / 3, (vHeight * 2) / 3), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            
            backButton.OnPressReleased += BackPressed;
            PortField.OnTextChanged += NumbersOnlyTextFieldOnTextChanged;
            startButton.OnPressReleased += StartGame;

            Components.Add(greyPanel);
            Components.Add(ServerNameField);
            Components.Add(serverNameFieldText);
            Components.Add(PortField);
            Components.Add(portFieldText);
            Components.Add(startButton);
            Components.Add(backButton);
            base.Initialize();
        }

        private void StartGame(object sender, EventArgs e)
        {
            int port = int.Parse(PortField.Text);
            string serverName = ServerNameField.Text;

            var generator = new WorldGenerator();
            generator.Create(graphicsDevice);
            var localGameState = generator.LocalGameState;
            var sharedGameState = generator.SharedGameState;

            var sharedGameLogic = new SharedGameLogic();

            var serverConnection = new ServerGameConnection(localGameState, sharedGameState, sharedGameLogic, port);

            var localGameLogic = new LocalGameLogic();

            var gameScreen = new GameScreen(content, graphics, graphicsDevice, spriteBatch, localGameLogic, sharedGameState, localGameState, serverConnection);
            gameScreen.Update(0);
            screenManager.TransitionTo(gameScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, gameScreen), 750);
        }

        private void NumbersOnlyTextFieldOnTextChanged(object sender, EventArgs e)
        {
            var field = sender as TextField;

            var newText = field.Text;

            var fixedText = new StringBuilder();

            foreach(var ch in newText)
            {
                if (char.IsDigit(ch))
                    fixedText.Append(ch);
            }

            field.Text = fixedText.ToString();
        }

        private void BackPressed(object sender, EventArgs e)
        {
            var newScreen = new LANSetupGameScreen(screenManager, content, graphics, graphicsDevice, spriteBatch);
            newScreen.Update(0);
            screenManager.TransitionTo(newScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, newScreen), 750);
        }
    }
}
