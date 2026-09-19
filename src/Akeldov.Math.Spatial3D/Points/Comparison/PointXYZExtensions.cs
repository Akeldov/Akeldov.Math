using System;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Spatial3D namespace.
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130
{
    public static partial class PointXYZExtensions
    {
        /// <summary>
        /// Indicates whether two points are within the specified Euclidean distance tolerance.
        /// </summary>
        /// <param name="source">The first point.</param>
        /// <param name="target">The second point.</param>
        /// <param name="epsilon">The inclusive, finite, non-negative distance tolerance.</param>
        /// <returns>
        /// <see langword="true"/> when the points are within tolerance; otherwise, <see langword="false"/>.
        /// Points with infinite coordinates are not considered almost equal.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="epsilon"/> is negative, NaN, or infinity.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(
            this PointXYZ source,
            PointXYZ target,
            float epsilon = GeometryConstants.GeometryEpsilon)
        {
            if (epsilon < 0f || float.IsNaN(epsilon) || float.IsInfinity(epsilon))
                throw new ArgumentOutOfRangeException(nameof(epsilon), "Distance tolerance must be finite and non-negative.");

            double dx = (double)target.X - source.X;
            double dy = (double)target.Y - source.Y;
            double dz = (double)target.Z - source.Z;

            return dx * dx + dy * dy + dz * dz <= (double)epsilon * epsilon;
        }
    }
}
