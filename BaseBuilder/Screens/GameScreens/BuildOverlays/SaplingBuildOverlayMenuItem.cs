using System;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.State.Resources;
using System.Collections.Generic;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class SaplingBuildOverlayMenuItem : SimpleBuildOverlayMenuItem
    {
        protected Rectangle SourceRect;

        public SaplingBuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch, true, "materials")
        {
            VisualCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(32, 32) });
            SourceRect = new Rectangle(66, 66, 32, 32);
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Tree(new PointD2D(0, 0), -1, TreeSize.Sapling, TreeStyle.Pointy, TreeColor.Green));
        }

        public override bool TryBuildEntity(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, PointD2D placeLoc, UnbuiltImmobileEntity buildingToPlace)
        {
            var worker = localGameState.SelectedEntity as CaveManWorker;

            if (worker.Inventory.GetAmountOf(Material.Sapling) < 1)
                return false;

            if (!worker.CollisionMesh.Intersects(buildingToPlace.CollisionMesh, worker.Position, placeLoc) && !worker.CollisionMesh.MinDistanceShorterThan(buildingToPlace.CollisionMesh, 1.0, worker.Position, placeLoc))
                return false;

            var success =  base.TryBuildEntity(sharedGameState, localGameState, netContext, placeLoc, buildingToPlace);

            if (!success)
                return false;

            worker.Inventory.RemoveMaterial(Material.Sapling, 1);
            return true;
        }

        public override void Render(Rectangle menuScreenLocation, int yOffset, bool selected)
        {
            var texture = Content.Load<Texture2D>(SpriteName);

            DrawTexture(menuScreenLocation, yOffset, texture, Location, SourceRect, selected ? Color.Azure : Color.White);
        }
    }
}