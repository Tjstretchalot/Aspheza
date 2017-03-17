using System;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D.Double;
using System.Collections.Generic;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class FarmBuildOverlayMenuItem : SimpleBuildOverlayMenuItem
    {
        public FarmBuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch, true, "farms")
        {
            VisualCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D>
            {
                new RectangleD2D(64, 68)
            });
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Farm(new PointD2D(0, 0), -1));
        }
    }
}