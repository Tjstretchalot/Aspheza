using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
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
        /// The text to display when this is hovered on. May be null
        /// </summary>
        string HoverText { get; }

        /// <summary>
        /// Renders this instance at the specified location. When rendering any internal
        /// position is not taken into account - a sprite or equivalent is rendered at the 
        /// specified position on the screen.
        /// </summary>
        /// <param name="context">Render context.</param>
        /// <param name="bounds">Bounds to render in.</param>
        /// <param name="overlay">The color to overlay</param>
        /// <param name="timeSinceLastRenderMS">The time since last render in milliseconds. May be used for animations</param>
        void Render(RenderContext context, PointD2D screenTopLeft, Color overlay);
    }
}
