using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Inverts the childs result - if the child returns success, the inverter
    /// returns failure. If the child returns failure, the child returns success.
    /// If the child returns running, the inverter returns running.
    /// </summary>
    public class EntityInverterTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                if(Child == null)
                    return "Inverting";
                return Child.PrettyDescription;
            }
        }

        public double Progress
        {
            get
            {
                if (Child == null)
                    return 0;
                return Child.Progress;
            }
        }

        public string TaskDescription
        {
            get
            {
                if (Child == null)
                    return "Invert";
                return $"Invert {Child.TaskName}";
            }
        }

        public string TaskName
        {
            get
            {
                if (Child == null)
                    return "Invert";
                return $"Invert {Child.TaskName}";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Inverting";
            }
        }

        public IEntityTask Child;
        protected bool ChildRunSinceLastReset;
        protected EntityTaskStatus? CachedResult;

        public EntityInverterTask(IEntityTask child)
        {
            Child = child;
            ChildRunSinceLastReset = false;
            CachedResult = null;
        }

        public EntityInverterTask(SharedGameState gameState, NetIncomingMessage message)
        {
            if(message.ReadBoolean())
            {
                var typeID = message.ReadInt16();
                var child = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(typeID), gameState, message);
            }

            ChildRunSinceLastReset = message.ReadBoolean();

            if(message.ReadBoolean())
                CachedResult = (EntityTaskStatus)message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            if(Child == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(TaskIdentifier.GetIDOfTask(Child.GetType()));
                Child.Write(message);
            }

            message.Write(ChildRunSinceLastReset);
            if(CachedResult.HasValue)
            {
                message.Write(true);
                message.Write((int)CachedResult.Value);
            }else
            {
                message.Write(false);
            }
        }

        public bool IsValid()
        {
            if (Child == null)
                return false;

            return Child.IsValid();
        }

        public void Reset(SharedGameState gameState)
        {
            CachedResult = null;

            if(ChildRunSinceLastReset)
            {
                Child.Reset(gameState);
                ChildRunSinceLastReset = false;
            }
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            if (CachedResult.HasValue)
                return CachedResult.Value;

            ChildRunSinceLastReset = true;
            var result = Child.SimulateTimePassing(gameState, timeMS);

            switch(result)
            {
                case EntityTaskStatus.Failure:
                    CachedResult = EntityTaskStatus.Success;
                    return EntityTaskStatus.Success;
                case EntityTaskStatus.Success:
                    CachedResult = EntityTaskStatus.Failure;
                    return EntityTaskStatus.Failure;
                case EntityTaskStatus.Running:
                    return EntityTaskStatus.Running;
                default:
                    throw new InvalidProgramException();
            }
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            Child?.Update(content, sharedGameState, localGameState);
        }

        public void Cancel(SharedGameState gameState)
        {
            Child?.Cancel(gameState);
        }
    }
}
