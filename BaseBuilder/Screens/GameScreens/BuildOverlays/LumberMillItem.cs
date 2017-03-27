using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Screens.Components.ScrollableComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class LumberMillItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = ScrollableComponentUtils.CreateTexture(context, "LumberMill");

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "LumberMill", texture, @"A lumbermill chops
wood into chopped 
wood.");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new LumberMill(new PointD2D(0, 0), -1));
        }
    }
}
