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
    /// This is a task that simply loops through other tasks in sequence. If the task returns success,
    /// the sequence will return running if there are other tasks (and call the next task in sequence
    /// next time). If the last task returns success, the sequence returns success.
    /// 
    /// If a task returns failure, immediately returns failure.
    /// </summary>
    public class EntitySequenceTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if(_TaskDescription == null)
                {
                    var builder = new StringBuilder("Run ");

                    bool first = true;
                    foreach(var task in Tasks)
                    {
                        if(!first)
                        {
                            builder.Append(", ");
                        }else
                        {
                            first = false;
                        }

                        builder.Append(task.TaskName);
                    }

                    builder.Append(" in order.");
                    _TaskDescription = builder.ToString();
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
                    _TaskName = $"Sequence {SpecificName}";
                }

                return _TaskName;
            }
        }

        public string TaskStatus
        {
            get
            {
                if (Counter == Tasks.Count)
                    return "Finished";

                return $"Counter={Counter} (TaskName = {Tasks[Counter].TaskName})";
            }
        }

        protected string SpecificName;
        protected int Counter;
        protected bool CounterRunAtleastOnce;

        protected List<IEntityTask> Tasks;

        /// <summary>
        /// Initialize a new sequence task with the specified tasks and the specified debug name.
        /// </summary>
        /// <param name="tasks">The tasks</param>
        /// <param name="specificName">The debug name. This task will have the TaskName $"Sequence {SpecificName}"</param>
        public EntitySequenceTask(List<IEntityTask> tasks, string specificName)
        {
            Tasks = tasks;
            SpecificName = specificName;
            Counter = 0;
        }

        public EntitySequenceTask(NetIncomingMessage message)
        {
            SpecificName = message.ReadString();
            Counter = message.ReadInt32();
            CounterRunAtleastOnce = message.ReadBoolean();

            var numTasks = message.ReadInt16();
            Tasks = new List<IEntityTask>(numTasks);

            for(int i = 0; i < Tasks.Count; i++)
            {
                var taskID = message.ReadInt16();

                Tasks.Add(TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), message));
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(SpecificName);
            message.Write(Counter);
            message.Write(CounterRunAtleastOnce);

            message.Write((short)Tasks.Count);
            foreach(var task in Tasks)
            {
                message.Write(TaskIdentifier.GetIDOfTask(task.GetType()));
                task.Write(message);
            }
        }
        
        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            var currTask = Tasks[Counter];

            var result = currTask.SimulateTimePassing(gameState, timeMS);

            switch(result)
            {
                case EntityTaskStatus.Running:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Running;
                case EntityTaskStatus.Success:
                    Counter++;
                    CounterRunAtleastOnce = false;
                    if (Counter == Tasks.Count)
                        return EntityTaskStatus.Success;
                    return EntityTaskStatus.Running;
                case EntityTaskStatus.Failure:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Failure;
            }

            throw new InvalidProgramException("Can't get here");
        }

        public void Reset(SharedGameState gameState)
        {
            for(int i = 0; i < Counter; i++)
            {
                Tasks[i].Reset(gameState);
            }

            if(CounterRunAtleastOnce && Counter < Tasks.Count)
            {
                Tasks[Counter].Reset(gameState);
            }

            Counter = 0;
        }
    }
}
