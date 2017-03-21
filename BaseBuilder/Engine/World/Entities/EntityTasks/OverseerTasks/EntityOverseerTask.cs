using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.OverseerTasks
{
    /// <summary>
    /// Base class for other files in this namespace
    /// </summary>
    public abstract class EntityOverseerTask : IEntityTask
    {
        public abstract string PrettyDescription { get; }

        public double Progress
        {
            get
            {
                return ((double)(OriginalTimeToCompleteMS - TimeRemainingMS)) / OriginalTimeToCompleteMS;
            }
        }

        public string TaskDescription
        {
            get
            {
                return "Summoning";
            }
        }

        public abstract string TaskName { get; }

        public string TaskStatus
        {
            get
            {
                return "Summoning";
            }
        }

        protected EntityTaskStatus? CachedResult;
        protected bool AteResources;
        protected bool StartedAnimation;
        protected int OverseerID;
        protected int OriginalTimeToCompleteMS;
        protected int TimeRemainingMS;

        public EntityOverseerTask(int overseerId, int timeToComplete)
        {
            OverseerID = overseerId;
            OriginalTimeToCompleteMS = timeToComplete;
            TimeRemainingMS = OriginalTimeToCompleteMS;
        }

        public EntityOverseerTask(NetIncomingMessage message)
        {
            if (message.ReadBoolean())
                CachedResult = (EntityTaskStatus)message.ReadInt32();
            AteResources = message.ReadBoolean();
            StartedAnimation = message.ReadBoolean();
            OverseerID = message.ReadInt32();
            OriginalTimeToCompleteMS = message.ReadInt32();
            TimeRemainingMS = message.ReadInt32();
        }
        
        public void Write(NetOutgoingMessage message)
        {
            if(CachedResult.HasValue)
            {
                message.Write(true);
                message.Write((int)CachedResult.Value);
            }else
            {
                message.Write(false);

            }
            message.Write(AteResources);
            message.Write(StartedAnimation);
            message.Write(OverseerID);
            message.Write(OriginalTimeToCompleteMS);
            message.Write(TimeRemainingMS);
        }

        public virtual bool IsValid()
        {
            return true;
        }

        public void Cancel(SharedGameState gameState)
        {
            if(StartedAnimation)
            {
                var mage = (OverseerMage)gameState.World.GetEntityByID(OverseerID);
                mage.AnimationRenderer.EndAnimation();
                StartedAnimation = false;
            }
            
            if (AteResources)
            {
                var mage = (OverseerMage)gameState.World.GetEntityByID(OverseerID);
                ReturnResources(gameState, mage);
                AteResources = false;
            }
        }

        public void Reset(SharedGameState gameState)
        {
            if (StartedAnimation)
            {
                var mage = (OverseerMage)gameState.World.GetEntityByID(OverseerID);
                mage.AnimationRenderer.EndAnimation();
                StartedAnimation = false;
            }

            TimeRemainingMS = OriginalTimeToCompleteMS;
            CachedResult = null;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (CachedResult.HasValue)
                return CachedResult.Value;

            TimeRemainingMS -= timeMS;
            
            if(!AteResources)
            {
                var mage = (OverseerMage)gameState.World.GetEntityByID(OverseerID);

                if(!EatResources(gameState, mage))
                {
                    CachedResult = EntityTaskStatus.Failure;
                    return EntityTaskStatus.Failure;
                }

                AteResources = true;
            }

            if(TimeRemainingMS <= 0)
            {
                var mage = (OverseerMage) gameState.World.GetEntityByID(OverseerID);

                if (mage == null)
                    return EntityTaskStatus.Failure;

                var res = ImplTask(gameState, mage);
                
                if(StartedAnimation)
                {
                    mage.AnimationRenderer.EndAnimation();
                    StartedAnimation = false;
                }

                if(!res)
                {
                    ReturnResources(gameState, mage);
                }

                AteResources = false; // resources are now consumed so make sure we dont accidentally return them

                CachedResult = res ? EntityTaskStatus.Success : EntityTaskStatus.Failure;
                return CachedResult.Value;
            }

            return EntityTaskStatus.Running;
        }

        /// <summary>
        /// Returns true if found and ate enough resources to cast this spell, or returns
        /// false if not enough resources were found and no resources were consumed
        /// </summary>
        /// <param name="gameState">Gamestate</param>
        /// <param name="mage">Mage</param>
        /// <returns>Success or failure</returns>
        protected abstract bool EatResources(SharedGameState gameState, OverseerMage mage);

        /// <summary>
        /// Returns resources that were consumed via EatResources (which returned true) back
        /// to the mage. Called after ImplTask returns failure.
        /// </summary>
        /// <param name="gameState">Game state</param>
        /// <param name="mage">Mage</param>
        protected abstract void ReturnResources(SharedGameState gameState, OverseerMage mage);

        /// <summary>
        /// Called after resources were consumed and enough time passed that the spell is ready
        /// to be cast. Returns if the spell was able to be cast successfully.
        /// </summary>
        /// <param name="gameState">The game state</param>
        /// <param name="mage">The mage</param>
        /// <returns>Success for true, false for failure</returns>
        protected abstract bool ImplTask(SharedGameState gameState, OverseerMage mage);

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            if(!StartedAnimation)
            {
                var mage = (OverseerMage)sharedGameState.World.GetEntityByID(OverseerID);
                mage.AnimationRenderer.StartAnimation(Utilities.Animations.AnimationType.Casting, mage.AnimationRenderer.CurrentAnimation.Direction);
                StartedAnimation = true;
            }
        }
    }
}
