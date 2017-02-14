using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a component that can be resized
    /// </summary>
    public interface IResizableComponent : IScreenComponent
    {
        /// <summary>
        /// The minimum width and height of this component.
        /// </summary>
        Point MinSize { get; }

        /// <summary>
        /// The maximum width and height of this component.
        /// </summary>
        Point MaxSize { get; }

        /// <summary>
        /// Resizes this component to the specified size
        /// </summary>
        /// <param name="size">Thew new size of this component</param>
        void Resize(Point size);
    }
}
