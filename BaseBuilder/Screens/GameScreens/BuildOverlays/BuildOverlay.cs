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

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// A build overlay shows where a building will be placed
    /// </summary>
    public class BuildOverlay : MyGameComponent
    {
        protected UnbuiltImmobileEntity BuildingToPlace;
        protected PointD2D CurrentPlaceLocation;
        protected bool CantPlace;

        public BuildOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            CurrentPlaceLocation = new PointD2D(0, 0);
            Init(new PointI2D(0, 0), new PointI2D(0, 0), 5);
        }

        public override void Draw(RenderContext context)
        {
            if(BuildingToPlace != null && !CantPlace)
            {
                var placeLocationScreen = new PointD2D((int)context.Camera.PixelLocationOfWorldX(CurrentPlaceLocation.X), (int)context.Camera.PixelLocationOfWorldY(CurrentPlaceLocation.Y));

                BuildingToPlace.Render(context, placeLocationScreen, Color.DarkBlue);
            }
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            if (BuildingToPlace == null)
            {
                if (last.IsKeyDown(Keys.B) && current.IsKeyUp(Keys.B))
                {
                    BuildingToPlace = new UnbuiltImmobileEntityAsDelegator(() => new House(new PointD2D(0, 0), sharedGameState.GetUniqueEntityID()));
                    return true;
                }
            }else
            {
                if(last.IsKeyDown(Keys.Escape) && current.IsKeyUp(Keys.Escape))
                {
                    BuildingToPlace = null;
                    return true;
                }
            }

            return false;
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if (BuildingToPlace == null)
                return false;

            // Snap to half-tiles
            CurrentPlaceLocation.X = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelX(current.Position.X) * 2)) / 2.0;
            CurrentPlaceLocation.Y = ((int)Math.Round(localGameState.Camera.WorldLocationOfPixelY(current.Position.Y) * 2)) / 2.0;

            // Don't snap to grid
            //CurrentPlaceLocation.X = localGameState.Camera.WorldLocationOfPixelX(current.Position.X);
            //CurrentPlaceLocation.Y = localGameState.Camera.WorldLocationOfPixelY(current.Position.Y);

            CantPlace = false;
            foreach (var ent in sharedGameState.World.GetEntitiesAtLocation(BuildingToPlace.CollisionMesh, CurrentPlaceLocation))
            {
                CantPlace = true;
                break;
            }

            if(CantPlace)
            {
                foreach (var tmp in sharedGameState.World.GetEntitiesAtLocation(BuildingToPlace.CollisionMesh, CurrentPlaceLocation)) { } // debug
            }

            if (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released && !CantPlace)
            {
                var ent = BuildingToPlace.CreateEntity(new PointD2D(CurrentPlaceLocation.X, CurrentPlaceLocation.Y));

                var order = netContext.GetPoolFromPacketType(typeof(BuildOrder)).GetGamePacketFromPool() as BuildOrder;
                order.Entity = ent;
                localGameState.Orders.Add(order);
            }
            return true;
        }
    }
}
