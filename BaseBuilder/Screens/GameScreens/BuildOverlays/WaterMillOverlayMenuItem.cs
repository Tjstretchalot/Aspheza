using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class WaterMillOverlayMenuItem : SimpleBuildOverlayMenuItem
    {
        public WaterMillOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch, true, "WaterMill")
        {
            VisualCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D>
            {
                new RectangleD2D(170, 94, 4, 2)
            });
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltWaterMillEntity(() => new WaterMill(new PointD2D(0, 0), gameState.GetUniqueEntityID()));
        }
    }
}
