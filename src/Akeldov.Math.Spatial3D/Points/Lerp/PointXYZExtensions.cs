using System;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Spatial3D namespace.
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130
{
    /// <summary>
    /// Provides extension methods for <see cref="PointXYZ"/>.
    /// </summary>
    public static partial class PointXYZExtensions
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
        public static PointXYZ LerpTo(this PointXYZ source, PointXYZ target, float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t))
                throw new ArgumentOutOfRangeException(nameof(t), "Interpolation parameter must be finite.");

            if (t == 0f)
                return source;

            if (t == 1f)
                return target;

            return new PointXYZ(
                (float)(source.X + ((double)target.X - source.X) * t),
                (float)(source.Y + ((double)target.Y - source.Y) * t),
                (float)(source.Z + ((double)target.Z - source.Z) * t));
        }
    }
}
