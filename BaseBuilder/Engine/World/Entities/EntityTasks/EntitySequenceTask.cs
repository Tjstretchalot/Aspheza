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
                    foreach(var task in Children)
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
                if (Counter == Children.Count)
                    return "Finished";

                return $"Counter={Counter} (TaskName = {Children[Counter].TaskName})";
            }
        }

        public string PrettyDescription
        {
            get
            {
                if (Children.Count == 0)
                    return "In a bad state =(";

                return Children[Counter].PrettyDescription;
            }
        }

        public double Progress
        {
            get
            {
                if (Children.Count == 0)
                    return 0;

                return Children[Counter].Progress;
            }
        }

        protected string SpecificName;
        protected int Counter;
        protected bool CounterRunAtleastOnce;

        public List<IEntityTask> Children;

        /// <summary>
        /// Initialize a new sequence task with the specified tasks and the specified debug name.
        /// </summary>
        /// <param name="tasks">The tasks</param>
        /// <param name="specificName">The debug name. This task will have the TaskName $"Sequence {SpecificName}"</param>
        public EntitySequenceTask(List<IEntityTask> tasks, string specificName)
        {
            Children = tasks;
            SpecificName = specificName;
            Counter = 0;
        }

        public EntitySequenceTask(SharedGameState gameState, NetIncomingMessage message)
        {
            SpecificName = message.ReadString();
            Counter = message.ReadInt32();
            CounterRunAtleastOnce = message.ReadBoolean();

            var numTasks = message.ReadInt16();
            Children = new List<IEntityTask>(numTasks);

            for(int i = 0; i < Children.Count; i++)
            {
                var taskID = message.ReadInt16();

                Children.Add(TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message));
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(SpecificName);
            message.Write(Counter);
            message.Write(CounterRunAtleastOnce);

            message.Write((short)Children.Count);
            foreach(var task in Children)
            {
                message.Write(TaskIdentifier.GetIDOfTask(task.GetType()));
                task.Write(message);
            }
        }
        
        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (Children.Count == 0)
                return EntityTaskStatus.Success;

            var currTask = Children[Counter];

            var result = currTask.SimulateTimePassing(gameState, timeMS);

            switch(result)
            {
                case EntityTaskStatus.Running:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Running;
                case EntityTaskStatus.Success:
                    Counter++;
                    CounterRunAtleastOnce = false;
                    if (Counter == Children.Count)
                        return EntityTaskStatus.Success;
                    return EntityTaskStatus.Running;
                case EntityTaskStatus.Failure:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Failure;
            }

            throw new InvalidProgramException("Can't get here");
        }

        public bool IsValid()
        {
            if (Children.Count == 0)
                return false;

            foreach(var child in Children)
            {
                if (!child.IsValid())
                    return false;
            }

            return true;
        }

        public void Reset(SharedGameState gameState)
        { 
            if(Children.Count == 0)
            {
                Counter = 0;
                return;
            }

            for(int i = 0; i < Counter; i++)
            {
                Children[i].Reset(gameState);
            }

            if(CounterRunAtleastOnce && Counter < Children.Count)
            {
                Children[Counter].Reset(gameState);
            }

            Counter = 0;
        }

        public void Cancel(SharedGameState gameState)
        {
            foreach(var task in Children)
            {
                task.Cancel(gameState);
            }
        }


        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
            foreach (var task in Children)
            {
                task.Update(content, sharedGameState, localGameState);
            }
        }
    }
}
