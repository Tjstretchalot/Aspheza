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
    class EntityRepeatUntilFailTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if (_TaskDescription == null)
                {
                    _TaskDescription = $"Repeat until failure {Task.TaskName}";
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
                return Task.PrettyDescription;
            }
        }

        public double Progress
        {
            get
            {
                return Task.Progress;
            }
        }

        protected IEntityTask Task;
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
            Task = task;
            SpecificName = specificName;
        }

        public EntityRepeatUntilFailTask(SharedGameState gameState, NetIncomingMessage message)
        {
            var taskID = message.ReadInt16();

            Task = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            SpecificName = message.ReadString();
            TaskRunSinceLastReset = message.ReadBoolean();
            Failed = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(TaskIdentifier.GetIDOfTask(Task.GetType()));

            Task.Write(message);
            message.Write(SpecificName);
            message.Write(TaskRunSinceLastReset);
            message.Write(Failed);
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Task.Reset(gameState);
                TaskRunSinceLastReset = false;
            }

            Failed = false;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (Failed)
                throw new InvalidProgramException("I was supposed to be reset!");

            var result = Task.SimulateTimePassing(gameState, timeMS);
            TaskRunSinceLastReset = true;

            switch (result)
            {
                case EntityTaskStatus.Success:
                    Task.Reset(gameState);
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
            Task.Update(content, sharedGameState, localGameState);
        }
    }
}
