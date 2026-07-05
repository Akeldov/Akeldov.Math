using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides clamping helpers for floating-point values.
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Restricts a value to the specified inclusive range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>The clamped value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float value, float min, float max)
        {
            if (max < min)
                throw new ArgumentException($"Cannot clamp value: min ({min}) must be less than or equal to max ({max}).");

            return MathF.Min(MathF.Max(value, min), max);
        }

        /// <summary>
        /// Raises a value to the specified minimum when it is smaller.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The inclusive minimum value.</param>
        /// <returns>The value or <paramref name="min"/>, whichever is greater.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampMin(this float value, float min)
        {
            return MathF.Max(value, min);
        }

        /// <summary>
        /// Lowers a value to the specified maximum when it is greater.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>The value or <paramref name="max"/>, whichever is smaller.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampMax(this float value, float max)
        {
            return MathF.Min(value, max);
        }
    }
}
