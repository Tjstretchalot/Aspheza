using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.EntityTasks.EntityConditionals;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// This is meant as an intermediary task which allows an entity to
    /// check for some condition. Typically these are used as all but the
    /// last part of sequences that are inside selectors.
    /// </summary>
    public class EntityConditionTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                return "Introspection";
            }
        }

        public double Progress
        {
            get
            {
                return ((double)TotalTimeMS - TimeRemainingMS) / TotalTimeMS;
            }
        }

        public string TaskDescription
        {
            get
            {
                return "Introspecting";
            }
        }

        public string TaskName
        {
            get
            {
                return "Introspect";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Introspecting";
            }
        }

        protected int SourceID;
        public IEntityCondition Conditional;
        protected EntityTaskStatus? CachedResult;
        protected int TimeRemainingMS;
        public int TotalTimeMS;

        public EntityConditionTask(int sourceID, IEntityCondition condition, int timeToCalculate=1000)
        {
            SourceID = sourceID;
            Conditional = condition;
            TotalTimeMS = timeToCalculate;
            TimeRemainingMS = TotalTimeMS;
        }

        public EntityConditionTask(NetIncomingMessage message)
        {
            SourceID = message.ReadInt32();

            if(message.ReadBoolean())
            {
                var type = EntityConditionIdentifier.GetTypeOfID(message.ReadInt16());
                Conditional = EntityConditionIdentifier.Init(type, message);
            }

            if(message.ReadBoolean())
            {
                CachedResult = (EntityTaskStatus)message.ReadInt32();
            }

            TotalTimeMS = message.ReadInt32();
            TimeRemainingMS = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(SourceID);

            if(Conditional == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(EntityConditionIdentifier.GetIDOfType(Conditional.GetType()));
                Conditional.Write(message);
            }

            if(!CachedResult.HasValue)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write((int)CachedResult.Value);
            }

            message.Write(TotalTimeMS);
            message.Write(TimeRemainingMS);
        }

        public void Cancel(SharedGameState gameState)
        {
            
        }

        public bool IsValid()
        {
            return SourceID >= 0 && Conditional != null;
        }

        public void Reset(SharedGameState gameState)
        {
            TimeRemainingMS = TotalTimeMS;
            CachedResult = null;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            if (CachedResult.HasValue)
                return CachedResult.Value;

            TimeRemainingMS -= timeMS;
            if(TimeRemainingMS <= 0)
            {
                var entity = gameState.World.GetEntityByID(SourceID);
                bool result = Conditional.Decide(gameState, entity);

                if(result)
                {
                    CachedResult = EntityTaskStatus.Success;
                }else
                {
                    CachedResult = EntityTaskStatus.Failure;
                }

                return CachedResult.Value;
            }

            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }
    }
}
