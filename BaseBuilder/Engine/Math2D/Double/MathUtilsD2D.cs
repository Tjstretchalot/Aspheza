using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Math2D.Double
{
    public class MathUtilsD2D
    {
        /// <summary>
        /// Numbers that are within this amount should be considered equal.
        /// </summary>
        public const double EPSILON = 0.000001;

        /// <summary>
        /// Compares d1 with d2 to see if they are equal.
        /// </summary>
        /// <param name="d1">First double</param>
        /// <param name="d2">Second double</param>
        /// <param name="epsilon">Epsilon to use, default EPSILON</param>
        /// <returns>True if within epsilon, false otherwise</returns>
        public static bool EpsilonEqual(double d1, double d2, double epsilon = EPSILON)
        {
            return d1 == d2 || Math.Abs(d1 - d2) < epsilon;
        }

        /// <summary>
        /// Compares d1 with d2 to see if d1 &lt;= d2 within epsilon.
        /// </summary>
        /// <param name="d1">First double</param>
        /// <param name="d2">Second double</param>
        /// <param name="epsilon">Epsilon to use, default EPSILON</param>
        /// <returns>True if equal within epsilon or d1 &lt; d2</returns>
        public static bool EpsilonLessThanOrEqual(double d1, double d2, double epsilon = EPSILON)
        {
            return d1 < d2 || EpsilonEqual(d1, d2, epsilon);
        }

        /// <summary>
        /// Compares d1 with d2 to see if d1 &lt; d2 of at least epsilon.
        /// </summary>
        /// <param name="d1">First double</param>
        /// <param name="d2">Second double</param>
        /// <param name="epsilon">Epsilon to use, default EPSILON</param>
        /// <returns>True if not equal within epsilon and d1 &lt; d2</returns>
        public static bool EpsilonLessThan(double d1, double d2, double epsilon = EPSILON)
        {
            return d1 < d2 && !EpsilonEqual(d1, d2, epsilon);
        }

        /// <summary>
        /// Compares d1 with d2 to see if they are equal within epsilon or d1 &gt; d2.
        /// </summary>
        /// <param name="d1">First double</param>
        /// <param name="d2">Second double</param>
        /// <param name="epsilon">Epsilon to use, default EPSILON</param>
        /// <returns>True if equal within epsilon or d1 &gt; d2</returns>
        public static bool EpsilonGreaterThanOrEqual(double d1, double d2, double epsilon = EPSILON)
        {
            return d1 > d2 || EpsilonEqual(d1, d2, epsilon);
        }

        /// <summary>
        /// Compares d1 with d2 to see if d1 &gt; d2 of at least epsilon.
        /// </summary>
        /// <param name="d1">First double</param>
        /// <param name="d2">Second double</param>
        /// <param name="epsilon">Epsilon to use, default EPSILON</param>
        /// <returns>True if not equal within epsilon and d1 &gt; d2</returns>
        public static bool EpsilonGreaterThan(double d1, double d2, double epsilon = EPSILON)
        {
            return d1 > d2 && !EpsilonEqual(d1, d2, epsilon);
        }
    }
}
