using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
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
        public double DeltaX { get; protected set; }

        /// <summary>
        /// Where along the y-axis this vector ends if it starts 
        /// at the origin
        /// </summary>
        public double DeltaY { get; protected set; }

        protected double _Magnitude;

        /// <summary>
        /// The magnitude of this vector.
        /// </summary>
        public double Magnitude
        {
            get
            {
                return 0; // TODO
            }
        }

        protected VectorD2D _UnitVector;

        /// <summary>
        /// The unit vector of this vector. This may be a reference
        /// to this instance if this instance is itself a unit vector.
        /// </summary>
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
        
        /// <summary>
        /// Returns the vector created by scaling this vector
        /// by the specified amount.
        /// </summary>
        /// <param name="scalar">The amount to scale this vector by</param>
        /// <returns>A new vector scaled by scalar</returns>
        public VectorD2D Scale(double scalar)
        {
            return null; // TODO
        }

        /// <summary>
        /// Returns the point at DeltaX, DeltaY
        /// </summary>
        /// <returns>PointD2D representation</returns>
        public PointD2D AsPointD2D()
        {
            return null; // TODO
        }

        public override string ToString()
        {
            return $"<{DeltaX}, {DeltaY}>";
        }
    }
}
