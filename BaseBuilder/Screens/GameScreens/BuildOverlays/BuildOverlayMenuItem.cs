using System;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Logic.Orders;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// Describes an item on the build menu
    /// </summary>
    public abstract class BuildOverlayMenuItem
    {
        protected ContentManager Content;
        protected GraphicsDeviceManager Graphics;
        protected GraphicsDevice GraphicsDevice;
        protected SpriteBatch SpriteBatch;

        /// <summary>
        /// The collision mesh for how this item is shown visually. Menu
        /// items do not respect camera zoom and will always draw the same
        /// size.
        /// </summary>
        public CollisionMeshD2D VisualCollisionMesh { get; protected set; }

        /// <summary>
        /// The location of this item relative to the top-left of the build overlay
        /// menu.
        /// </summary>
        public PointI2D Location;

        /// <summary>
        /// If this item is selectable
        /// </summary>
        public bool Selectable { get; protected set; }

        /// <summary>
        /// Set up the overlay without a location, specifying if it can be selected.
        /// </summary>
        /// <param name="selectable">If this item is selectable.</param>
        protected BuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, bool selectable)
        {
            Content = content;
            Graphics = graphics;
            GraphicsDevice = graphicsDevice;
            SpriteBatch = spriteBatch;

            Selectable = selectable;
        }
        
        /// <summary>
        /// Determines if this menu item contains the mouse at if the mouse
        /// is at the specified location, taking into account where the menu
        /// is on the screen and the scroll bar (yOffset), and if any clipping
        /// occurs.
        /// </summary>
        /// <param name="mouseLocation">The location of the menu on the screen</param>
        /// <param name="yOffset">The y offset caused by the scroll bar</param>
        /// <returns>If this item contains the mouse</returns>
        public bool ContainsMouse(Rectangle menuScreenLocation, int yOffset, Point mouse)
        {
            if (!menuScreenLocation.Contains(mouse))
                return false;

            var myDisplayPos = new PointD2D(menuScreenLocation.X + Location.X, menuScreenLocation.Y + Location.Y + yOffset);

            return VisualCollisionMesh.Contains(new PointD2D(mouse.X, mouse.Y), myDisplayPos);
        }
        
        /// <summary>
        /// Renders this build overlay menu item at it's current location, taking
        /// into account the menu's location and the scroll bar (yOffset). Clips
        /// the item if appropriate.
        /// </summary>
        /// <param name="menuScreenLocation">Where the menu is located on the screen</param>
        /// <param name="yOffset">The y offset caused by the scroll bar</param>
        /// <param name="selected">If this menu item is currently selected</param>
        public abstract void Render(Rectangle menuScreenLocation, int yOffset, bool selected);

        protected void DrawTexture(Rectangle menuScreenLocation, int yOffset, Texture2D texture, PointI2D textureLocation, Rectangle fullSrcRect, Color overlay)
        {
            Rectangle destRect, srcRect;
            // We need to take care of the following situations:
            // 1. We're not visible because we've scrolled past this.
            // 2. We're not visible because we haven't scroll to this.
            // 3. We're retreating into the top of the screen.
            // 4. We're just poking out of the bottom of the screen.
            // 5. We're fully visible
            // ----------------------------------------------------

            // Case 1:
            //   Our visual bottom is above the top of the menu
            if (textureLocation.Y + VisualCollisionMesh.Height + yOffset <= 0)
                return;
            // ----------------------------------------------------

            // Case 2:
            //   Our visual top is below the bottom of the menu
            if (textureLocation.Y + yOffset >= menuScreenLocation.Height)
                return;
            // ----------------------------------------------------

            // Case 3:
            //  Our visual top is above the top of the menu, but our visual bottom
            //  is inside the menu

            if (textureLocation.Y + yOffset < 0 && textureLocation.Y + VisualCollisionMesh.Height + yOffset > 0)
            {
                var heightVisible = textureLocation.Y + VisualCollisionMesh.Height + yOffset;

                destRect = new Rectangle(textureLocation.X + menuScreenLocation.X, menuScreenLocation.Y, (int)VisualCollisionMesh.Width, (int)heightVisible);
                srcRect = new Rectangle(fullSrcRect.X, fullSrcRect.Y + (int)(VisualCollisionMesh.Height - heightVisible), (int)VisualCollisionMesh.Width, (int)heightVisible);

                SpriteBatch.Draw(texture, destRect, srcRect, overlay);
                return;
            }

            // Case 4:
            //   Our visual top is inside the menu, but our visual bottom is below
            //   the menu

            if (textureLocation.Y + VisualCollisionMesh.Height + yOffset > menuScreenLocation.Height && textureLocation.Y + yOffset < menuScreenLocation.Height)
            {
                var heightVisible = menuScreenLocation.Height - (textureLocation.Y + yOffset);

                destRect = new Rectangle(textureLocation.X + menuScreenLocation.X, menuScreenLocation.Y + menuScreenLocation.Height - heightVisible, (int)VisualCollisionMesh.Width, heightVisible);
                srcRect = new Rectangle(fullSrcRect.X, fullSrcRect.Y, (int)VisualCollisionMesh.Width, heightVisible);

                SpriteBatch.Draw(texture, destRect, srcRect, overlay);
                return;
            }

            // Case 5:
            //   We're fully visible

            destRect = new Rectangle(textureLocation.X + menuScreenLocation.X, textureLocation.Y + menuScreenLocation.Y + yOffset, (int)VisualCollisionMesh.Width, (int)VisualCollisionMesh.Height);
            srcRect = new Rectangle(fullSrcRect.X, fullSrcRect.Y, (int)VisualCollisionMesh.Width, (int)VisualCollisionMesh.Height);

            SpriteBatch.Draw(texture, destRect, srcRect, overlay);
        }
        /// <summary>
        /// Updates any animations this menu item does
        /// </summary>
        /// <param name="elapsedMS">Elapsed time this frame</param>
        /// <param name="selected">If this menu item is currently selected</param>
        public abstract void Update(int elapsedMS, bool selected);

        /// <summary>
        /// Creates an unbuilt immobile entity version of this menu item. Will
        /// only be called if this menu item is selectable.
        /// </summary>
        /// <param name="gameState">The current shared game state</param>
        /// <returns>Unbuilt immobile entity version of this item</returns>
        public abstract UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState);

        /// <summary>
        /// Tries to build the entity at the specified location with the specified unbuilt immobile entity.
        /// </summary>
        /// <param name="sharedGameState">Shared game state</param>
        /// <param name="localGameState">Local game state</param>
        /// <param name="netContext">Net context</param>
        /// <param name="placeLoc">Top-left location for the building</param>
        /// <param name="buildingToPlace">The building to place</param>
        /// <returns>If an entity was built</returns>
        public virtual bool TryBuildEntity(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, PointD2D placeLoc, UnbuiltImmobileEntity buildingToPlace)
        {
            var ent = buildingToPlace.CreateEntity(new PointD2D(placeLoc.X, placeLoc.Y));

            var order = netContext.GetPoolFromPacketType(typeof(BuildOrder)).GetGamePacketFromPool() as BuildOrder;
            order.Entity = ent;
            localGameState.Orders.Add(order);
            return true;
        }
    }
}