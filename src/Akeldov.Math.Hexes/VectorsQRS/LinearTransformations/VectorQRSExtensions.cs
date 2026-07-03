using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Rotates the value using the specified angle.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="angleRad">The angleRad value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS Rotate(this VectorQRS point, float angleRad)
        {
            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            float q = point.Q * cos - point.R * sin;
            float r = point.Q * sin + point.R * cos;

            return new VectorQRS(q, r);
        }

        /// <summary>
        /// Rotates the value using the specified angle.
        /// </summary>
        /// <param name="v">The v value.</param>
        /// <param name="angle">The angle value.</param>
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
