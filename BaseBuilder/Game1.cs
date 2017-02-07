using BaseBuilder.Engine;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Entities.MobileEntities;
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
        const double MIN_CAMERA_ZOOM = 32;
        const double CAMERA_SPEED = 0.01;
        const double SCROLL_SPEED = 0.01;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        TileWorld world;
        RenderContext renderContext;
        UpdateContext updateContext;

        RectangleD2D screenBounds;
        SpriteFont font;

        Random random;

        double cameraPartialZoom;
        PointD2D cameraPartialTopLeft;
        Camera camera;

        int previousScrollWheelValue;
        bool mouseWasDown;

        int gameTimeMS;


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
            updateContext = new UpdateContext();
            random = new Random();

            IsMouseVisible = true;
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
            camera = new Camera(new PointD2D(0, 0), screenBounds, 16);
            cameraPartialZoom = camera.Zoom;
            cameraPartialTopLeft = new PointD2D(0, 0);

            font = Content.Load<SpriteFont>("Arial");

            previousScrollWheelValue = Mouse.GetState().ScrollWheelValue;
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
                    var point = new PointI2D(x, y);
                    var rand = random.NextDouble();
                    
                    tiles.Add(new GrassTile(point, tileCollisionMesh));

                }
            }

            world = new TileWorld(120, 80, tiles);

            var archerCollisionMesh = new RectangleD2D(7.375, 7.375);
            world.AddMobileEntity(new Archer(new PointD2D(5, 5), archerCollisionMesh));
            world.AddMobileEntity(new Archer(new PointD2D(25, 5), archerCollisionMesh));
            world.AddMobileEntity(new Archer(new PointD2D(5, 25), archerCollisionMesh));
            world.AddMobileEntity(new Archer(new PointD2D(50, 5), archerCollisionMesh));
            world.AddMobileEntity(new Archer(new PointD2D(5, 50), archerCollisionMesh));
        }

        void UpdateCamera(UpdateContext context)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraPartialTopLeft.X = Math.Max(0, cameraPartialTopLeft.X - CAMERA_SPEED * context.ElapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraPartialTopLeft.X = Math.Min(world.TileWidth - camera.VisibleWorldWidth, cameraPartialTopLeft.X + CAMERA_SPEED * context.ElapsedMS);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraPartialTopLeft.Y = Math.Max(0, cameraPartialTopLeft.Y - CAMERA_SPEED * context.ElapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraPartialTopLeft.Y = Math.Min(world.TileHeight - camera.VisibleWorldHeight, cameraPartialTopLeft.Y + CAMERA_SPEED * context.ElapsedMS);


            var scrollWheel = Mouse.GetState().ScrollWheelValue;

            if (scrollWheel != previousScrollWheelValue)
            {
                var delta = scrollWheel - previousScrollWheelValue;
                previousScrollWheelValue = scrollWheel;

                var oldZoom = cameraPartialZoom;
                var oldWorldWidth = camera.VisibleWorldWidth;
                var oldWorldHeight = camera.VisibleWorldHeight;
                var newZoom = cameraPartialZoom + delta * SCROLL_SPEED;

                newZoom = Math.Min(newZoom, MIN_CAMERA_ZOOM);
                newZoom = Math.Max(newZoom, camera.ScreenLocation.Width / world.TileWidth);

                double mousePixelX = Mouse.GetState().Position.X, mousePixelY = Mouse.GetState().Position.Y;
                double mouseWorldX, mouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);

                cameraPartialZoom = newZoom;

                camera.Zoom = Math.Round(cameraPartialZoom);

                double newMouseWorldX, newMouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out newMouseWorldX, out newMouseWorldY);

                cameraPartialTopLeft.X -= (newMouseWorldX - mouseWorldX);
                cameraPartialTopLeft.Y -= (newMouseWorldY - mouseWorldY);

                cameraPartialTopLeft.X = Math.Max(cameraPartialTopLeft.X, 0);
                cameraPartialTopLeft.Y = Math.Max(cameraPartialTopLeft.Y, 0);
                cameraPartialTopLeft.X = Math.Min(cameraPartialTopLeft.X, world.TileWidth - camera.VisibleWorldWidth);
                cameraPartialTopLeft.Y = Math.Min(cameraPartialTopLeft.Y, world.TileHeight - camera.VisibleWorldHeight);
            }

            // the goal is that the camera can move in increments of one pixel. One pixel = (1 world unit / Zoom).
            int pixelTopLeftX = (int)Math.Round(cameraPartialTopLeft.X * camera.Zoom);
            int pixelTopLeftY = (int)Math.Round(cameraPartialTopLeft.Y * camera.Zoom);
            camera.WorldTopLeft.X = pixelTopLeftX / camera.Zoom;
            camera.WorldTopLeft.Y = pixelTopLeftY / camera.Zoom;
        }

        void HandleMouseGrassToDirt(UpdateContext context, bool click, PointD2D worldMousePosition, bool onScreen)
        {
            if (!onScreen || !click)
                return;

            int tileX = (int)worldMousePosition.X, tileY = (int)worldMousePosition.Y;
            var tile = context.World.TileAt(tileX, tileY);

            if(typeof(DirtTile).IsAssignableFrom(tile.GetType()))
            {
                context.World.SetTile(context, new GrassTile(new PointI2D(tileX, tileY), new RectangleD2D(1, 1)));
            }else if(typeof(GrassTile).IsAssignableFrom(tile.GetType()))
            {
                context.World.SetTile(context, new DirtTile(new PointI2D(tileX, tileY), new RectangleD2D(1, 1)));
            }
        }

        void HandleMouse(UpdateContext context)
        {
            var mouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

            var click = mouseDown && !mouseWasDown;

            var pixelMousePosition = new PointD2D(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
            var worldMousePosition = camera.WorldLocationOfPixel(pixelMousePosition);
            var onScreen = pixelMousePosition.X >= 0 && pixelMousePosition.X < screenBounds.Width && pixelMousePosition.Y >= 0 && pixelMousePosition.Y < screenBounds.Height;

            //HandleMouseGrassToDirt(context, click, worldMousePosition, onScreen);
            mouseWasDown = mouseDown;
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

            var elapsedMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            gameTimeMS += elapsedMS;

            bool loadedWorld = world == null;
            if (world == null)
                LoadWorld();

            updateContext.World = world;
            updateContext.ElapsedMS = elapsedMS;
            updateContext.GameTimeMS = gameTimeMS;

            if (loadedWorld)
                world.LoadingDone(updateContext);
            
            UpdateCamera(updateContext);

            world.Update(updateContext);

            HandleMouse(updateContext);
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
            renderContext.Camera = camera;

            spriteBatch.Begin();
            world.Render(renderContext);
            
            spriteBatch.DrawString(font, $"Camera Location: {camera.WorldTopLeft}; Camera Zoom: {camera.Zoom}", new Vector2(5, 5), Color.White);
            spriteBatch.End();
        }
    }
}
