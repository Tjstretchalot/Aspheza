using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// Describes the status of an entity task. Looks suspiciously like a behavior tree
    /// </summary>
    public enum EntityTaskStatus
    {
        Success = 0,
        Failure,
        Running
    }
}
