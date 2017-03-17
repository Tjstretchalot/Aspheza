using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class LibraryBuildOverlayMenuItem : SimpleBuildOverlayMenuItem
    {
        public LibraryBuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch, true, "Library")
        {
            VisualCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(132, 164) });
        }

        public override UnbuiltImmobileEntity CreateUnbuiltImmobileEntity(SharedGameState gameState)
        {
            return new UnbuiltImmobileEntityAsDelegator(() => new Library(new PointD2D(0, 0), -1));
        }
    }
}
