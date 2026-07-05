using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides extension methods for <see cref="VectorXY"/>.
    /// </summary>
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Restricts each component of a vector to the specified inclusive component ranges.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="min">The inclusive minimum vector.</param>
        /// <param name="max">The inclusive maximum vector.</param>
        /// <returns>The clamped vector.</returns>
        /// <exception cref="ArgumentException">Thrown when a maximum component is less than the corresponding minimum component.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Clamp(this VectorXY source, VectorXY min, VectorXY max)
        {
            if (max.X < min.X)
                throw new ArgumentException($"Cannot clamp value: min.X ({min.X}) must be less than or equal to max.X ({max.X}).");

            if (max.Y < min.Y)
                throw new ArgumentException($"Cannot clamp value: min.Y ({min.Y}) must be less than or equal to max.Y ({max.Y}).");

            var x = MathF.Min(MathF.Max(source.X, min.X), max.X);
            var y = MathF.Min(MathF.Max(source.Y, min.Y), max.Y);

            return new VectorXY(x, y);
        }

        /// <summary>
        /// Raises each component to the specified minimum component when it is smaller.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="min">The inclusive minimum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ClampMin(this VectorXY source, VectorXY min)
        {
            var x = MathF.Max(source.X, min.X);
            var y = MathF.Max(source.Y, min.Y);

            return new VectorXY(x, y);
        }

        /// <summary>
        /// Lowers each component to the specified maximum component when it is greater.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="max">The inclusive maximum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ClampMax(this VectorXY source, VectorXY max)
        {
            var x = MathF.Min(source.X, max.X);
            var y = MathF.Min(source.Y, max.Y);

            return new VectorXY(x, y);
        }

        /// <summary>
        /// Raises each component to the specified integer minimum component when it is smaller.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="min">The inclusive minimum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ClampMin(this VectorXY source, VectorXYInt min)
        {
            var x = MathF.Max(source.X, min.X);
            var y = MathF.Max(source.Y, min.Y);

            return new VectorXY(x, y);
        }

        /// <summary>
        /// Lowers each component to the specified integer maximum component when it is greater.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="max">The inclusive maximum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ClampMax(this VectorXY source, VectorXYInt max)
        {
            var x = MathF.Min(source.X, max.X);
            var y = MathF.Min(source.Y, max.Y);

            return new VectorXY(x, y);
        }
    }
}
