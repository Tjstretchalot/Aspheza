using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class EntityRepeatUntilFailTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if (_TaskDescription == null)
                {
                    _TaskDescription = $"Repeat until failure {Child.TaskName}";
                }

                return _TaskDescription;
            }
        }

        protected string _TaskName;
        public string TaskName
        {
            get
            {
                if (_TaskName == null)
                {
                    _TaskName = $"RepeatUntilFail ({SpecificName})";
                }

                return _TaskName;
            }
        }

        public string TaskStatus
        {
            get
            {
                if (Failed)
                    return "Failed";

                return "Repeating";
            }
        }

        public string PrettyDescription
        {
            get
            {
                return Child.PrettyDescription;
            }
        }

        public double Progress
        {
            get
            {
                return Child.Progress;
            }
        }

        protected IEntityTask Child;
        protected string SpecificName;
        protected bool TaskRunSinceLastReset;
        protected bool Failed;

        /// <summary>
        /// Create a new repeat until fail task with the task to repeat and a specific name.
        /// </summary>
        /// <param name="task">The task to repeat.</param>
        /// <param name="specificName">The name of this task is RepeatUntilFail {SpecificName}</param>
        public EntityRepeatUntilFailTask(IEntityTask task, string specificName)
        {
            Child = task;
            SpecificName = specificName;
        }

        public EntityRepeatUntilFailTask()
        {
            Child = null;
        }

        public EntityRepeatUntilFailTask(SharedGameState gameState, NetIncomingMessage message)
        {
            if (message.ReadBoolean())
            {
                var taskID = message.ReadInt16();

                Child = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            }

            SpecificName = message.ReadString();
            TaskRunSinceLastReset = message.ReadBoolean();
            Failed = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            if (Child != null)
            {
                message.Write(true);
                message.Write(TaskIdentifier.GetIDOfTask(Child.GetType()));
                Child.Write(message);
            }else
            {
                message.Write(false);
            }

            message.Write(SpecificName);
            message.Write(TaskRunSinceLastReset);
            message.Write(Failed);
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Child?.Reset(gameState);
                TaskRunSinceLastReset = false;
            }

            Failed = false;
        }

        public void Cancel(SharedGameState gameState)
        {
             Child?.Cancel(gameState);
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            if (Failed)
                throw new InvalidProgramException("I was supposed to be reset!");

            var result = Child.SimulateTimePassing(gameState, timeMS);
            TaskRunSinceLastReset = true;

            switch (result)
            {
                case EntityTaskStatus.Success:
                    Child.Reset(gameState);
                    TaskRunSinceLastReset = false;
                    break;
                case EntityTaskStatus.Failure:
                    Failed = true;
                    return EntityTaskStatus.Failure;
                case EntityTaskStatus.Running:
                    break;
            }

            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            Child.Update(content, sharedGameState, localGameState);
        }

        public bool IsValid()
        {
            return Child != null && Child.IsValid();
        }
    }
}
