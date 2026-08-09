using System;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides extension methods for <see cref="PointXY"/>.
    /// </summary>
    public static partial class PointXYExtensions
    {
        /// <summary>
        /// Linearly interpolates or extrapolates from this point to the specified target point.
        /// </summary>
        /// <param name="source">The interpolation start point.</param>
        /// <param name="target">The interpolation target point.</param>
        /// <param name="t">
        /// The interpolation parameter. A value of 0 returns <paramref name="source"/>,
        /// a value of 1 returns <paramref name="target"/>, values between 0 and 1
        /// interpolate between the points, and values outside that range extrapolate
        /// along the same line.
        /// </param>
        /// <returns>The interpolated or extrapolated point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="t"/> is NaN or infinity.
        /// </exception>
        public static PointXY LerpTo(this PointXY source, PointXY target, float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t))
                throw new ArgumentOutOfRangeException(nameof(t), "Interpolation parameter must be finite.");

            return new PointXY(
                source.X + (target.X - source.X) * t,
                source.Y + (target.Y - source.Y) * t);
        }
    }
}
