using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public class UnbuiltImmobileEntityAsDelegator : UnbuiltImmobileEntity
    {
        protected Func<ImmobileEntity> EntityCreator;
        protected ImmobileEntity CurrentEntity;

        public UnbuiltImmobileEntityAsDelegator(Func<ImmobileEntity> entityCreator)
        {
            EntityCreator = entityCreator;
            CurrentEntity = entityCreator();
        }

        public CollisionMeshD2D CollisionMesh
        {
            get
            {
                return CurrentEntity.CollisionMesh;
            }
        }

        public string HoverText { get { return CurrentEntity.HoverText; } }

        public string UnbuiltHoverText {  get { return CurrentEntity.UnbuiltHoverText; } }

        public ImmobileEntity CreateEntity(PointD2D location)
        {
            var tmp = CurrentEntity;
            tmp.Position.X = location.X;
            tmp.Position.Y = location.Y;

            CurrentEntity = EntityCreator();
            return tmp;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            CurrentEntity.Render(context, screenTopLeft, overlay);
        }

        public void TryRotate(int dir)
        {
            var directional = CurrentEntity as Directional;

            if (directional != null)
            {
                bool clockwise = dir > 0;
                var newDir = Direction.Down;
                switch(directional.Direction)
                {
                    case Direction.Down:
                        if (clockwise)
                            newDir = Direction.Left;
                        else
                            newDir = Direction.Right;
                        break;
                    case Direction.Up:
                        if (clockwise)
                            newDir = Direction.Right;
                        else
                            newDir = Direction.Left;
                        break;
                    case Direction.Left:
                        if (clockwise)
                            newDir = Direction.Up;
                        else
                            newDir = Direction.Down;
                        break;
                    case Direction.Right:
                        if (clockwise)
                            newDir = Direction.Down;
                        else
                            newDir = Direction.Up;
                        break;
                    default:
                        throw new InvalidProgramException("can't get here");
                }
                directional.Direction = newDir;
            }
        }

        public virtual bool TilesAreValid(SharedGameState gameState, PointD2D placeLocation)
        {
            var tiles = new List<PointI2D>();
            CurrentEntity.CollisionMesh.TilesIntersectedAt(placeLocation, tiles);

            foreach(var tilePos in tiles)
            {
                var tile = gameState.World.TileAt(tilePos.X, tilePos.Y);
                if (!tile.Ground)
                    return false;
            }

            return true;
        }
    }
}
