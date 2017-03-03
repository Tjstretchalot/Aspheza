using BaseBuilder.Engine;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Tiles;
using BaseBuilder.Screens;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilder
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static Game1 Instance;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        IScreenManager screenManager;

        FrameCounter frameCounter;

        public Game1()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            frameCounter = new FrameCounter();

            Window.Title = "Aspheza";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            graphics.PreferMultiSampling = true;
            /*
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            */

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 1152;
            graphics.PreferredBackBufferHeight = 648;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            
            UIUtils.Load();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            screenManager = new ScreenManager();
            screenManager.SetInitialScreen(new SplashScreen(screenManager, Content, graphics, GraphicsDevice, spriteBatch));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }



        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var deltaMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            screenManager.Update(deltaMS);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            screenManager.Draw();

            //var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

           // frameCounter.Update(deltaTime);

            //var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

           // spriteBatch.Begin();
            //spriteBatch.DrawString(Content.Load<SpriteFont>("Arial"), fps, new Vector2(1, 1), Color.Black);
            //spriteBatch.End();

        }
    }
}
