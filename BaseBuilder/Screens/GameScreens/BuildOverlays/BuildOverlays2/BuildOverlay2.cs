using System;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components.ScrollableComponents;
using BaseBuilder.Screens.GameScreens.TaskOverlays;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays.BuildOverlays2
{
    public class BuildOverlay2 : MyGameComponent
    {
        public ScrollableComponentWrapper ScrollableComponent;
        public BuildOverlayImpl BuildOverlayImpl;

        public UnbuiltImmobileEntity BuildingToPlace;
        public PointD2D PlaceLocation;
        public bool CantPlace;
        public int ScrollWheelAmountToNext;

        public BuildOverlay2(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 10);

            PlaceLocation = new PointD2D(0, 0);
        }

        public override void PreDraw(RenderContext renderContext)
        {
            ScrollableComponent?.PreDraw(renderContext);
        }

        public override void Draw(RenderContext context)
        {
            if (ScrollableComponent == null)
                return;
            
            if(BuildingToPlace != null)
            {
                var drawLocation = new PointD2D((int)context.Camera.PixelLocationOfWorldX(PlaceLocation.X), (int)context.Camera.PixelLocationOfWorldY(PlaceLocation.Y));

                BuildingToPlace.Render(context, drawLocation, CantPlace ? Color.Red : new Color(200, 200, 100));
            }

            ScrollableComponent.Draw(context);
        }

        public void InitMenu()
        {
            BuildOverlayImpl = new BuildOverlayImpl();
            ScrollableComponent = new ScrollableComponentWrapper(Content, Graphics, GraphicsDevice, SpriteBatch, BuildOverlayImpl,
                new PointI2D(GraphicsDevice.Viewport.Width - 300, 25), new PointI2D(250, GraphicsDevice.Viewport.Height - 250), 1);

            BuildOverlayImpl.RedrawRequired += (sender, args) => RedrawMenu();
            BuildOverlayImpl.SelectionChanged += (sender, args) =>
            {
                BuildingToPlace = BuildOverlayImpl.Selected;
            };
        }

        public void RedrawMenu()
        {
            ScrollableComponent.Invalidate();
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            ScrollableComponent?.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            if (ScrollableComponent == null)
            {
                if (keysReleasedThisFrame.Contains(Keys.B))
                {
                    InitMenu();
                    return true;
                }
                return false;
            }
            else
            {
                if (keysReleasedThisFrame.Any((k) => k == Keys.B || k == Keys.Escape))
                {
                    if (BuildingToPlace != null)
                    {
                        BuildOverlayImpl.ResetSelected();
                    }
                    else
                    {
                        DisposeMenu();
                    }
                    return true;
                }
                else
                {
                    return ScrollableComponent.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame);
                }
            }
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool handledScroll)
        {
            if (ScrollableComponent == null)
                return;

            ScrollableComponent.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref handledScroll);

            if (BuildingToPlace == null)
                return;
            if (handled)
                return;

            handled = true;
            
            var desLeft = current.Position.X - ((BuildingToPlace.CollisionMesh.Right - BuildingToPlace.CollisionMesh.Left) * localGameState.Camera.Zoom) / 2.0;
            var desTop = current.Position.Y - ((BuildingToPlace.CollisionMesh.Bottom - BuildingToPlace.CollisionMesh.Top) * localGameState.Camera.Zoom) / 2.0;

            // Snap to tiles
            PlaceLocation.X = (int)Math.Round(localGameState.Camera.WorldLocationOfPixelX(desLeft));
            PlaceLocation.Y = (int)Math.Round(localGameState.Camera.WorldLocationOfPixelY(desTop));

            // Snap to half-tiles
            //CurrentPlaceLocation.X = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelX(desLeft) * 2)) / 2.0;
            //CurrentPlaceLocation.Y = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelY(desTop) * 2)) / 2.0;

            // Don't snap to grid
            //CurrentPlaceLocation.X = localGameState.Camera.WorldLocationOfPixelX(current.Position.X);
            //CurrentPlaceLocation.Y = localGameState.Camera.WorldLocationOfPixelY(current.Position.Y);

            CantPlace = false;
            if (PlaceLocation.X < 0 || PlaceLocation.Y < 0 ||
                PlaceLocation.X + BuildingToPlace.CollisionMesh.Right >= sharedGameState.World.TileWidth ||
                PlaceLocation.Y + BuildingToPlace.CollisionMesh.Bottom >= sharedGameState.World.TileHeight)
            {
                CantPlace = true;
            }
            else
            {
                foreach (var ent in sharedGameState.World.GetEntitiesAtLocation(BuildingToPlace.CollisionMesh, PlaceLocation))
                {
                    CantPlace = true;
                    break;
                }

                if (!CantPlace)
                {
                    CantPlace = !BuildingToPlace.TilesAreValid(sharedGameState, PlaceLocation);
                }
            }

            if (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released && !CantPlace)
            {
                BuildOverlayImpl.TryBuildEntity(sharedGameState, localGameState, netContext, PlaceLocation, BuildingToPlace);
                BuildOverlayImpl.ResetSelected();
            }
            else if (!handledScroll && last.ScrollWheelValue != current.ScrollWheelValue)
            {
                handledScroll = true;
                var deltaScrollWheel = current.ScrollWheelValue - last.ScrollWheelValue;

                ScrollWheelAmountToNext -= Math.Abs(deltaScrollWheel);

                if (ScrollWheelAmountToNext <= 0)
                {
                    ScrollWheelAmountToNext = 100;

                    BuildingToPlace.TryRotate(Math.Sign(deltaScrollWheel));
                }
            }
        }

        public void DisposeMenu()
        {
            ScrollableComponent.Dispose();
            BuildOverlayImpl = null;
            ScrollableComponent = null;
            BuildingToPlace = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposeMenu();
        }
    }
}
