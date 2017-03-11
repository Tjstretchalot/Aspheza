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
                    _TaskDescription = $"Repeat {Child.TaskName}";
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

        public string PrettyDescription
        {
            get
            {
                if (Child == null)
                    return "In a bad state =(";

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

        public IEntityTask Child;
        protected string SpecificName;
        protected bool TaskRunSinceLastReset;
        public int Times;
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
            Child = task;
            SpecificName = specificName;
            TaskRunSinceLastReset = false;
            Times = times;
            TimesRemaining = Times;
        }

        public EntityRepeaterTask(SharedGameState gameState, NetIncomingMessage message)
        {
            var haveChild = message.ReadBoolean();
            if (haveChild)
            {
                var taskID = message.ReadInt16();
                Child = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            }
            SpecificName = message.ReadString();
            TaskRunSinceLastReset = message.ReadBoolean();
            Times = message.ReadInt32();
            TimesRemaining = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            if (Child != null)
            {
                message.Write(true);
                var taskID = TaskIdentifier.GetIDOfTask(Child.GetType());
                message.Write(taskID);
                Child.Write(message);
            }else
            {
                message.Write(false);
            }

            message.Write(SpecificName);
            message.Write(TaskRunSinceLastReset);
            message.Write(Times);
            message.Write(TimesRemaining);
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Child?.Reset(gameState);
                TaskRunSinceLastReset = false;
            }

            TimesRemaining = Times;
        }

        public void Cancel(SharedGameState gameState)
        {
             Child?.Cancel(gameState);
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            var result = Child.SimulateTimePassing(gameState, timeMS);
            TaskRunSinceLastReset = true;

            switch (result)
            {
                case EntityTaskStatus.Success:
                    Child.Reset(gameState);
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

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            Child?.Update(content, sharedGameState, localGameState);
        }

        public bool IsValid()
        {
            if (Child == null)
                return false;

            return Child.IsValid();
        }
    }
}
