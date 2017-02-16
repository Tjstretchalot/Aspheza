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

namespace BaseBuilder.Screens.GameScreen
{
    /// <summary>
    /// This screen is visible while the game is in play. The screen itself delegates all logic to
    /// the 
    /// </summary>
    public class GameScreen : IScreen
    {
        const double MIN_CAMERA_ZOOM = 32;
        const double CAMERA_SPEED = 0.01;
        const double SCROLL_SPEED = 0.01;

        ContentManager content;
        GraphicsDeviceManager graphics;
        GraphicsDevice graphicsDevice;
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

        [Obsolete(message: "This type of function is related to the logic of the game, not the rendering")]
        protected void LoadWorld()
        {
            var tiles = new List<Tile>(120 * 80);

            var tileCollisionMesh = new RectangleD2D(1, 1);

            for (int y = 0; y < 80; y++)
            {
                for (int x = 0; x < 120; x++)
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

        
        [Obsolete(message: "This type of function is related to the logic of the game, not the rendering")]
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

        void HandleMouse(UpdateContext context)
        {
            var mouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

            var click = mouseDown && !mouseWasDown;

            var pixelMousePosition = new PointD2D(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
            var worldMousePosition = camera.WorldLocationOfPixel(pixelMousePosition);
            var onScreen = pixelMousePosition.X >= 0 && pixelMousePosition.X < screenBounds.Width && pixelMousePosition.Y >= 0 && pixelMousePosition.Y < screenBounds.Height;
            
            mouseWasDown = mouseDown;
        }

        public void Update(int deltaMS)
        {
            gameTimeMS += deltaMS;

            bool loadedWorld = world == null;
            if (world == null)
                LoadWorld();

            updateContext.World = world;
            updateContext.ElapsedMS = deltaMS;
            updateContext.GameTimeMS = gameTimeMS;

            if (loadedWorld)
                world.LoadingDone(updateContext);

            UpdateCamera(updateContext);

            world.Update(updateContext);

            HandleMouse(updateContext);
        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.CornflowerBlue);

            // TODO game states and stuff
            if (world == null)
                return;

            renderContext.Graphics = graphics;
            renderContext.SpriteBatch = spriteBatch;
            renderContext.Content = content;
            renderContext.Camera = camera;

            spriteBatch.Begin();
            world.Render(renderContext);

            spriteBatch.DrawString(font, $"Camera Location: {camera.WorldTopLeft}; Camera Zoom: {camera.Zoom}", new Vector2(5, 5), Color.White);
            spriteBatch.End();
        }
    }
}
