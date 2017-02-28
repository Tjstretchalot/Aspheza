using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using Microsoft.Xna.Framework;

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

        public PolygonD2D CollisionMesh
        {
            get
            {
                return CurrentEntity.CollisionMesh;
            }
        }

        public string HoverText { get { return null; } }

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
    }
}
