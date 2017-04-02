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
using BaseBuilder.Engine.State.Resources;

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
            var built = new ChickenCoop(new PointD2D(0, 0), -1);
            return new UnbuiltImmobileEntityAsDelegator(() => new UnbuiltBuilding(new PointD2D(0, 0), -1,
                new List<Tuple<Material, int>> { Tuple.Create(Material.Wood, 10), Tuple.Create(Material.Bread, 5), Tuple.Create(Material.CarrotSeed, 5) }, built, 100000));
        }

        protected override void PreAddButton(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload, string thingName, TextureComponent texture, string description, ScrollableComponentAsLayoutManager parent)
        {
            parent.Children.Add(CreateMaterialCostsDisplay(context, redraw, redrawAndReload, Tuple.Create(Material.Wood, 10), Tuple.Create(Material.Bread, 5),
                Tuple.Create(Material.CarrotSeed, 5)));
        }
    }
}
