using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities.Animations
{
    public enum AnimationType
    {
        Idle = 0, Standing, Moving, Chopping, Logging, Casting,

        /// <summary>
        /// Typically used for unbuilt buildings with less than
        /// 30% progress to completion
        /// </summary>
        Unbuilt,
        /// <summary>
        /// Typically used for unbuilt buildings with 30% to 60%
        /// progress to completion
        /// </summary>
        UnbuiltThirty,
        /// <summary>
        /// Typically used for unbuilt buildings with 60% to 90%
        /// progress to completion
        /// </summary>
        UnbuiltSixty,
        /// <summary>
        /// Typically used for unbuild buildings with 90% to 100%
        /// progress to completion
        /// </summary>
        UnbuiltNinety
    }
}
