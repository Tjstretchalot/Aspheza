using BaseBuilder.Engine.State;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// A repeater task simply repeats another task
    /// </summary>
    public class EntityFailerTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if (_TaskDescription == null)
                {
                    _TaskDescription = $"Run {Task.TaskName} and Failure/Success => Failure";
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
                    _TaskName = $"Fail ({SpecificName})";
                }

                return _TaskName;
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Failing";
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

        /// <summary>
        /// Create a new failer task with the task to repeat and a specific name
        /// </summary>
        /// <param name="task"></param>
        /// <param name="specificName"></param>
        public EntityFailerTask(IEntityTask task, string specificName)
        {
            Task = task;
            SpecificName = specificName;
        }

        public EntityFailerTask(SharedGameState gameState, NetIncomingMessage message)
        {
            var taskID = message.ReadInt16();
            Task = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            SpecificName = message.ReadString();

            TaskRunSinceLastReset = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(TaskIdentifier.GetIDOfTask(Task.GetType()));
            Task.Write(message);
            message.Write(SpecificName);

            message.Write(TaskRunSinceLastReset);
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Task.Reset(gameState);
                TaskRunSinceLastReset = false;
            }
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            var result = Task.SimulateTimePassing(gameState, timeMS);
            TaskRunSinceLastReset = true;

            switch (result)
            {
                case EntityTaskStatus.Success:
                case EntityTaskStatus.Failure:
                    return EntityTaskStatus.Failure;
                case EntityTaskStatus.Running:
                    return result;
            }

            throw new InvalidProgramException("Can't get here");
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            Task.Update(content, sharedGameState, localGameState);
        }
    }
}
