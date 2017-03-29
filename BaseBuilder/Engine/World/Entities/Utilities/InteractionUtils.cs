using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    /// <summary>
    /// Functions useful when two things are interacting with each other
    /// </summary>
    public class InteractionUtils
    {
        /// <summary>
        /// Determines if thing1 can interact with thing2 right now at their
        /// current positions
        /// </summary>
        /// <param name="thing1">Thing one</param>
        /// <param name="thing2">Thing two</param>
        /// <returns>If they are close enough to interact</returns>
        public static bool CanInteract(MobileEntity thing1, Thing thing2)
        {
            if(thing2.PreferredAdjacentPoints != null)
            {
                PointI2D thing1LocationAsOffset = (PointI2D)(thing1.Position - thing2.Position);

                return thing2.PreferredAdjacentPoints.Find((tup) => tup.Item1 == thing1LocationAsOffset) != null;
            }

            return thing1.CollisionMesh.Intersects(thing2.CollisionMesh, thing1.Position, thing2.Position) || thing1.CollisionMesh.MinDistanceShorterThan(thing2.CollisionMesh, 0.8, thing1.Position, thing2.Position);
        }
    }
}
