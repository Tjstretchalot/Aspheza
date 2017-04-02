using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Screens.Components.ScrollableComponents;
using static BaseBuilder.Screens.Components.ScrollableComponents.ScrollableComponentUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class TempleItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = CreateTexture(context, "Temple");

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Temple", texture, @"Useless");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            var built = new Temple(new PointD2D(0, 0), -1);
            return new UnbuiltImmobileEntityAsDelegator(() => new UnbuiltBuilding(new PointD2D(0, 0), -1,
                new List<Tuple<Material, int>> { Tuple.Create(Material.Wood, 10) }, built, 100000));
        }

        protected override void PreAddButton(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload, string thingName, TextureComponent texture, string description, ScrollableComponentAsLayoutManager parent)
        {
            parent.Children.Add(CreateMaterialCostsDisplay(context, redraw, redrawAndReload, Tuple.Create(Material.Wood, 10)));
        }
    }
}
