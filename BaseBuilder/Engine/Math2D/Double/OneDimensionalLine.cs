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
    /// 
    /// Note you can only compare these lines if they are aligned to the same axis.
    /// </remarks>
    public class OneDimensionalLine
    {
        /// <summary>
        /// The axis that this line is along.
        /// </summary>
        public VectorD2D Axis;

        /// <summary>
        /// The distance from the origin this line starts, along
        /// the axis that created it.
        /// </summary>
        public double Start;

        /// <summary>
        /// The distance from the origin this line ends, along the
        /// axis that created it.
        /// </summary>
        public double End;

        /// <summary>
        /// Initializes the one dimensional line from start to end. 
        /// </summary>
        /// <param name="axis">The axis this line is on</param>
        /// <param name="start">The start of the line</param>
        /// <param name="end">The end of the line</param>
        /// <exception cref="ArgumentNullException">If axis is null</exception>
        /// <exception cref="InvalidProgramException">If start and end are the same</exception>
        public OneDimensionalLine(VectorD2D axis, double start, double end)
        {
            if (axis == null)
                throw new ArgumentNullException(nameof(axis));

            if (start == end)
                throw new InvalidProgramException($"Start cannot match end (both are {start})");

            Axis = axis;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Determines if this line intersects the other line. 
        /// </summary>
        /// <param name="other">The line to compare with</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns>True if the lines intersect, false otherwise</returns>
        /// <exception cref="InvalidProgramException">If this line and other are not on the same axis</exception>
        public bool Intersects(OneDimensionalLine other, bool strict = false)
        {
            return false; // TODO
        }

        /// <summary>
        /// Returns the largest line that both this line and the other line 
        /// include, if there is strict intersection between the lines.
        /// 
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="other">The line to compare with</param>
        /// <returns>The largest line both this line and other include or null if no strict intersection.</returns>
        /// <exception cref="InvalidProgramException">If this line and other are not on the same axis</exception>
        public OneDimensionalLine IntersectionLine(OneDimensionalLine other)
        {
            return null; // TODO
        }
        
        /// <summary>
        /// Converts this one dimensional line representation to the standard
        /// finite line representation.
        /// </summary>
        /// <returns>Finite line representation.</returns>
        public FiniteLineD2D AsFiniteLineD2D()
        {
            return null; // TODO
        }
    }
}
