using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class Archer : MobileEntity
    {
        public Archer(PointD2D position, RectangleD2D collisionMesh, int id) : base(position, collisionMesh, id, "Archer")
        {
        }

    }
}
