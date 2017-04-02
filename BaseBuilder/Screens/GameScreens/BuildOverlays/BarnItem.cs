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
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class BarnItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = CreateTexture(context, "StorageBarn", sourceRect: new Rectangle(102, 0, 100, 86));

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Storage Barn", texture, @"A storage barn can
hold a lot of items.");
        }


        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            var built = new StorageBarn(new PointD2D(0, 0), -1, Direction.Left);
            return new UnbuiltImmobileEntityAsDelegator(() => new UnbuiltBuilding(new PointD2D(0, 0), -1,
                new List<Tuple<Material, int>> { Tuple.Create(Material.Wood, 30) }, built, 100000));
        }

        protected override void PreAddButton(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload, string thingName, TextureComponent texture, string description, ScrollableComponentAsLayoutManager parent)
        {
            parent.Children.Add(CreateMaterialCostsDisplay(context, redraw, redrawAndReload, Tuple.Create(Material.Wood, 30)));
        }
    }
}
