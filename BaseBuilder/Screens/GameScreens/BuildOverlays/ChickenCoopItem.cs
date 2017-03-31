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
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class ChickenCoopItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = CreateTexture(context, "ChickenCoop", new PointI2D(121, 118), new Rectangle(0, 0, 242, 236));

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Chicken Coop", texture, @"A chicken coop lets
chickens produce 
eggs.");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new ChickenCoop(new PointD2D(0, 0), -1));
        }
    }
}
