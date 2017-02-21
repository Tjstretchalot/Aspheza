using BaseBuilder.Engine;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// This screen is visible while the game is in play. The screen itself delegates all logic to
    /// the 
    /// </summary>
    public class GameScreen : IScreen
    {
        ContentManager content;
        GraphicsDeviceManager graphics;
        GraphicsDevice graphicsDevice;
        SpriteBatch spriteBatch;

        LocalGameLogic localGameLogic;
        SharedGameState sharedGameState;
        LocalGameState localGameState;
        IGameConnection gameConnection;

        RenderContext renderContext;

        SpriteFont debugFont;

        public GameScreen(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, 
            LocalGameLogic localGameLogic, SharedGameState sharedGameState, LocalGameState localGameState, IGameConnection gameConnection)
        {
            this.content = content;
            this.graphics = graphics;
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
            this.localGameLogic = localGameLogic;
            this.sharedGameState = sharedGameState;
            this.localGameState = localGameState;
            this.gameConnection = gameConnection;

            debugFont = content.Load<SpriteFont>("Bitter-Regular");
        }
        
        public void Update(int deltaMS)
        {
            localGameLogic.HandleUserInput(sharedGameState, localGameState, gameConnection.Context, deltaMS);

            gameConnection.ConsiderGameUpdate();
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

            // TODO game states and stuff

            renderContext.Graphics = graphics;
            renderContext.SpriteBatch = spriteBatch;
            renderContext.Content = content;
            renderContext.Camera = localGameState.Camera;
            renderContext.GraphicsDevice = graphicsDevice;
            renderContext.DebugFont = debugFont;
            renderContext.CollisionDebug = localGameState.CollisionDebug;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            sharedGameState.World.Render(renderContext);

            //spriteBatch.DrawString(debugFont, $"Camera Location: {localGameState.Camera.WorldTopLeft}; Camera Zoom: {localGameState.Camera.Zoom}", new Vector2(5, 5), Color.White);
            spriteBatch.DrawString(debugFont, $"Game Time: {sharedGameState.GameTimeMS}", new Vector2(5, 5), Color.Black);
            spriteBatch.End();
        }
    }
}
