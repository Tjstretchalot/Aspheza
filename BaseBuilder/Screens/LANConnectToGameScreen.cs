using BaseBuilder.Engine;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.State;
using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.GameScreens;
using BaseBuilder.Screens.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens
{
    public class LANConnectToGameScreen : ComponentScreen
    {
        protected TextField ServerAddressField;
        protected TextField PortField;
        protected TextField MyNameField;

        public LANConnectToGameScreen(IScreenManager screenManager, ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(screenManager, content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override void Initialize()
        {
            var rand = new Random();
            var cfg = new Dictionary<string, string>()
            {
                { "server_address", "127.0.0.1" },
                { "port", "5175" },
                { "my_name", $"Player {rand.Next(1000)}" }
            };

            ScreenManager.LoadConfig(cfg, "lan_connect_to_game_screen.dat");

            var vWidth = graphicsDevice.Viewport.Width;
            var vHeight = graphicsDevice.Viewport.Height;

            var myWidth = (int)Math.Min(vWidth * 0.8, 800);
            var myHeight = (int)Math.Min(vHeight * 0.8, 600);

            var greyPanel = new GreyPanel(new Rectangle(vWidth / 2 - myWidth / 2, vHeight / 2 - myHeight / 2, myWidth, myHeight));

            ServerAddressField = UIUtils.CreateTextField(new Point(-1, -1), new Point(greyPanel.Size.X / 4, 30));
            ServerAddressField.Center = new Point(greyPanel.Center.X - ServerAddressField.Size.X / 2 - 5, greyPanel.Center.Y - greyPanel.Size.Y / 6);
            ServerAddressField.Text = cfg["server_address"];
            var serverNameFieldText = new Text(new Point(0, 0), "Server Address", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            serverNameFieldText.Center = new Point(ServerAddressField.Center.X - ServerAddressField.Size.X / 2 + serverNameFieldText.Size.X / 2, ServerAddressField.Center.Y - ServerAddressField.Size.Y / 2 - serverNameFieldText.Size.Y / 2 - 5);

            PortField = UIUtils.CreateTextField(new Point(-1, -1), new Point(greyPanel.Size.X / 4, 30));
            PortField.Center = new Point(greyPanel.Center.X + PortField.Size.X / 2 + 5, greyPanel.Center.Y - greyPanel.Size.Y / 6);
            PortField.Text = cfg["port"];
            var portFieldText = new Text(new Point(0, 0), "Port", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);

            portFieldText.Center = new Point(PortField.Center.X - PortField.Size.X / 2 + portFieldText.Size.X / 2, PortField.Center.Y - PortField.Size.Y / 2 - portFieldText.Size.Y / 2 - 5);

            MyNameField = UIUtils.CreateTextField(new Point(greyPanel.Center.X, greyPanel.Center.Y), new Point(greyPanel.Size.X / 4, 30));
            MyNameField.Text = cfg["my_name"];
            var myNameFieldText = new Text(new Point(0, 0), "Your Name", content.Load<SpriteFont>("Bitter-Regular"), Color.Black);
            myNameFieldText.Center = new Point(MyNameField.Center.X - MyNameField.Size.X / 2 + myNameFieldText.Size.X / 2, MyNameField.Center.Y - MyNameField.Size.Y / 2 - myNameFieldText.Size.Y / 2 - 5);

            var startButton = UIUtils.CreateButton(new Point(-1, -1), "Connect", UIUtils.ButtonColor.Green, UIUtils.ButtonSize.Medium);
            startButton.Center = new Point(greyPanel.Center.X - startButton.Size.X / 2 - 5, greyPanel.Center.Y + greyPanel.Size.Y / 6);
            var backButton = UIUtils.CreateButton(new Point(-1, -1), "Back", UIUtils.ButtonColor.Grey, UIUtils.ButtonSize.Medium);
            backButton.Center = new Point(greyPanel.Center.X + backButton.Size.X / 2 + 5, greyPanel.Center.Y + greyPanel.Size.Y / 6);


            backButton.OnPressReleased += BackPressed;
            PortField.OnTextChanged += NumbersOnlyTextFieldOnTextChanged;
            startButton.OnPressReleased += StartGame;

            Components.Add(greyPanel);
            Components.Add(ServerAddressField);
            Components.Add(serverNameFieldText);
            Components.Add(PortField);
            Components.Add(portFieldText);
            Components.Add(MyNameField);
            Components.Add(myNameFieldText);
            Components.Add(startButton);
            Components.Add(backButton);
            base.Initialize();
        }

        private void SaveConfig()
        {
            var cfg = new Dictionary<string, string>()
            {
                { "server_address", ServerAddressField.Text },
                { "port",  PortField.Text },
                { "my_name", MyNameField.Text }
            };

            ScreenManager.SaveConfig(cfg, "lan_connect_to_game_screen.dat");
        }

        private void StartGame(object sender, EventArgs e)
        {
            int port = int.Parse(PortField.Text);
            string serverAddress = ServerAddressField.Text;
            var desName = MyNameField.Text;

            IPAddress ipAddress;

            if(!IPAddress.TryParse(serverAddress, out ipAddress))
            {
                Console.WriteLine($"ipaddress could not be parsed: {serverAddress}");
                return;
            }

            var endpoint = new IPEndPoint(ipAddress, port);

            var clientConnection = new ClientGameConnection(endpoint, desName);

            var screenSize = graphicsDevice.Viewport;
            var localGameLogic = new LocalGameLogic(content);

            while (!clientConnection.Connected)
            {
                clientConnection.ContinueConnecting(screenSize);

                System.Threading.Thread.Sleep(16);
            }

            var gameScreen = new GameScreen(content, graphics, graphicsDevice, spriteBatch, localGameLogic, clientConnection.SharedState, clientConnection.LocalState, clientConnection);
            gameScreen.Update(0);
            screenManager.TransitionTo(gameScreen, new CrossFadeTransition(content, graphics, graphicsDevice, spriteBatch, this, gameScreen), 750);
            SaveConfig();
        }

        private void NumbersOnlyTextFieldOnTextChanged(object sender, EventArgs e)
        {
            var field = sender as TextField;

            var newText = field.Text;

            var fixedText = new StringBuilder();

            foreach (var ch in newText)
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
