using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Screens.Components.ScrollableComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays.BuildOverlays2
{
    /// <summary>
    /// Describes a single building on the build menu
    /// </summary>
    public abstract class BuildOverlayMenuItem2
    {
        /// <summary>
        /// Builds the scrollable component
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="menu">Menu</param>
        /// <param name="redraw">Should be invoked when this component needs to redraw</param>
        /// <param name="redrawAndReload">Should be invoked when this component resizes</param>
        /// <returns></returns>
        public abstract IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload);

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
