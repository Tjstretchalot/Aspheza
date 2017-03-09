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
                    _TaskDescription = $"Run {Child.TaskName} and Failure/Success => Failure";
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
                if (Child == null)
                    return "Fails the child";
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

        /// <summary>
        /// Create a new failer task with the task to repeat and a specific name
        /// </summary>
        /// <param name="task"></param>
        /// <param name="specificName"></param>
        public EntityFailerTask(IEntityTask task, string specificName)
        {
            Child = task;
            SpecificName = specificName;
        }

        public EntityFailerTask(SharedGameState gameState, NetIncomingMessage message)
        {
            bool child = message.ReadBoolean();
            if (child)
            {
                var taskID = message.ReadInt16();
                Child = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            }
            SpecificName = message.ReadString();

            TaskRunSinceLastReset = message.ReadBoolean();
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
        }

        public void Reset(SharedGameState gameState)
        {
            if (TaskRunSinceLastReset)
            {
                Child?.Reset(gameState);
                TaskRunSinceLastReset = false;
            }
        }

        public void Cancel(SharedGameState gameState)
        {
            Child?.Cancel(gameState);
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (Child == null)
                return EntityTaskStatus.Failure;

            var result = Child.SimulateTimePassing(gameState, timeMS);
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
            Child?.Update(content, sharedGameState, localGameState);
        }
    }
}
