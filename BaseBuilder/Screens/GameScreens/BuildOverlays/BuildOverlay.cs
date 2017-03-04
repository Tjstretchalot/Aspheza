using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// A build overlay shows where a building will be placed
    /// </summary>
    public class BuildOverlay : MyGameComponent
    {
        protected BuildOverlayMenuComponent Menu;
        protected bool MenuVisible;

        protected static Color PlaceableColor = new Color(200, 200, 100);
        protected UnbuiltImmobileEntity BuildingToPlace;
        protected PointD2D CurrentPlaceLocation;
        protected bool CantPlace;

        protected int ScrollWheelAmountToNext;

        public BuildOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            CurrentPlaceLocation = new PointD2D(0, 0);
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 10);

            Menu = new BuildOverlayMenuComponent(content, graphics, graphicsDevice, spriteBatch);
        }

        public override void Draw(RenderContext context)
        {
            if (!MenuVisible)
                return;

            if (BuildingToPlace != null)
            {
                var placeLocationScreen = new PointD2D((int)context.Camera.PixelLocationOfWorldX(CurrentPlaceLocation.X), (int)context.Camera.PixelLocationOfWorldY(CurrentPlaceLocation.Y));

                BuildingToPlace.Render(context, placeLocationScreen, CantPlace ? Color.Red : PlaceableColor);
            }
            
            Menu.Draw(context);
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            if (!MenuVisible)
            {
                if (keysReleasedThisFrame.Contains(Keys.B))
                {
                    MenuVisible = true;
                    return true;
                }
            }else
            {
                if(keysReleasedThisFrame.Contains(Keys.Escape))
                {
                    if (Menu.Current != null)
                        Menu.ClearSelection();
                    else
                        MenuVisible = false;
                    return true;
                }else
                {
                    if (Menu.HandleKeyboardState(sharedGameState, localGameState, netContext, last, current, keysReleasedThisFrame))
                        return true;
                }
            }


            if(keysReleasedThisFrame.Contains(Keys.Delete))
            {
                var immobileSel = localGameState.SelectedEntity as ImmobileEntity;

                if(immobileSel != null)
                {
                    var order = netContext.GetPoolFromPacketType(typeof(DeconstructOrder)).GetGamePacketFromPool() as DeconstructOrder;
                    order.EntityID = immobileSel.ID;
                    localGameState.Orders.Add(order);
                    return true;
                }
            }

            return false;
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if (!MenuVisible)
                return false;

            if (Menu.HandleMouseState(sharedGameState, localGameState, netContext, last, current))
                return true;

            BuildingToPlace = Menu.Current;
            if (BuildingToPlace == null)
                return false;

            var desLeft = current.Position.X - ((BuildingToPlace.CollisionMesh.Right - BuildingToPlace.CollisionMesh.Left) * localGameState.Camera.Zoom) / 2.0;
            var desTop = current.Position.Y - ((BuildingToPlace.CollisionMesh.Bottom - BuildingToPlace.CollisionMesh.Top) * localGameState.Camera.Zoom) / 2.0;

            // Snap to tiles
            CurrentPlaceLocation.X = (int)Math.Round(localGameState.Camera.WorldLocationOfPixelX(desLeft));
            CurrentPlaceLocation.Y = (int)Math.Round(localGameState.Camera.WorldLocationOfPixelY(desTop));

            // Snap to half-tiles
            //CurrentPlaceLocation.X = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelX(desLeft) * 2)) / 2.0;
            //CurrentPlaceLocation.Y = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelY(desTop) * 2)) / 2.0;

            // Don't snap to grid
            //CurrentPlaceLocation.X = localGameState.Camera.WorldLocationOfPixelX(current.Position.X);
            //CurrentPlaceLocation.Y = localGameState.Camera.WorldLocationOfPixelY(current.Position.Y);

            CantPlace = false;
            if (CurrentPlaceLocation.X < 0 || CurrentPlaceLocation.Y < 0 ||
                CurrentPlaceLocation.X + BuildingToPlace.CollisionMesh.Right >= sharedGameState.World.TileWidth ||
                CurrentPlaceLocation.Y + BuildingToPlace.CollisionMesh.Bottom >= sharedGameState.World.TileHeight)
            {
                CantPlace = true;
            }
            else
            {
                foreach (var ent in sharedGameState.World.GetEntitiesAtLocation(BuildingToPlace.CollisionMesh, CurrentPlaceLocation))
                {
                    CantPlace = true;
                    break;
                }

                if(!CantPlace)
                {
                    CantPlace = !BuildingToPlace.TilesAreValid(sharedGameState, CurrentPlaceLocation);
                }
            }

            if (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released && !CantPlace)
            {
                var ent = BuildingToPlace.CreateEntity(new PointD2D(CurrentPlaceLocation.X, CurrentPlaceLocation.Y));

                var order = netContext.GetPoolFromPacketType(typeof(BuildOrder)).GetGamePacketFromPool() as BuildOrder;
                order.Entity = ent;
                localGameState.Orders.Add(order);

                Menu.ClearSelection();
            }else if(last.ScrollWheelValue != current.ScrollWheelValue)
            {
                var deltaScrollWheel = current.ScrollWheelValue - last.ScrollWheelValue;

                ScrollWheelAmountToNext -= Math.Abs(deltaScrollWheel);

                if(ScrollWheelAmountToNext <= 0)
                {
                    ScrollWheelAmountToNext = 100;

                    BuildingToPlace.TryRotate(Math.Sign(deltaScrollWheel));
                }
            }
            return true;
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.Update(sharedGameState, localGameState, netContext, timeMS);
            Menu.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        public override void Dispose()
        {
            base.Dispose();
            Menu.Dispose();
        }
    }
}
