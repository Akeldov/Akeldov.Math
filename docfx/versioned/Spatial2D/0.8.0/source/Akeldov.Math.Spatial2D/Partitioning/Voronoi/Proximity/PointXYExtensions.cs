using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    internal static partial class PointXYExtensions
    {
        /// <summary>
        /// Returns the index of the nearest weighted Voronoi site to the specified point.
        /// </summary>
        /// <param name="sites">The weighted Voronoi sites to search.</param>
        /// <param name="point">The point to assign to a site.</param>
        /// <returns>The nearest site index, using site weight for weighted-distance comparison.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetNearestWeightedSiteIndex(this Site[] sites, PointXY point)
        {
            var px = point.X; var py = point.Y;
            float bestWeightedDistance = float.PositiveInfinity;
            int bestWeightedIndex = 0;
            float bestInfiniteDistance = float.PositiveInfinity;
            int bestInfiniteIndex = -1;

            int i = 0, n = sites.Length;
            for (; i + 3 < n; i += 4)
            {
                ref readonly var s0 = ref sites[i + 0];
                ref readonly var s1 = ref sites[i + 1];
                ref readonly var s2 = ref sites[i + 2];
                ref readonly var s3 = ref sites[i + 3];

                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i + 0,
                    px,
                    py,
                    s0.Position.X,
                    s0.Position.Y,
                    s0.Weight))
                    return i + 0;

                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i + 1,
                    px,
                    py,
                    s1.Position.X,
                    s1.Position.Y,
                    s1.Weight))
                    return i + 1;

                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i + 2,
                    px,
                    py,
                    s2.Position.X,
                    s2.Position.Y,
                    s2.Weight))
                    return i + 2;

                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i + 3,
                    px,
                    py,
                    s3.Position.X,
                    s3.Position.Y,
                    s3.Weight))
                    return i + 3;
            }
            for (; i < n; i++)
            {
                ref readonly var s = ref sites[i];
                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i,
                    px,
                    py,
                    s.Position.X,
                    s.Position.Y,
                    s.Weight))
                    return i;
            }

            return bestInfiniteIndex >= 0 ? bestInfiniteIndex : bestWeightedIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryUpdate(
            ref float bestWeightedDistance,
            ref int bestWeightedIndex,
            ref float bestInfiniteDistance,
            ref int bestInfiniteIndex,
            int idx,
            float px,
            float py,
            float x,
            float y,
            float weight)
        {
            float dx = x - px; float dy = y - py;
            float d2 = dx * dx + dy * dy;

            if (d2 <= GeometryConstants.GeometryEpsilonSquared)
                return true;

            if (float.IsPositiveInfinity(weight))
            {
                if (d2 < bestInfiniteDistance)
                {
                    bestInfiniteDistance = d2;
                    bestInfiniteIndex = idx;
                }

                return false;
            }

            if (weight == 0f)
                return false;

            float weightedD2 = d2 / (weight * weight);
            if (weightedD2 < bestWeightedDistance)
            {
                bestWeightedDistance = weightedD2;
                bestWeightedIndex = idx;
            }

            return false;
        }
    }
}
