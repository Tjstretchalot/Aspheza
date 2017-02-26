﻿using System;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;
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
        public static VectorD2D X_AXIS = new VectorD2D(1, 0);
        public static VectorD2D Y_AXIS = new VectorD2D(0, 1);

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

        protected double? _Slope;

        /// <summary>
        /// The slope of this VectorD2D
        /// </summary>
        public double Slope
        {
            get
            {
                if(!_Slope.HasValue)
                {
                    if(EpsilonEqual(DeltaX, 0))
                    {
                        _Slope = double.PositiveInfinity;
                    }else
                    {
                        _Slope = DeltaY / DeltaX;
                    }
                }

                return _Slope.Value;
            }
        }

        protected double? _MagnitudeSquared;

        /// <summary>
        /// The squared magnitude of this vector
        /// </summary>
        public double MagnitudeSquared
        {
            get
            {
                if (!_MagnitudeSquared.HasValue)
                {
                    _MagnitudeSquared = (DeltaX * DeltaX) + (DeltaY * DeltaY);
                }

                return _MagnitudeSquared.Value;
            }
        }

        protected double? _Magnitude;

        /// <summary>
        /// The magnitude of this vector.
        /// </summary>
        public double Magnitude
        {
            get
            {
                if (!_Magnitude.HasValue)
                {
                    _Magnitude = Math.Sqrt(MagnitudeSquared);
                }

                return _Magnitude.Value;
            }
        }

        private double? _Theta;

        /// <summary>
        /// Gets the angle of this vector counter-clockwise from positive x.
        /// </summary>
        public double Theta
        {
            get
            {
                if(!_Theta.HasValue)
                {
                    _Theta = Math.Atan2(DeltaY, DeltaX);
                }

                return _Theta.Value;
            }
        }

        private double? _CosTheta;
        
        /// <summary>
        /// Math.Cos(Theta)
        /// </summary>
        public double CosTheta
        {
            get
            {
                if (!_CosTheta.HasValue)
                    _CosTheta = Math.Cos(Theta);

                return _CosTheta.Value;
            }
        }

        private double? _SinTheta;

        /// <summary>
        /// Math.Sin(Theta)
        /// </summary>
        public double SinTheta
        {
            get
            {
                if (!_SinTheta.HasValue)
                    _SinTheta = Math.Sin(Theta);

                return _SinTheta.Value;
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
                if (_UnitVector == null)
                {
                    if (EpsilonEqual(Magnitude, 1))
                        _UnitVector = this;
                    else
                        _UnitVector = new VectorD2D(DeltaX / Magnitude, DeltaY / Magnitude);
                }

                return _UnitVector;
            }
        }
           
        /// <summary>
        /// Constructs a new vector, described by the ending
        /// point if it were positioned at the origin.
        /// </summary>
        /// <param name="dx">Delta x of the vector</param>
        /// <param name="dy">Delta y of the vector</param>
        /// <exception cref="InvalidProgramException">If dx == dy == 0</exception>
        public VectorD2D(double dx, double dy)
        {
            if (EpsilonEqual(dx, 0) && EpsilonEqual(dy, 0))
                throw new InvalidProgramException("A 0-vector is not allowed.");

            DeltaX = dx;
            DeltaY = dy;
        }
        
        /// <summary>
        /// Returns the vector created by scaling this vector
        /// by the specified amount.
        /// </summary>
        /// <param name="scalar">The amount to scale this vector by</param>
        /// <returns>A new vector scaled by scalar</returns>
        /// <exception cref="InvalidProgramException">If scalar is 0</exception>
        public VectorD2D Scale(double scalar)
        {
            if (EpsilonEqual(scalar, 0))
                throw new ArgumentNullException("Scalar cannot be 0 (a 0-vector is not allowed)");

            return new VectorD2D(DeltaX * scalar, DeltaY * scalar);
        }

        /// <summary>
        /// Checks if this vector is parallel to the other vector. Note that two
        /// vectors are considered parallel even if they are going in opposite directions.
        /// </summary>
        /// <param name="other">The other vector</param>
        /// <returns>True if parallel, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool IsParallel(VectorD2D other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            
            return Slope == other.Slope;
        }

        /// <summary>
        /// Returns true if this vector is going in the opposite direction of the other
        /// vector. Otherwise, returns false.
        /// </summary>
        /// <param name="other">The vector to compare wtih</param>
        /// <returns>True if this and other is antiparallel, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If other is null</exception>
        public bool IsAntiParallel(VectorD2D other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return UnitVector == other.UnitVector.Scale(-1);
        }

        /// <summary>
        /// Returns the point at DeltaX, DeltaY
        /// </summary>
        /// <returns>PointD2D representation</returns>
        public PointD2D AsPointD2D()
        {
            return new PointD2D(DeltaX, DeltaY);
        }

        public static VectorD2D operator +(VectorD2D v1, VectorD2D v2)
        {
            return new VectorD2D(v1.DeltaX + v2.DeltaX, v1.DeltaY + v2.DeltaY);
        }

        public static VectorD2D operator -(VectorD2D v1, VectorD2D v2)
        {
            return new VectorD2D(v1.DeltaX + v2.DeltaX, v1.DeltaY + v2.DeltaY);
        }

        public static VectorD2D operator -(VectorD2D v)
        {
            return new VectorD2D(-v.DeltaX, -v.DeltaY);
        }

        public static VectorD2D operator *(VectorD2D v1, double scalar)
        {
            return new VectorD2D(v1.DeltaX * scalar, v1.DeltaY * scalar);
        }

        public static VectorD2D operator /(VectorD2D v1, double divisor)
        {
            return new VectorD2D(v1.DeltaX / divisor, v1.DeltaY / divisor);
        }

        public static bool operator ==(VectorD2D v1, VectorD2D v2)
        {
            if (ReferenceEquals(v1, null) || ReferenceEquals(v2, null))
                return ReferenceEquals(v1, v2);

            return EpsilonEqual(v1.DeltaX, v2.DeltaX) && EpsilonEqual(v1.DeltaY, v2.DeltaY);
        }

        public static bool operator !=(VectorD2D v1, VectorD2D v2)
        {
            if (ReferenceEquals(v1, null) || ReferenceEquals(v2, null))
                return !ReferenceEquals(v1, v2);

            return !EpsilonEqual(v1.DeltaX, v2.DeltaX) || !EpsilonEqual(v1.DeltaY, v2.DeltaY);
        }

        public override bool Equals(object obj)
        {
            var v2 = obj as VectorD2D;

            if (ReferenceEquals(v2, null))
                return false;

            return this == v2;
        }

        public override int GetHashCode()
        {
            var result = 31;

            result = result * 17 + DeltaX.GetHashCode();
            result = result * 17 + DeltaY.GetHashCode();

            return result;
        }

        public override string ToString()
        {
            return $"<{DeltaX}, {DeltaY}>";
        }
    }
}
