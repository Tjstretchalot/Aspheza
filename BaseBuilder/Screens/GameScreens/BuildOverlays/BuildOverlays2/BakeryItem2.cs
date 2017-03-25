using System;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Screens.Components.ScrollableComponents;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Screens.GComponents.ScrollableComponents;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.Components.ScrollableComponents.Distinguishers;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays.BuildOverlays2
{
    public class BakeryItem2 : BuildOverlayMenuItem2
    {
        protected static Color[] UnselectedColors = new [] { new Color(144, 144, 144), new Color(77, 77, 77), new Color(188, 188, 188) };
        protected static Color[] SelectedColors = new[] { new Color(122, 122, 122), new Color(188, 188, 188), new Color(77, 77, 77) };

        protected WeakReference<ScrollableComponentAsDistinguisher> Distinguisher;
        protected bool ShowingSelected;

        public BakeryItem2()
        {
            Distinguisher = new WeakReference<ScrollableComponentAsDistinguisher>(null);
        }

        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var weakMenu = new WeakReference<BuildOverlayImpl>(menu);
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 7);
            var texture = CreateTexture(context, "Bakery");
            layout.Children.Add(Wrap(CreateText(context, "Bakery")));
            layout.Children.Add(Wrap(texture));
            layout.Children.Add(Wrap(CreateText(context, @"A bakery combines sugar, 
eggs, and wheat into bread.", true)));
            var button = CreateButton(context, redraw, redrawAndReload, "Build");
            button.PressReleased += (sender, args) =>
            {
                BuildOverlayImpl strongMenu;
                if (!weakMenu.TryGetTarget(out strongMenu))
                    return;

                strongMenu.SetSelectedItem(this);
            };
            layout.Children.Add(Wrap(button));

            var prettyBackground = new ScrollableComponentAsDistinguisher(layout, 
                new List<IDistinguisherComponent> { new RoundedRectDistinguisher(UnselectedColors[0], UnselectedColors[1], UnselectedColors[2], 15, 2, 1) },
                10,
                10);
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

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Bakery(new PointD2D(0, 0), -1));
        }
    }
}
