using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Spatial3D namespace.
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130
{
    public static partial class PointXYZExtensions
    {
        /// <summary>
        /// Returns the squared Euclidean distance from this point to the specified point.
        /// </summary>
        /// <param name="source">The point to measure from.</param>
        /// <param name="target">The point to measure to.</param>
        /// <returns>The squared Euclidean distance between the points.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SquaredDistanceTo(this PointXYZ source, PointXYZ target)
        {
            float dx = target.X - source.X;
            float dy = target.Y - source.Y;
            float dz = target.Z - source.Z;

            return dx * dx + dy * dy + dz * dz;
        }
    }
}
