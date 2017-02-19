using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// A repeater task simply repeats another task
    /// </summary>
    public class EntitySucceederTask : IEntityTask
    {
        protected string _TaskDescription;
        public string TaskDescription
        {
            get
            {
                if(_TaskDescription == null)
                {
                    _TaskDescription = $"Run {Task.TaskName} and Failure/Success => Success";
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
                    _TaskName = $"Succeed ({SpecificName})";
                }

                return _TaskName;
            }
        }

        public string TaskStatus
        {
            get
            {
                return "Succeeding";
            }
        }

        protected IEntityTask Task;
        protected string SpecificName;
        protected bool TaskRunSinceLastReset;

        /// <summary>
        /// Create a new succeeder task with the task to repeat and a specific name
        /// </summary>
        /// <param name="task"></param>
        /// <param name="specificName"></param>
        public EntitySucceederTask(IEntityTask task, string specificName)
        {
            Task = task;
            SpecificName = specificName;
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
                    return EntityTaskStatus.Success;
                case EntityTaskStatus.Running:
                    return result;
            }

            throw new InvalidProgramException("Can't get here");
        }
    }
}
