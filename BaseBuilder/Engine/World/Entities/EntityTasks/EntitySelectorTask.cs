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
    /// <summary>
    /// A selector task acts much like a sequence task in that it loops through
    /// child tasks in order. However, unlike a sequence task the selector task
    /// handles success/failure of children differently:
    /// 
    /// If a child returns failure and there are other tasks in the selector, the selector
    /// returns running (and calls the next task in the selector next time).
    /// 
    /// If the last child returns failure, the selector returns failure
    /// 
    /// If a child returns success, the selector immediately returns success
    /// </summary>
    public class EntitySelectorTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if (_TaskDescription == null)
                {
                    var builder = new StringBuilder("Run ");

                    bool first = true;
                    foreach (var task in Children)
                    {
                        if (!first)
                        {
                            builder.Append(", ");
                        }
                        else
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
                if (_TaskName == null)
                {
                    _TaskName = $"Selector {SpecificName}";
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
                if (Children == null || Children.Count == 0)
                    return "In a bad state =(";

                return Children[Counter].PrettyDescription;
            }
        }

        public double Progress
        {
            get
            {
                if (Children == null || Children.Count == 0)
                    return 0;
                return Children[Counter].Progress;
            }
        }

        protected string SpecificName;
        protected int Counter;
        protected bool CounterRunAtleastOnce;

        public List<IEntityTask> Children;

        /// <summary>
        /// Initialize a new selector task with the specified tasks and the specified debug name.
        /// </summary>
        /// <param name="tasks">The tasks</param>
        /// <param name="specificName">The debug name. This task will have the TaskName $"Selector {SpecificName}"</param>
        public EntitySelectorTask(List<IEntityTask> tasks, string specificName)
        {
            Children = tasks;
            SpecificName = specificName;
            Counter = 0;
        }
        
        public EntitySelectorTask(SharedGameState gameState, NetIncomingMessage message)
        {
            var numTasks = message.ReadInt16();

            Children = new List<IEntityTask>(numTasks);
            for(int i = 0; i < numTasks; i++)
            {
                var taskID = message.ReadInt16();

                Children.Add(TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message));
            }

            Counter = message.ReadInt32();
            CounterRunAtleastOnce = message.ReadBoolean();
            SpecificName = message.ReadString();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write((short)Children.Count);

            foreach(var task in Children)
            {
                message.Write(TaskIdentifier.GetIDOfTask(task.GetType()));
                task.Write(message);
            }

            message.Write(Counter);
            message.Write(CounterRunAtleastOnce);
            message.Write(SpecificName);
        }
        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            var currTask = Children[Counter];

            var result = currTask.SimulateTimePassing(gameState, timeMS);

            switch (result)
            {
                case EntityTaskStatus.Running:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Running;
                case EntityTaskStatus.Success:
                    CounterRunAtleastOnce = true;
                    return EntityTaskStatus.Success;
                case EntityTaskStatus.Failure:
                    Counter++;
                    CounterRunAtleastOnce = false;
                    if (Counter == Children.Count)
                        return EntityTaskStatus.Failure;
                    return EntityTaskStatus.Running;
            }

            throw new InvalidProgramException("Can't get here");
        }

        public void Reset(SharedGameState gameState)
        {
            for (int i = 0; i < Counter; i++)
            {
                Children[i].Reset(gameState);
            }

            if (CounterRunAtleastOnce && Counter < Children.Count)
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

        public bool IsValid()
        {
            if (Children.Count == 0)
                return false;

            foreach(var task in Children)
            {
                if (!task.IsValid())
                    return false;
            }

            return true;
        }
    }
}
