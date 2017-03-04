using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class UnbuiltWaterMillEntity : UnbuiltImmobileEntityAsDelegator
    {
        protected static CollisionMeshD2D LandTileCollisionMesh;
        protected static CollisionMeshD2D WaterTileCollisionMesh;

        static UnbuiltWaterMillEntity()
        {
            LandTileCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> {
                new RectangleD2D(114.0 / 16.0, 85.0 / 16.0)
                });
            WaterTileCollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> {
                new RectangleD2D(28.0 / 16.0, 85.0 / 16.0, 128.0 / 16.0, 0)
             });
        }

        public UnbuiltWaterMillEntity(Func<ImmobileEntity> entityCreator) : base(entityCreator)
        {
        }

        public override bool TilesAreValid(SharedGameState gameState, PointD2D placeLocation)
        {
            var tiles = new List<PointI2D>();
            LandTileCollisionMesh.TilesIntersectedAt(placeLocation, tiles);
            foreach(var tilePos in tiles)
            {
                var tile = gameState.World.TileAt(tilePos.X, tilePos.Y);

                if (!tile.Ground)
                    return false;
             }

            WaterTileCollisionMesh.TilesIntersectedAt(placeLocation, tiles);
            foreach(var tilePos in tiles)
            {
                var tile = gameState.World.TileAt(tilePos.X, tilePos.Y);

                if (!tile.Water)
                    return false;
            }

            return true;
        }
    }
}
