using System;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.WorldObject.Entities;

namespace BaseBuilder.Engine.World
{
    public class EntityEventArgs : EventArgs
    {
        public Entity Entity { get; }

        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }
    }
}