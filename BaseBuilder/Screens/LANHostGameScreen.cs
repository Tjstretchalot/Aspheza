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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BaseBuilder.Screens
{
    public class LANHostGameScreen : ComponentScreen
    {
        protected TextField MyNameField;
        protected TextField PortField;

        public LANHostGameScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var cfg = new Dictionary<string, string>()
            {
                { "my_name", "Host" },
                { "port", "5175" }
            };
            ScreenManager.LoadConfig(cfg, "lan_host_game_screen.dat");

            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;
            
            var greyPanel = new GreyPanel(new Rectangle((int)(vWidth * 0.1), (int)(vHeight * 0.1), (int)(vWidth * 0.8), (int)(vHeight * 0.8)));

            MyNameField = UIUtils.CreateTextField(new Point(vWidth / 3, vHeight / 3), new Point(vWidth / 4, 30));
            MyNameField.Text = cfg["my_name"];
            var myNameFieldText = new Text(new Point(0, 0), "Your Name", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            myNameFieldText.Center = new Point(MyNameField.Center.X - MyNameField.Size.X / 2 + myNameFieldText.Size.X / 2, MyNameField.Center.Y - MyNameField.Size.Y / 2 - myNameFieldText.Size.Y / 2 - 5);

            PortField = UIUtils.CreateTextField(new Point((vWidth * 2) / 3, vHeight / 3), new Point(vWidth / 4, 30));
            PortField.Text = cfg["port"]; ;
            var portFieldText = new Text(new Point(0, 0), "Port", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            portFieldText.Center = new Point(PortField.Center.X - PortField.Size.X / 2 + portFieldText.Size.X / 2, PortField.Center.Y - PortField.Size.Y / 2 - portFieldText.Size.Y / 2 - 5);

            var startButton = UIUtils.CreateButton(new Point(vWidth / 3, (vHeight * 2) / 3), "Start", UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            var backButton = UIUtils.CreateButton(new Point((vWidth * 2) / 3, (vHeight * 2) / 3), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            
            backButton.OnPressReleased += BackPressed;
            PortField.OnTextChanged += NumbersOnlyTextFieldOnTextChanged;
            startButton.OnPressReleased += StartGame;

            Components.Add(greyPanel);
            Components.Add(MyNameField);
            Components.Add(myNameFieldText);
            Components.Add(PortField);
            Components.Add(portFieldText);
            Components.Add(startButton);
            Components.Add(backButton);
            base.Initialize();
        }

        private void SaveConfig()
        {
            var cfg = new Dictionary<string, string>()
            {
                { "port",  PortField.Text },
                { "my_name", MyNameField.Text }
            };

            ScreenManager.SaveConfig(cfg, "lan_host_game_screen.dat");
        }

        private void StartGame(object sender, EventArgs e)
        {
            int port = int.Parse(PortField.Text);
            string myName = MyNameField.Text;

            var generator = new WorldGenerator();
            generator.Create(graphicsDevice);
            var localGameState = generator.LocalGameState;
            var sharedGameState = generator.SharedGameState;
            sharedGameState.Players[0].Name = myName;

            var sharedGameLogic = new SharedGameLogic();

            var serverConnection = new ServerGameConnection(localGameState, sharedGameState, sharedGameLogic, port);
            
            var localGameLogic = new LocalGameLogic(content);
            serverConnection.BeginListening();

            var gameScreen = new GameScreen(content, graphics, graphicsDevice, spriteBatch, localGameLogic, sharedGameState, localGameState, serverConnection);
            gameScreen.Update(0);
            screenManager.TransitionTo(gameScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, gameScreen), 750);
            SaveConfig();
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
            SaveConfig();
        }
    }
}
