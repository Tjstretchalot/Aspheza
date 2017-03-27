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

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class BakeryItem : BuildOverlayMenuItem
    {
        public BakeryItem()
        {
        }

        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = CreateTexture(context, "Bakery");

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Bakery", texture, @"A bakery combines one 
sugar, one egg, and 
one wheat into one 
bread.");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Bakery(new PointD2D(0, 0), -1));
        }
    }
}
