using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Rotates a vector around the origin by the specified angle.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <returns>The rotated vector.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXY vector, float angle)
        {
            GeometryConstants.ValidateFiniteAngle(angle, nameof(angle));

            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            float x = vector.X * cos - vector.Y * sin;
            float y = vector.X * sin + vector.Y * cos;

            return new VectorXY(x, y);
        }
    }
}
