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
            
            var myWidth = (int)Math.Min(vWidth * 0.8, 800);
            var myHeight = (int)Math.Min(vHeight * 0.8, 600);

            var greyPanel = new GreyPanel(new Rectangle(vWidth / 2 - myWidth / 2, vHeight / 2 - myHeight / 2, myWidth, myHeight));

            MyNameField = UIUtils.CreateTextField(new Point(-1, -1), new Point(greyPanel.Size.X / 4, 30)); 
            MyNameField.Text = cfg["my_name"];
            MyNameField.Center = new Point(greyPanel.Center.X - MyNameField.Size.X / 2 - 5, greyPanel.Center.Y - greyPanel.Size.Y / 6);
            var myNameFieldText = new Text(new Point(0, 0), "Your Name", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            myNameFieldText.Center = new Point(MyNameField.Center.X - MyNameField.Size.X / 2 + myNameFieldText.Size.X / 2, MyNameField.Center.Y - MyNameField.Size.Y / 2 - myNameFieldText.Size.Y / 2 - 5);

            PortField = UIUtils.CreateTextField(new Point(-1, -1), new Point(greyPanel.Size.X / 4, 30));
            PortField.Text = cfg["port"]; ;
            PortField.Center = new Point(greyPanel.Center.X + PortField.Size.X / 2 + 5, greyPanel.Center.Y - greyPanel.Size.Y / 6);

            var portFieldText = new Text(new Point(0, 0), "Port", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            portFieldText.Center = new Point(PortField.Center.X - PortField.Size.X / 2 + portFieldText.Size.X / 2, PortField.Center.Y - PortField.Size.Y / 2 - portFieldText.Size.Y / 2 - 5);

            var startButton = UIUtils.CreateButton(new Point(-1, -1), "Start", UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            startButton.Center = new Point(greyPanel.Center.X - startButton.Size.X / 2 - 5, greyPanel.Center.Y + greyPanel.Size.Y / 6);
            var backButton = UIUtils.CreateButton(new Point(-1, -1), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            backButton.Center = new Point(greyPanel.Center.X + backButton.Size.X / 2 + 5, greyPanel.Center.Y + greyPanel.Size.Y / 6);

            backButton.PressReleased += BackPressed;
            PortField.TextChanged += NumbersOnlyTextFieldOnTextChanged;
            startButton.PressReleased += StartGame;

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
            
            var localGameLogic = new LocalGameLogic(content, graphics, graphicsDevice, spriteBatch);
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
