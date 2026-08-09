using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides tolerance-based comparison helpers for floating-point values.
    /// </summary>
    public static class GeometryFloatExtensions
    {
        /// <summary>
        /// Indicates whether two values differ by no more than the specified tolerance.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <param name="epsilon">The inclusive comparison tolerance.</param>
        /// <returns><see langword="true"/> when the values are within tolerance; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this float left, float right, float epsilon = GeometryConstants.GeometryEpsilon)
        {
            return MathF.Abs(left - right) <= epsilon;
        }

        /// <summary>
        /// Indicates whether a value is within the specified tolerance of zero.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="epsilon">The inclusive zero tolerance.</param>
        /// <returns><see langword="true"/> when the value is within tolerance of zero; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlmostZero(this float value, float epsilon = GeometryConstants.GeometryEpsilon)
        {
            return MathF.Abs(value) <= epsilon;
        }
    }
}
