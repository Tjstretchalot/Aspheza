using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math.Double
{
    /// <summary>
    /// Describes a vector. A vector has direction and magnitude, but not 
    /// location.
    /// </summary>
    public class VectorD2D
    {
        /// <summary>
        /// Where along the x-axis this vector ends if it starts
        /// at the origin
        /// </summary>
        public double DeltaX;

        /// <summary>
        /// Where along the y-axis this vector ends if it starts 
        /// at the origin
        /// </summary>
        public double DeltaY;

        protected VectorD2D _UnitVector;
        public VectorD2D UnitVector
        {
            get
            {
                return null; // TODO
            }
        }

        /// <summary>
        /// Constructs a new vector, described by the ending
        /// point if it were positioned at the origin.
        /// </summary>
        /// <param name="dx">Delta x of the vector</param>
        /// <param name="dy">Delta y of the vector</param>
        public VectorD2D(double dx, double dy)
        {
            DeltaX = dx;
            DeltaY = dy;
        }
        

    }
}
