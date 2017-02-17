using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class Overseer : MobileEntity
    {
        public Overseer(PointD2D position, RectangleD2D collisionMesh, int id) : base(position, collisionMesh, id, "Overseer")
        {
        }
    }
}
