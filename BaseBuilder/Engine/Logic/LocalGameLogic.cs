using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic
{
    /// <summary>
    /// The local game logic converts player actions (such as clicking a unit) into modifications to
    /// the local game state (the selected unit). Some local game modifications will propagate through
    /// networking into the shared game state, but this class never directly modifies anything except
    /// for the local game state.
    /// </summary>
    public class LocalGameLogic
    {
        protected int previousScrollWheelValue;
        
        protected int minCameraZoom;
        protected double cameraSpeed;
        protected double cameraZoomSpeed;
        protected double cameraPartialZoom;
        protected PointD2D cameraPartialTopLeft;

        public LocalGameLogic()
        {
            cameraSpeed = 0.01;
            minCameraZoom = 32;
            cameraZoomSpeed = 0.01;

            cameraPartialZoom = 8;

            cameraPartialTopLeft = new PointD2D(0, 0);
        }

        protected virtual void UpdateCamera(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS)
        {
            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraPartialTopLeft.X = Math.Max(0, cameraPartialTopLeft.X - cameraSpeed * elapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraPartialTopLeft.X = Math.Min(world.TileWidth - camera.VisibleWorldWidth, cameraPartialTopLeft.X + cameraSpeed * elapsedMS);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraPartialTopLeft.Y = Math.Max(0, cameraPartialTopLeft.Y - cameraSpeed * elapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraPartialTopLeft.Y = Math.Min(world.TileHeight - camera.VisibleWorldHeight, cameraPartialTopLeft.Y + cameraSpeed * elapsedMS);


            var scrollWheel = Mouse.GetState().ScrollWheelValue;

            if (scrollWheel != previousScrollWheelValue)
            {
                var delta = scrollWheel - previousScrollWheelValue;
                previousScrollWheelValue = scrollWheel;

                var oldZoom = cameraPartialZoom;
                var oldWorldWidth = camera.VisibleWorldWidth;
                var oldWorldHeight = camera.VisibleWorldHeight;
                var newZoom = cameraPartialZoom + delta * cameraZoomSpeed;

                newZoom = Math.Min(newZoom, minCameraZoom);
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

        /// <summary>
        /// Converts user input into modifications in the local game state.
        /// </summary>
        /// <param name="sharedGameState">The shared game state for reference</param>
        /// <param name="localGameState">The local game state, for reference and modification.</param>
        public void HandleUserInput(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS)
        {
            UpdateCamera(sharedGameState, localGameState, elapsedMS);
        }
    }
}
