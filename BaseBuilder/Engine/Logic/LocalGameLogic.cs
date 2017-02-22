using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Screens.GameScreens;
using Microsoft.Xna.Framework;
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
        protected bool previousDKeyState;
        protected Microsoft.Xna.Framework.Input.ButtonState previousLeftMouseButtonState;
        protected Microsoft.Xna.Framework.Input.ButtonState previousRightMouseButtonState;

        protected int minCameraZoom;
        protected double cameraSpeed;
        protected double cameraZoomSpeed;
        protected double cameraPartialZoom;
        protected PointD2D cameraPartialTopLeft;
        
        MouseState? mouseLast;
        KeyboardState? keyboardLast;
        List<Keys> keysPressedReusable;

        public LocalGameLogic()
        {
            cameraSpeed = 0.01;
            minCameraZoom = 32;
            cameraZoomSpeed = 0.01;

            cameraPartialZoom = 8;

            cameraPartialTopLeft = new PointD2D(0, 0);
            keysPressedReusable = new List<Keys>();
        }

        public void AddComponent(LocalGameState localGameState, IMyGameComponent component)
        {
            LogicUtils.BinaryInsert(localGameState.Components, component, (c1, c2) => c1.Z - c2.Z); // ascending order
        }

        public void RemoveComponent(LocalGameState localGameState, IMyGameComponent component)
        {
            localGameState.Components.Remove(component);
        }

        public void UpdateComponentZ(LocalGameState localGameState, IMyGameComponent component)
        {
            RemoveComponent(localGameState, component);
            AddComponent(localGameState, component);
        }
        
        protected void UpdateComponents(SharedGameState sharedGameState, LocalGameState localGameState)
        {
            var mouseCurr = Mouse.GetState();
            var keyboardCurr = Keyboard.GetState();

            if (mouseLast.HasValue && keyboardLast.HasValue)
            {
                var keysCurr = keyboardCurr.GetPressedKeys();

                keysPressedReusable.Clear();
                keysPressedReusable.AddRange(keyboardLast.Value.GetPressedKeys());
                keysPressedReusable.RemoveAll((k) => keysCurr.Contains(k));
                bool mouseHandled = false, keyboardHandled = false;
                for (int i = localGameState.Components.Count - 1; i >= 0; i--)
                {
                    var comp = localGameState.Components[i];
                    if (!mouseHandled && comp.HandleMouseState(sharedGameState, localGameState, mouseLast.Value, mouseCurr))
                        mouseHandled = true;
                    if (!keyboardHandled && comp.HandleKeyboardState(sharedGameState, localGameState, keyboardLast.Value, keyboardCurr, keysPressedReusable))
                        keyboardHandled = true;

                    if (mouseHandled && keyboardHandled)
                        return;
                }
            }

            foreach(var comp in localGameState.Components)
            {
                comp.Update(sharedGameState, localGameState);
            }

            mouseLast = mouseCurr;
            keyboardLast = keyboardCurr;
        }

        protected void CheckForMoveOrder(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS)
        {
            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if ((Mouse.GetState().RightButton != previousRightMouseButtonState) && (previousRightMouseButtonState == ButtonState.Pressed))
            {
                double mousePixelX = Mouse.GetState().Position.X, mousePixelY = Mouse.GetState().Position.Y;
                double mouseWorldX, mouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);
                mouseWorldX = (int)mouseWorldX;
                mouseWorldY = (int)mouseWorldY;
                
                if (localGameState.SelectedEntity != null)
                {
                    //Console.WriteLine($"Issue move order to enitity ID {localGameState.SelectedEntity.ID} To ({mouseWorldX} {mouseWorldY}).");
                    if(!Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    {
                        var cancelTasksOrder = netContext.GetPoolFromPacketType(typeof(CancelTasksOrder)).GetGamePacketFromPool() as CancelTasksOrder;
                        cancelTasksOrder.EntityID = localGameState.SelectedEntity.ID;
                        localGameState.Orders.Add(cancelTasksOrder);
                    }
                    var moveOrder = netContext.GetPoolFromPacketType(typeof(MoveOrder)).GetGamePacketFromPool() as MoveOrder;
                    moveOrder.EntityID = localGameState.SelectedEntity.ID;
                    moveOrder.End = new PointI2D((int)mouseWorldX, (int)mouseWorldY);
                    localGameState.Orders.Add(moveOrder);
                }
            }
            previousRightMouseButtonState = Mouse.GetState().RightButton;
        }

        protected void CheckForSelect(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS)
        {
            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if ((Mouse.GetState().LeftButton != previousLeftMouseButtonState) && (previousLeftMouseButtonState == ButtonState.Pressed))
            {
                double mousePixelX = Mouse.GetState().Position.X, mousePixelY = Mouse.GetState().Position.Y;
                double mouseWorldX, mouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);

                var entity = world.GetMobileEntityAtLocation(new PointD2D(mouseWorldX, mouseWorldY));
                if (entity == null)
                {
                    if (localGameState.SelectedEntity == null)
                    {
                        previousLeftMouseButtonState = Mouse.GetState().LeftButton;
                        return;
                    }

                    localGameState.SelectedEntity.Selected = false;
                    localGameState.SelectedEntity = entity;
                    previousLeftMouseButtonState = Mouse.GetState().LeftButton;
                    return;
                }

                if (localGameState.SelectedEntity == null)
                {
                    localGameState.SelectedEntity = entity;
                    entity.Selected = true;
                } else
                {
                    localGameState.SelectedEntity.Selected = false;
                    localGameState.SelectedEntity = entity;
                    entity.Selected = true;
                }
            }
            previousLeftMouseButtonState = Mouse.GetState().LeftButton;
        }

        protected void CheckForColisionDebugUpadate(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS)
        {
            if ((Keyboard.GetState().IsKeyDown(Keys.D) != previousDKeyState) && !previousDKeyState && (Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt)))
            {
                localGameState.CollisionDebug = !localGameState.CollisionDebug;
            }
            previousDKeyState = Keyboard.GetState().IsKeyDown(Keys.D);
        }
        
        protected virtual void UpdateCamera(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS)
        {
            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                cameraPartialTopLeft.X = Math.Max(0, cameraPartialTopLeft.X - cameraSpeed * elapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                cameraPartialTopLeft.X = Math.Min(world.TileWidth - camera.VisibleWorldWidth - 1, cameraPartialTopLeft.X + cameraSpeed * elapsedMS);

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                cameraPartialTopLeft.Y = Math.Max(0, cameraPartialTopLeft.Y - cameraSpeed * elapsedMS);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                cameraPartialTopLeft.Y = Math.Min(world.TileHeight - camera.VisibleWorldHeight - 1, cameraPartialTopLeft.Y + cameraSpeed * elapsedMS);


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
                cameraPartialTopLeft.X = Math.Min(cameraPartialTopLeft.X, world.TileWidth - camera.VisibleWorldWidth - 1);
                cameraPartialTopLeft.Y = Math.Min(cameraPartialTopLeft.Y, world.TileHeight - camera.VisibleWorldHeight - 1);
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
        public void HandleUserInput(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS)
        {
            CheckForColisionDebugUpadate(sharedGameState, localGameState, elapsedMS);
            CheckForSelect(sharedGameState, localGameState, elapsedMS);
            UpdateCamera(sharedGameState, localGameState, elapsedMS);
            CheckForMoveOrder(sharedGameState, localGameState, netContext, elapsedMS);
            UpdateComponents(sharedGameState, localGameState);
        }
    }
}
