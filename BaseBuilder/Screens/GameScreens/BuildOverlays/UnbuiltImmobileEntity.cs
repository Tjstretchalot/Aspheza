using BaseBuilder.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// Describes an unbuilt immobile entity - an unbuilt immobile entity
    /// is one that was selected in the build menu but not placed, not one
    /// that is currently building.
    /// </summary>
    public interface UnbuiltImmobileEntity : Renderable
    {
        /// <summary>
        /// The collision mesh used for this unbuilt entity
        /// </summary>
        PolygonD2D CollisionMesh { get; }

        /// <summary>
        /// Creates the entity at the specified location
        /// </summary>
        /// <param name="location">The location to spawn the entity</param>
        /// <returns>The entity</returns>
        ImmobileEntity CreateEntity(PointD2D location);

        /// <summary>
        /// Tries to rotate. If direction > 0, tries to rotate clockwise. Otherwise, tries
        /// to rotate counterclockwise
        /// </summary>
        /// <param name="direction">Direction to rotate in</param>
        void TryRotate(int direction);
    }
}
