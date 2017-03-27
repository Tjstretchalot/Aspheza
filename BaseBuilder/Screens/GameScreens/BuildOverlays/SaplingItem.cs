using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Screens.Components.ScrollableComponents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class SaplingItem : BuildOverlayMenuItem
    {
        public override IScrollableComponent BuildComponent(RenderContext context, BuildOverlayImpl menu, EventHandler redraw, EventHandler redrawAndReload)
        {
            var texture = ScrollableComponentUtils.CreateTexture(context, "materials", sourceRect: new Rectangle(66, 0, 32, 32));

            return CreateMenuItemFromTexture(context, menu, redraw, redrawAndReload, "Tree", texture, @"A sapling grows into
a tree. Must be
selecting a worker
with a sapling and
plant nearby.");
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Tree(new PointD2D(0, 0), -1, TreeSize.Sapling, TreeStyle.Pointy, TreeColor.Green));
        }

        public override bool TryBuildEntity(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, PointD2D placeLoc, UnbuiltImmobileEntity buildingToPlace)
        {
            var worker = localGameState.SelectedEntity as CaveManWorker;

            if (worker == null)
                return false;

            if (worker.Inventory.GetAmountOf(Material.Sapling) < 1)
                return false;

            if (!worker.CollisionMesh.Intersects(buildingToPlace.CollisionMesh, worker.Position, placeLoc) && !worker.CollisionMesh.MinDistanceShorterThan(buildingToPlace.CollisionMesh, 1.0, worker.Position, placeLoc))
                return false;

            var success = base.TryBuildEntity(sharedGameState, localGameState, netContext, placeLoc, buildingToPlace);

            if (!success)
                return false;

            worker.Inventory.RemoveMaterial(Material.Sapling, 1);
            return true;
        }
    }
}
