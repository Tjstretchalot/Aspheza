using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// A repeater task simply repeats another task. If the underlying task fails, immediately
    /// returns failed.
    /// </summary>
    public class EntityRepeaterTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if(_TaskDescription == null)
                {
                    _TaskDescription = $"Repeat {Task.TaskName}";
                }

                return _TaskDescription;
            }
        }

        protected string _TaskName;
        public string TaskName
        {
            get
            {
                if(_TaskName == null)
                {
                    _TaskName = $"Repeater ({SpecificName})";
                }

                return _TaskName;
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Repeating";
            }
        }

        protected IEntityTask Task;
        protected string SpecificName;
        protected bool TaskRunSinceLastReset;
        protected int Times;
        protected int TimesRemaining;

        /// <summary>
        /// Create a new repeater task with the task to repeat and a specific name. Optionally specify
        /// the number of times to get "Success" before returning success and requiring a reset.
        /// </summary>
        /// <param name="task">The task to repeat.</param>
        /// <param name="specificName">The name of this task is Repeater {SpecificName}</param>
        /// <param name="times">If not 0, the number of times the task is run before returning success</param>
        public EntityRepeaterTask(IEntityTask task, string specificName, int times=0)
        {
            Task = task;
            SpecificName = specificName;
        }

        public EntityRepeaterTask(NetIncomingMessage message)
        {
            var taskID = message.ReadInt16();
            Task = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), message);
            SpecificName = message.ReadString();
            TaskRunSinceLastReset = message.ReadBoolean();
            Times = message.ReadInt32();
            TimesRemaining = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            var taskID = TaskIdentifier.GetIDOfTask(Task.GetType());

            message.Write(taskID);
            Task.Write(message);
            message.Write(SpecificName);
            message.Write(TaskRunSinceLastReset);
            message.Write(Times);
            message.Write(TimesRemaining);
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Task.Reset(gameState);
                TaskRunSinceLastReset = false;
            }

            TimesRemaining = Times;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            var result = Task.SimulateTimePassing(gameState, timeMS);
            TaskRunSinceLastReset = true;

            switch (result)
            {
                case EntityTaskStatus.Success:
                    Task.Reset(gameState);
                    TaskRunSinceLastReset = false;
                    if (Times != 0)
                    {
                        TimesRemaining--;
                        if (TimesRemaining <= 0)
                        {
                            return EntityTaskStatus.Success;
                        }
                    }
                    break;
                case EntityTaskStatus.Failure:
                    return EntityTaskStatus.Failure;
                case EntityTaskStatus.Running:
                    break;
            }

            return EntityTaskStatus.Running;
        }
    }
}
