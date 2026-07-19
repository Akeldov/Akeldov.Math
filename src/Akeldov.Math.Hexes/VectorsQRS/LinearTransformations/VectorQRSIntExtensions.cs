using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorQRSIntExtensions
    {
        /// <summary>
        /// Rotates the QRS vector counterclockwise in the hex coordinate plane.
        /// </summary>
        /// <param name="point">The integer QRS vector to rotate.</param>
        /// <param name="angleRad">The rotation angle in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="angleRad"/> is not finite.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS Rotate(this VectorQRSInt point, float angleRad)
        {
            return new VectorQRS(point.Q, point.R).Rotate(angleRad);
        }

        /// <summary>
        /// Rotates an integer QRS vector counterclockwise in 60-degree increments.
        /// </summary>
        /// <param name="v">The integer QRS vector to rotate.</param>
        /// <param name="angle">The sixfold counterclockwise rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRSInt Rotate(this VectorQRSInt v, SixfoldAngle angle)
        {
            return angle switch
            {
                SixfoldAngle.Deg0 => v,
                SixfoldAngle.Deg60 => new VectorQRSInt(-v.R, -v.S),
                SixfoldAngle.Deg120 => new VectorQRSInt(v.S, v.Q),
                SixfoldAngle.Deg180 => new VectorQRSInt(-v.Q, -v.R),
                SixfoldAngle.Deg240 => new VectorQRSInt(v.R, v.S),
                SixfoldAngle.Deg300 => new VectorQRSInt(-v.S, -v.Q),
                _ => throw new ArgumentOutOfRangeException(nameof(angle), angle, $"The given angle: {angle} is not a sixfold angle.")
            };
        }
    }
}
