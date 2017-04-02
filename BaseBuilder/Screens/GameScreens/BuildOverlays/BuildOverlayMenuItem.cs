using BaseBuilder.Engine.Context;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.Components.ScrollableComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using BaseBuilder.Screens.Components.ScrollableComponents.Distinguishers;
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// Describes a single building on the build menu
    /// </summary>
    public abstract class BuildOverlayMenuItem
    {
        protected static Color[] UnselectedColors = new[] { new Color(144, 144, 144), new Color(77, 77, 77), new Color(188, 188, 188) };
        protected static Color[] SelectedColors = new[] { new Color(144, 144, 144), new Color(35, 35, 255), new Color(188, 188, 188) };
        
        protected WeakReference<ScrollableComponentAsDistinguisher> Distinguisher;
        protected bool ShowingSelected;

        protected BuildOverlayMenuItem()
        {
            Distinguisher = new WeakReference<ScrollableComponentAsDistinguisher>(null);
        }

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

        protected virtual IScrollableComponent CreateMaterialCostsDisplay(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, 
            params Tuple<Material, int>[] materials)
        {
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 7);

            ScrollableComponentAsLayoutManager current = null;
            for(int i = 0; i < materials.Length; i++)
            {
                var tup = materials[i];
                if(i % 3 == 0)
                {
                    current = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 7);
                    layout.Children.Add(current);
                }

                current.Children.Add(Label(context, tup.Item2.ToString(), Wrap(tup.Item1.GetAsTextureComponent(context.Content)), false));
            }

            return layout;
        }

        protected virtual void PreAddButton(RenderContext context, BuildOverlayImpl menu, EventHandler redraw,
            EventHandler redrawAndReload, string thingName, TextureComponent texture, string description, ScrollableComponentAsLayoutManager layout)
        { }

        protected IScrollableComponent CreateMenuItemFromTexture(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, 
            EventHandler redrawAndReload, string thingName, TextureComponent texture, string description)
        {
            var weakMenu = new WeakReference<BuildOverlayImpl>(menu);
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 7);

            layout.Children.Add(CreatePadding(240, 3));
            layout.Children.Add(Wrap(CreateText(context, thingName)));
            layout.Children.Add(Wrap(texture));
            layout.Children.Add(Wrap(CreateText(context, description, true)));
            PreAddButton(context, menu, redraw, redrawAndReload, thingName, texture, description, layout);
            var button = CreateButton(context, redraw, redrawAndReload, "Build");
            button.PressReleased += (sender, args) =>
            {
                BuildOverlayImpl strongMenu;
                if (!weakMenu.TryGetTarget(out strongMenu))
                    return;

                strongMenu.SetSelectedItem(this);
            };
            layout.Children.Add(Wrap(button));
            layout.Children.Add(CreatePadding(1, 3));


            var prettyBackground = new ScrollableComponentAsDistinguisher(layout,
                new List<IDistinguisherComponent> { new RoundedRectDistinguisher(UnselectedColors[0], UnselectedColors[1], UnselectedColors[2], 15, 2, 1) },
                10,
                2);
            ShowingSelected = false;
            Distinguisher.SetTarget(prettyBackground);

            menu.SelectionChanged += (sender, args) =>
            {
                ScrollableComponentAsDistinguisher disting;
                if (!Distinguisher.TryGetTarget(out disting))
                    return;

                if (disting.Disposed)
                    return;

                BuildOverlayImpl strongMenu;
                if (!weakMenu.TryGetTarget(out strongMenu))
                    return;

                var nowSelected = strongMenu.SelectedItem == this;
                if (nowSelected == ShowingSelected)
                    return;

                ShowingSelected = nowSelected;

                var newColors = nowSelected ? SelectedColors : UnselectedColors;
                var newBackground = new RoundedRectDistinguisher(newColors[0], newColors[1], newColors[2], 15, 2, 1);

                disting.Distinguishers[0].Dispose();
                disting.Distinguishers[0] = newBackground;

                redrawAndReload?.Invoke(null, EventArgs.Empty);
            };
            return prettyBackground;
        }
    }
}
