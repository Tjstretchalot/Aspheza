using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    /// <summary>
    /// Describes a "one-dimensional line". All lines are one dimensional
    /// in that they have no width, but this specifically refers to a line
    /// that is only on one axis.
    /// </summary>
    /// <remarks>
    /// If you project a normal line onto an axis you get a new line. However,
    /// that line doesn't have the same level of information. The resulting line
    /// can be described by it's distance from the origin along the axis. 
    /// 
    /// I.e. if a line is projected onto an axis &lt;1, 1&gt; resulting in &lt;-0.5, -0.5&gt;
    /// to &lt;0.5, 0.5&gt; this could also be referred to as simply the line along the axis
    /// &lt;1, 1&gt; starting at -(Math.Sqrt(1/2 * 1/2 + 1/2 * 1/2)) to Math.Sqrt(1/2 * 1/2 + 1/2 * 1/2)
    /// which is very easily compared with other lines projected onto that axis.
    /// </remarks>
    public class OneDimensionalLine
    {
        public double Start;
        public double End;
    }
}
