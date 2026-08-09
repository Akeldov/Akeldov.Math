using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class PointXYExtensions
    {
        /// <summary>
        /// Indicates whether two points are within the specified Euclidean distance tolerance.
        /// </summary>
        /// <param name="source">The first point.</param>
        /// <param name="target">The second point.</param>
        /// <param name="epsilon">The inclusive distance tolerance.</param>
        /// <returns><see langword="true"/> when the points are within tolerance; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(
            this PointXY source,
            PointXY target,
            float epsilon = GeometryConstants.GeometryEpsilon)
        {
            return source.SquaredDistanceTo(target) <= epsilon * epsilon;
        }
    }
}
