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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

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

        public string PrettyDescription
        {
            get
            {
                return "Moving";
            }
        }

        public double Progress
        {
            get
            {
                if (Start == null)
                    return 0;
                var distanceRemaining = (Destination - Entity.Position).AsVectorD2D().Magnitude;
                if(distanceRemaining > InitialDistance)
                {
                    return 0;
                }

                return 1 - (distanceRemaining / InitialDistance);
            }
        }

        protected int EntityID;
        protected MobileEntity Entity;
        protected PointD2D Start;
        protected PointI2D Destination;
        protected double InitialDistance;

        protected UnitPath Path;
        protected bool Finished;
        protected bool FailedToFindPath;

        protected Random random;
        protected bool PlaySFX;
        protected int NextSFXCounter;

        /// <summary>
        /// Initializes a move task that will calculate a path on the next SimulateTimePassing
        /// </summary>
        /// <param name="entity">The entity which will be moving</param>
        /// <param name="destination">Where the entity is going</param>
        public EntityMoveTask(MobileEntity entity, PointI2D destination)
        {
            Start = new PointD2D(entity.Position.X, entity.Position.Y);
            Entity = entity;
            EntityID = entity.ID;
            Destination = destination;

            InitialDistance = (Destination - Start).AsVectorD2D().Magnitude;

            Path = null;
            FailedToFindPath = false;
            Finished = false;

            random = new Random();
        }

        /// <summary>
        /// Initializes an entity move task that was created by a call to Write on an outgoing message. Does
        /// not load the entity from the gamestate until the next call to SimulateTimePassing (meaning this can
        /// be used in the constructor of an Entity who is trying to load its own tasks)
        /// </summary>
        /// <param name="gameState">The game state</param>
        /// <param name="message">The message</param>
        public EntityMoveTask(SharedGameState gameState, NetIncomingMessage message)
        {
            EntityID = message.ReadInt32();
            if (message.ReadBoolean())
                Start = new PointD2D(message);
            Destination = new PointI2D(message);
            InitialDistance = message.ReadDouble();

            if(message.ReadBoolean())
                Path = new UnitPath(message);
            Finished = message.ReadBoolean();
            FailedToFindPath = message.ReadBoolean();

            random = new Random();
        }

        /// <summary>
        /// <para>Writes this move task to the specified message, except the entity is saved solely via its
        /// EntityID. This means this function is safe to call from the entity whose task this is (a 
        /// common case).</para>
        /// </summary>
        /// <param name="message">The message to write to.</param>
        public void Write(NetOutgoingMessage message)
        {
            message.Write(EntityID);
            if (Start == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                Start.Write(message);
            }

            Destination.Write(message);
            message.Write(InitialDistance);

            if (Path == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                Path.Write(message);
            }

            message.Write(Finished);
            message.Write(FailedToFindPath);
        }


        public void Reset(SharedGameState gameState)
        {
            Path = null;
            FailedToFindPath = false;
            Finished = false;
            Start = null;
        }

        /// <summary>
        /// Attempts to move the entity linearly between points on a path generated by gameState.Pathfinder
        /// on the first call to SimulateTimePassing.
        /// 
        /// If a path cannot be found, returns EntityTaskStatus.Failure.
        /// If the entity is still moving, returns EntityTaskStatus.Running
        /// If the entity has reached its destination this frame, returns EntityTaskStatus.Success
        /// </summary>
        /// <param name="gameState">The game state</param>
        /// <param name="timeMS">The time in milliseconds</param>
        /// <returns>Success if entity at destination, Failure if no path is found, Running if the entity is still moving.</returns>
        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if(Entity == null)
            {
                Entity = gameState.World.MobileEntities.Find((me) => me.ID == EntityID);
            }

            if(Start == null)
            {
                Start = new PointD2D(Entity.Position.X, Entity.Position.Y);
                InitialDistance = (Destination - Start).AsVectorD2D().Magnitude;
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
                    OnFinished(gameState);
                    return EntityTaskStatus.Success;
                }

                curr = Path.GetCurrent();
            }

            ImplMove(gameState, curr, timeMS);
            
            bool move = !gameState.World.UpdateTileCollisions(Entity);
            if(move)
            {
                NextSFXCounter--;

                if(NextSFXCounter <= 0)
                {
                    PlaySFX = true;
                    NextSFXCounter = random.Next(3) + 9;
                }
            }
            if (Finished)
            {
                OnFinished(gameState);
                return EntityTaskStatus.Success;
            }
            return EntityTaskStatus.Running;
        }

        void OnFinished(SharedGameState gameState)
        {
            Path = null;
            // fix rounding
            Entity.Position.X = Destination.X;
            Entity.Position.Y = Destination.Y;
            gameState.World.UpdateTileCollisions(Entity);
            Entity.OnStop(gameState);
        }

        void OnMove(SharedGameState gameState, int timeMS, double dx, double dy)
        {
            Entity.OnMove(gameState, timeMS, dx, dy);
        }

        /// <summary>
        /// This function handles the meat of the movement. This function is built recursively;
        /// if the entity is nearly at the next grid location it will not require the full moveMS
        /// to start the next path. In this case, ImplMove calculates the time that is spent to move
        /// to the next grid location, calls Path.Next(), and calls itself with the remaining move time
        /// and the new part of the path.
        /// 
        /// Finished may be set to true if the destination is reached.
        /// 
        /// This will crash if the entity is at curr (or within E-06 units of it).
        /// </summary>
        /// <param name="gameState">The game state</param>
        /// <param name="curr">Where the entity is currently</param>
        /// <param name="moveMS">The maximum amount of milliseconds of movement the entity has left this frame</param>
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
                OnMove(gameState, moveMS, moveVec.DeltaX, moveVec.DeltaY);

                Finished = !Path.Next();
                if (Finished)
                    return;

                curr = Path.GetCurrent();

                int msRemaining = moveMS - msToMoveMoveVec;

                if (msRemaining > 0)
                    ImplMove(gameState, curr, msRemaining);
            }else
            {
                var posOffset = moveVec.UnitVector.Scale(unitsMaxThisTick);
                Entity.Position.X += posOffset.DeltaX;
                Entity.Position.Y += posOffset.DeltaY;
                OnMove(gameState, moveMS, moveVec.DeltaX, moveVec.DeltaY);
            }
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            if (PlaySFX)
            {
                var sfxNm = $"Sounds/footstep05";
                var sfx = content.Load<SoundEffect>(sfxNm);
                var inst = sfx.CreateInstance();
                inst.Volume = 0.5f;
                inst.Play();
                PlaySFX = false;
            }
        }
    }
}
