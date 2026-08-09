using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class PointXYExtensions
    {
        /// <summary>
        /// Rotates a point around the origin and then applies an offset.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <param name="offset">The translation offset.</param>
        /// <returns>The transformed point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Transform(this PointXY point, float angle, VectorXY offset)
        {
            VectorXY rotated = new VectorXY(point.X, point.Y).Rotate(angle);
            return new PointXY(rotated.X + offset.X, rotated.Y + offset.Y);
        }

        /// <summary>
        /// Rotates a point around the origin and then applies an integer offset.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <param name="offset">The translation offset.</param>
        /// <returns>The transformed point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Transform(this PointXY point, float angle, VectorXYInt offset)
        {
            return point.Transform(angle, (VectorXY)offset);
        }

        /// <summary>
        /// Scales a point relative to the origin, rotates it around the origin, and then applies an offset.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <param name="scaleFactor">The uniform scale factor.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <param name="offset">The translation offset.</param>
        /// <returns>The transformed point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Transform(this PointXY point, float scaleFactor, float angle, VectorXY offset)
        {
            VectorXY scaled = new VectorXY(point.X, point.Y) * scaleFactor;
            VectorXY rotated = scaled.Rotate(angle);
            return new PointXY(rotated.X + offset.X, rotated.Y + offset.Y);
        }

        /// <summary>
        /// Scales a point relative to the origin, rotates it around the origin, and then applies an integer offset.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <param name="scaleFactor">The uniform scale factor.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <param name="offset">The translation offset.</param>
        /// <returns>The transformed point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Transform(this PointXY point, float scaleFactor, float angle, VectorXYInt offset)
        {
            return point.Transform(scaleFactor, angle, (VectorXY)offset);
        }

        /// <summary>
        /// Rotates a point around the specified pivot.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot point.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <returns>The rotated point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Rotate(this PointXY point, PointXY pivot, float angle)
        {
            VectorXY rotated = (point - pivot).Rotate(angle);
            return pivot + rotated;
        }

        /// <summary>
        /// Rotates a point around the specified integer pivot.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot point.</param>
        /// <param name="angle">The rotation angle in radians.</param>
        /// <returns>The rotated point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointXY Rotate(this PointXY point, VectorXYInt pivot, float angle)
        {
            return point.Rotate(new PointXY(pivot.X, pivot.Y), angle);
        }
    }
}
