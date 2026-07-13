using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Assigns hex centers to weighted Voronoi sites.
    /// </summary>
    public sealed class VoronoiHexPartitioner
    {
        private readonly Site[] _sites;

        /// <summary>
        /// Initializes a new hex Voronoi partitioner with the specified sites.
        /// </summary>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        public VoronoiHexPartitioner(IReadOnlyList<Site> sites)
        {
            _sites = CopyAndValidateSites(sites);
        }

        /// <summary>
        /// Assigns every center from the specified hex center map to its nearest weighted Voronoi site.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <returns>
        /// A new read-only hex partition map with per-hex assignments and a semantic cell list.
        /// Use <see cref="VoronoiHexPartitionMap.ToMutableHexMap"/> to create a mutable
        /// caller-owned assignment copy.
        /// </returns>
        public VoronoiHexPartitionMap Partition(HexCenterMap hexCenters)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            var count = hexCenters.Topology.Count;
            var cellIndexes = new int[count];
            var hexIndexBuckets = CreateHexIndexBuckets(_sites.Length);

            int flatIndex = 0;
            for (int y = 0; y < hexCenters.Topology.Resolution.Y; y++)
            {
                for (int x = 0; x < hexCenters.Topology.Resolution.X; x++)
                {
                    PointXY center = hexCenters[flatIndex];
                    if (float.IsNaN(center.X) || float.IsInfinity(center.X) ||
                        float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                        throw new ArgumentOutOfRangeException(nameof(hexCenters), "Hex center coordinates must be finite.");

                    int cellIndex = GetNearestWeightedCellIndex(center);
                    cellIndexes[flatIndex] = cellIndex;
                    hexIndexBuckets[cellIndex].Add(new VectorXYInt(x, y));
                    flatIndex++;
                }
            }

            var cells = CreateCells(hexIndexBuckets);

            var assignments = new VoronoiCell[count];
            for (int i = 0; i < assignments.Length; i++)
            {
                assignments[i] = cells[cellIndexes[i]];
            }

            return new VoronoiHexPartitionMap(hexCenters, assignments, cells);
        }

        private static Site[] CopyAndValidateSites(IReadOnlyList<Site> sites)
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));

            if (sites.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(sites));

            bool hasNonZeroWeight = false;
            var copy = new Site[sites.Count];
            for (int i = 0; i < sites.Count; i++)
            {
                var site = sites[i];

                if (float.IsNaN(site.Position.X) || float.IsInfinity(site.Position.X) ||
                    float.IsNaN(site.Position.Y) || float.IsInfinity(site.Position.Y))
                    throw new ArgumentOutOfRangeException(nameof(sites), "Site position coordinates must be finite.");

                if (site.Weight < 0f || float.IsNaN(site.Weight))
                    throw new ArgumentOutOfRangeException(nameof(sites), "Site weight must be non-negative and not NaN.");

                if (site.Weight > 0f)
                    hasNonZeroWeight = true;

                copy[i] = site;
            }

            if (!hasNonZeroWeight)
                throw new ArgumentException("At least one site weight must be positive.", nameof(sites));

            return copy;
        }

        private static List<VectorXYInt>[] CreateHexIndexBuckets(int count)
        {
            var buckets = new List<VectorXYInt>[count];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new List<VectorXYInt>();
            }

            return buckets;
        }

        private VoronoiCell[] CreateCells(List<VectorXYInt>[] hexIndexBuckets)
        {
            var cells = new VoronoiCell[_sites.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new VoronoiCell(i, _sites[i], hexIndexBuckets[i]);
            }

            return cells;
        }

        private int GetNearestWeightedCellIndex(PointXY point)
        {
            double px = point.X;
            double py = point.Y;
            double bestWeightedDistance = double.PositiveInfinity;
            int bestWeightedIndex = 0;
            double bestInfiniteDistance = double.PositiveInfinity;
            int bestInfiniteIndex = -1;

            for (int i = 0; i < _sites.Length; i++)
            {
                ref readonly var site = ref _sites[i];
                if (TryUpdate(
                    ref bestWeightedDistance,
                    ref bestWeightedIndex,
                    ref bestInfiniteDistance,
                    ref bestInfiniteIndex,
                    i,
                    px,
                    py,
                    site.Position.X,
                    site.Position.Y,
                    site.Weight))
                    return i;
            }

            return bestInfiniteIndex >= 0 ? bestInfiniteIndex : bestWeightedIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryUpdate(
            ref double bestWeightedDistance,
            ref int bestWeightedIndex,
            ref double bestInfiniteDistance,
            ref int bestInfiniteIndex,
            int index,
            double px,
            double py,
            double x,
            double y,
            double weight)
        {
            double dx = x - px;
            double dy = y - py;
            double distanceSquared = dx * dx + dy * dy;

            if (distanceSquared <= GeometryConstants.GeometryEpsilonSquared)
                return true;

            if (double.IsPositiveInfinity(weight))
            {
                if (distanceSquared < bestInfiniteDistance)
                {
                    bestInfiniteDistance = distanceSquared;
                    bestInfiniteIndex = index;
                }

                return false;
            }

            if (weight == 0f)
                return false;

            double weightedDistanceSquared = distanceSquared / (weight * weight);
            if (weightedDistanceSquared < bestWeightedDistance)
            {
                bestWeightedDistance = weightedDistanceSquared;
                bestWeightedIndex = index;
            }

            return false;
        }
    }
}
