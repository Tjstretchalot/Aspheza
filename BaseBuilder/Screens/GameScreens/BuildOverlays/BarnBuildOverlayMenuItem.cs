using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class BarnBuildOverlayMenuItem : SimpleBuildOverlayMenuItem
    {
        public BarnBuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch, true, "StorageBarn")
        {
            VisualCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D>{
                new RectangleD2D(100, 86)
            });
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new StorageBarn(new PointD2D(0, 0), -1, Direction.Left));
        }
    }
}
