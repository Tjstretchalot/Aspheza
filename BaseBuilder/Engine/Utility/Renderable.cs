using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Utility
{
    /// <summary>
    /// Describes something that can be rendered.
    /// </summary>
    public interface Renderable
    {
        /// <summary>
        /// Renders this instance inside the specified bounds. When rendering position
        /// is not taken into account - a sprite or equivalent is rendered inside the 
        /// bounds without regard to anything else.
        /// </summary>
        /// <param name="context">Render context.</param>
        /// <param name="bounds">Bounds to render in.</param>
        void Render(RenderContext context, RectangleD2D bounds);
    }
}
