﻿using BaseBuilder.Engine.Context;
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
    public class TavernItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = ScrollableComponentUtils.CreateTexture(context, "Tavern");

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Tavern", texture, @"A tavern can create
ale from fruits and
rum from sugar.");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Tavern(new PointD2D(0, 0), -1));
        }
    }
}