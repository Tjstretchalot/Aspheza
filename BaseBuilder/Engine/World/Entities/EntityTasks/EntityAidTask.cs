using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// This task allows a mobile entity to aid an Aidable that NeedsAid
    /// </summary>
    public class EntityAidTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                return "Aiding";
            }
        }

        public double Progress
        {
            get
            {
                return 1; 
            }
        }

        public string TaskDescription
        {
            get
            {
                return "Aid";
            }
        }

        public string TaskName
        {
            get
            {
                return "Aid";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Aiding";
            }
        }

        protected EntityTaskStatus? CachedResult;
        protected int AiderID;
        public ITransferTargeter AidingTargeter;

        protected bool SetDirection;
        
        public EntityAidTask(int aiderId, ITransferTargeter aidingTargeter)
        {
            AiderID = aiderId;
            AidingTargeter = aidingTargeter;
        }
        
        public EntityAidTask(NetIncomingMessage message)
        {
            AiderID = message.ReadInt32();

            if (message.ReadBoolean())
            {
                var typeID = message.ReadInt16();
                AidingTargeter = TransferTargeterIdentifier.Init(TransferTargeterIdentifier.GetTypeOfID(typeID), message);
            }

            if (message.ReadBoolean())
                CachedResult = (EntityTaskStatus)message.ReadInt32();

            SetDirection = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(AiderID);
            
            if(AidingTargeter != null)
            {
                message.Write(true);
                message.Write(TransferTargeterIdentifier.GetIDOfType(AidingTargeter.GetType()));
                AidingTargeter.Write(message);
            }else
            {
                message.Write(false);
            }

            if(CachedResult.HasValue)
            {
                message.Write(true);
                message.Write((int)CachedResult.Value);
            }else
            {
                message.Write(false);
            }

            message.Write(SetDirection);
        }

        public void Cancel(SharedGameState gameState)
        {
        }

        public bool IsValid()
        {
            return AiderID >= 0 && AidingTargeter != null && AidingTargeter.IsValid();
        }

        public void Reset(SharedGameState gameState)
        {
            SetDirection = false;
            CachedResult = null;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;
            if (CachedResult.HasValue)
                return CachedResult.Value;

            var aider = (MobileEntity)gameState.World.GetEntityByID(AiderID, true);
            var aiding = AidingTargeter.FindTarget(gameState, aider) as Aidable;

            if(aiding == null)
            {
                CachedResult = EntityTaskStatus.Failure;
                return  EntityTaskStatus.Failure;
            }

            if(!aiding.NeedAid)
            {
                CachedResult = EntityTaskStatus.Success;
                return EntityTaskStatus.Success;
            }

            aiding.Aid(aider, timeMS);
            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }
    }
}
