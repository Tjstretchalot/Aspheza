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
    public class LibraryItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = ScrollableComponentUtils.CreateTexture(context, "Library");

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Libary", texture, @"Useless");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Library(new PointD2D(0, 0), -1));
        }
    }
}