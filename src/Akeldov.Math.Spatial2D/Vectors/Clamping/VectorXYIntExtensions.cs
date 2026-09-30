using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides extension methods for <see cref="VectorXYInt"/>.
    /// </summary>
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Restricts each component of an integer vector to the specified inclusive component ranges.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="min">The inclusive minimum vector.</param>
        /// <param name="max">The inclusive maximum vector.</param>
        /// <returns>The clamped vector.</returns>
        /// <exception cref="ArgumentException">Thrown when a maximum component is less than the corresponding minimum component.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt Clamp(this VectorXYInt source, VectorXYInt min, VectorXYInt max)
        {
#pragma warning disable MA0015 // Specify the parameter name in ArgumentException
            if (max.X < min.X)
                throw new ArgumentException($"Cannot clamp value: min.X ({min.X}) must be less than or equal to max.X ({max.X}).");

            if (max.Y < min.Y)
                throw new ArgumentException($"Cannot clamp value: min.Y ({min.Y}) must be less than or equal to max.Y ({max.Y}).");
#pragma warning restore MA0015 // Specify the parameter name in ArgumentException

            int x = System.Math.Min(System.Math.Max(source.X, min.X), max.X);
            int y = System.Math.Min(System.Math.Max(source.Y, min.Y), max.Y);

            return new VectorXYInt(x, y);
        }

        /// <summary>
        /// Raises each component to the specified minimum component when it is smaller.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="min">The inclusive minimum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ClampMin(this VectorXYInt source, VectorXYInt min)
        {
            int x = System.Math.Max(source.X, min.X);
            int y = System.Math.Max(source.Y, min.Y);

            return new VectorXYInt(x, y);
        }

        /// <summary>
        /// Lowers each component to the specified maximum component when it is greater.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="max">The inclusive maximum vector.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ClampMax(this VectorXYInt source, VectorXYInt max)
        {
            int x = System.Math.Min(source.X, max.X);
            int y = System.Math.Min(source.Y, max.Y);

            return new VectorXYInt(x, y);
        }

        /// <summary>
        /// Raises each component to the specified minimum component when it is smaller.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="minX">The inclusive minimum X component.</param>
        /// <param name="minY">The inclusive minimum Y component.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ClampMin(this VectorXYInt source, int minX, int minY)
        {
            int x = System.Math.Max(source.X, minX);
            int y = System.Math.Max(source.Y, minY);

            return new VectorXYInt(x, y);
        }

        /// <summary>
        /// Lowers each component to the specified maximum component when it is greater.
        /// </summary>
        /// <param name="source">The vector to clamp.</param>
        /// <param name="maxX">The inclusive maximum X component.</param>
        /// <param name="maxY">The inclusive maximum Y component.</param>
        /// <returns>The clamped vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ClampMax(this VectorXYInt source, int maxX, int maxY)
        {
            int x = System.Math.Min(source.X, maxX);
            int y = System.Math.Min(source.Y, maxY);

            return new VectorXYInt(x, y);
        }
    }
}
