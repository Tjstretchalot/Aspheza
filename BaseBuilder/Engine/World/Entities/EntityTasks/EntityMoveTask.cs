using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.Logic.Pathfinders;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.Math2D.Double;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Describes a task for a mobile entity to move to a specified point. Returns success
    /// if it reaches that point, returns failure if a path cannot be found / the entity is
    /// blocked.
    /// </summary>
    public class EntityMoveTask : IEntityTask
    {
        
        public string TaskDescription
        {
            get
            {
                return $"Moving {Entity.GetType().Name} (id={Entity.ID}) to {Destination}";
            }
        }

        public string TaskName
        {
            get
            {
                return "Move";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Moving";
            }
        }

        protected int EntityID;
        protected MobileEntity Entity;
        protected PointI2D Destination;

        protected UnitPath Path;
        protected bool Finished;
        protected bool FailedToFindPath;

        public EntityMoveTask(MobileEntity entity, PointI2D destination)
        {
            Entity = entity;
            EntityID = entity.ID;
            Destination = destination;

            Path = null;
            FailedToFindPath = false;
            Finished = false;
        }

        public EntityMoveTask(SharedGameState gameState, NetIncomingMessage message)
        {
            EntityID = message.ReadInt32();
            Destination = new PointI2D(message);

            Path = new UnitPath(message);
            Finished = message.ReadBoolean();
            FailedToFindPath = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(EntityID);
            Destination.Write(message);
            Path.Write(message);
            message.Write(Finished);
            message.Write(FailedToFindPath);
        }


        public void Reset(SharedGameState gameState)
        {
            Path = null;
            FailedToFindPath = false;
            Finished = false;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if(Entity == null)
            {
                Entity = gameState.World.MobileEntities.Find((me) => me.ID == EntityID);
            }

            if (FailedToFindPath || Finished)
                throw new InvalidProgramException("I should have been reset!");

            if(Path == null)
            {
                Path = gameState.Pathfinder.CalculatePath(gameState.World, Entity, new PointI2D((int)Entity.Position.X, (int)Entity.Position.Y), Destination);
                if(Path == null)
                {
                    FailedToFindPath = true;
                    return EntityTaskStatus.Failure;
                }
            }

            var curr = Path.GetCurrent();
            if(EpsilonEqual(Entity.Position.X, curr.X) && EpsilonEqual(Entity.Position.Y, curr.Y))
            {
                Finished = !Path.Next();

                if (Finished)
                {
                    Path = null;
                    return EntityTaskStatus.Success;
                }

                curr = Path.GetCurrent();
            }

            ImplMove(gameState, curr, timeMS);
            gameState.World.UpdateTileCollisions(Entity);
            if (Finished)
            {
                Path = null;
                return EntityTaskStatus.Success;
            }
            return EntityTaskStatus.Running;
        }

        private void ImplMove(SharedGameState gameState, PointI2D curr, int moveMS)
        {
            var moveVec = new VectorD2D(curr.X - Entity.Position.X, curr.Y - Entity.Position.Y);

            var speedUnitsPerMS = Entity.SpeedUnitsPerMS; 

            var unitsMaxThisTick = speedUnitsPerMS * moveMS;
            var unitsSqMaxThisTick = unitsMaxThisTick * unitsMaxThisTick;
            
            if(EpsilonLessThan(moveVec.MagnitudeSquared, unitsSqMaxThisTick))
            {
                int msToMoveMoveVec = (int)Math.Round(moveVec.Magnitude / speedUnitsPerMS);

                Entity.Position.X = curr.X;
                Entity.Position.Y = curr.Y;

                Finished = !Path.Next();
                if (Finished)
                    return;

                int msRemaining = moveMS - msToMoveMoveVec;

                if (msRemaining > 0)
                    ImplMove(gameState, Path.GetCurrent(), msRemaining);
            }else
            {
                var posOffset = moveVec.UnitVector.Scale(unitsMaxThisTick);
                Entity.Position.X += posOffset.DeltaX;
                Entity.Position.Y += posOffset.DeltaY;
            }
        }
    }
}
