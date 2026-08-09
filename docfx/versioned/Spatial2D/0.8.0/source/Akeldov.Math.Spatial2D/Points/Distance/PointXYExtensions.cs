using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class PointXYExtensions
    {
        /// <summary>
        /// Returns the squared Euclidean distance from this point to the specified point.
        /// </summary>
        /// <param name="source">The point to measure from.</param>
        /// <param name="target">The point to measure to.</param>
        /// <returns>The squared Euclidean distance between the points.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SquaredDistanceTo(this PointXY source, PointXY target)
        {
            float dx = target.X - source.X;
            float dy = target.Y - source.Y;

            return dx * dx + dy * dy;
        }
    }
}
