using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Rotates the QRS vector counterclockwise in the hex coordinate plane.
        /// </summary>
        /// <param name="point">The QRS vector to rotate.</param>
        /// <param name="angleRad">The rotation angle in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> contains a non-finite component or
        /// <paramref name="angleRad"/> is not finite.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS Rotate(this VectorQRS point, float angleRad)
        {
            if (float.IsNaN(point.Q) || float.IsInfinity(point.Q) ||
                float.IsNaN(point.R) || float.IsInfinity(point.R) ||
                float.IsNaN(point.S) || float.IsInfinity(point.S))
                throw new ArgumentOutOfRangeException(nameof(point), point, "QRS vector components must be finite.");

            if (float.IsNaN(angleRad) || float.IsInfinity(angleRad))
                throw new ArgumentOutOfRangeException(nameof(angleRad), angleRad, "Rotation angle must be finite.");

            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);
            float sinOverSqrt3 = sin * 0.5773502691896258f;
            float twoSinOverSqrt3 = 2f * sinOverSqrt3;

            float q = point.Q * (cos - sinOverSqrt3) - point.R * twoSinOverSqrt3;
            float r = point.Q * twoSinOverSqrt3 + point.R * (cos + sinOverSqrt3);

            return new VectorQRS(q, r);
        }

        /// <summary>
        /// Rotates a QRS vector counterclockwise in 60-degree increments.
        /// </summary>
        /// <param name="v">The QRS vector to rotate.</param>
        /// <param name="angle">The sixfold counterclockwise rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS Rotate(this VectorQRS v, SixfoldAngle angle)
        {
            return angle switch
            {
                SixfoldAngle.Deg0 => v,
                SixfoldAngle.Deg60 => new VectorQRS(-v.R, -v.S),
                SixfoldAngle.Deg120 => new VectorQRS(v.S, v.Q),
                SixfoldAngle.Deg180 => new VectorQRS(-v.Q, -v.R),
                SixfoldAngle.Deg240 => new VectorQRS(v.R, v.S),
                SixfoldAngle.Deg300 => new VectorQRS(-v.S, -v.Q),
                _ => throw new ArgumentOutOfRangeException(nameof(angle), angle, $"The given angle: {angle} is not a sixfold angle.")
            };
        }
    }
}
