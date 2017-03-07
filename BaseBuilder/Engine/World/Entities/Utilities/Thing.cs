﻿using BaseBuilder.Engine.Math2D.Double;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    /// <summary>
    /// Describes something that is in the world
    /// </summary>
    public interface Thing
    {
        /// <summary>
        /// The unique identifier for this thing
        /// </summary>
        int ID { get; }

        /// <summary>
        /// If this thing has been removed from the world.
        /// </summary>
        bool Destroyed { get; set; }

        /// <summary>
        /// The position
        /// </summary>
        PointD2D Position { get; }

        /// <summary>
        /// The collision mesh
        /// </summary>
        CollisionMeshD2D CollisionMesh { get; }

    }
}