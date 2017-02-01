using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Tiles;
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
        const double CAMERA_SPEED = 2;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        TileWorld world;
        RenderContext renderContext;

        RectangleD2D screenBounds;
        PointD2D cameraLocation;
        SpriteFont font;

        Random random;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            renderContext = new RenderContext();
            cameraLocation = new PointD2D(0, 0);
            random = new Random();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            screenBounds = new RectangleD2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            font = Content.Load<SpriteFont>("Arial");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void LoadWorld()
        {
            var tiles = new List<Tile>(120 * 80);

            var tileCollisionMesh = new RectangleD2D(1, 1);
            
            for(int y = 0; y < 80; y++)
            {
                for(int x = 0; x < 120; x++)
                {
                    var point = new PointD2D(x, y);
                    var rand = random.NextDouble();

                    if (rand < 0.05)
                        tiles.Add(new StoneTile(point, tileCollisionMesh));
                    else
                        tiles.Add(new GrassTile(point, tileCollisionMesh));

                }
            }

            world = new TileWorld(120, 80, tiles);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO game states and stuff
            if (world == null)
                LoadWorld();

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraLocation.X = Math.Max(0, cameraLocation.X - CAMERA_SPEED);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraLocation.X = Math.Min(world.TileWidth * CameraZoom.SCREEN_OVER_WORLD- screenBounds.Width, cameraLocation.X + CAMERA_SPEED);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraLocation.Y = Math.Max(0, cameraLocation.Y - CAMERA_SPEED);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraLocation.Y = Math.Min(world.TileHeight * CameraZoom.SCREEN_OVER_WORLD - screenBounds.Height, cameraLocation.Y + CAMERA_SPEED);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO game states and stuff
            if (world == null)
                return;

            renderContext.Graphics = graphics;
            renderContext.SpriteBatch = spriteBatch;
            renderContext.Content = Content;

            spriteBatch.Begin();
            world.Render(renderContext, screenBounds, cameraLocation);

            spriteBatch.DrawString(font, $"Camera Location: {cameraLocation}", new Vector2(5, 5), Color.White);
            spriteBatch.End();
        }
    }
}
