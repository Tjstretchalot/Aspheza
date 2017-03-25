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
using BaseBuilder.Engine.World.Entities.ImmobileEntities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays.BuildOverlays2
{
    public class FarmItem2 : BuildOverlayMenuItem2
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var weakMenu = new WeakReference<BuildOverlayImpl>(menu);
            var layout = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.CenteredSuggested, 5);
            var texture = CreateTexture(context, "Farm");
            layout.Children.Add(Wrap(texture));
            var button = CreateButton(context, redraw, redrawAndReload, "Build");
            button.PressReleased += (sender, args) =>
            {
                BuildOverlayImpl strongMenu;
                if (!weakMenu.TryGetTarget(out strongMenu))
                    return;

                strongMenu.SetSelectedItem(this);
            };
            layout.Children.Add(Wrap(button));
            return layout;
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Farm(new PointD2D(0, 0), -1));
        }
    }
}
