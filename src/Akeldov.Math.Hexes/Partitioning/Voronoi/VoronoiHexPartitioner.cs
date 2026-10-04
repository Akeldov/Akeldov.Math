using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
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
        private readonly EmptyCellPolicy _emptyCellPolicy;

        /// <summary>
        /// Initializes a new hex Voronoi partitioner with the specified sites, preserving empty cells.
        /// </summary>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        public VoronoiHexPartitioner(IReadOnlyList<Site> sites)
            : this(sites, EmptyCellPolicy.LeaveAsIs)
        {
        }

        /// <summary>
        /// Initializes a new hex Voronoi partitioner with empty-cell handling.
        /// </summary>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no hexes.</param>
        public VoronoiHexPartitioner(IReadOnlyList<Site> sites, EmptyCellPolicy emptyCellPolicy)
        {
            _sites = CopyAndValidateSites(sites);
            _emptyCellPolicy = emptyCellPolicy;
        }

        /// <summary>
        /// Assigns every center from the specified hex center map to its nearest weighted Voronoi site.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <returns>
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// Use <see cref="VoronoiHexPartitionMap.ToMutableHexMap"/> to create a mutable
        /// caller-owned assignment copy.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// </exception>
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

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite);

            var assignments = new int[count];
            for (int i = 0; i < assignments.Length; i++)
            {
                assignments[i] = cellIndexesBySite[cellIndexes[i]];
            }

            return new VoronoiHexPartitionMap(hexCenters, assignments, cells);
        }

        /// <summary>
        /// Assigns participating centers from the specified hex center map to their nearest weighted
        /// Voronoi site.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="participationMask">
        /// The Boolean map that indicates which hex centers participate in the partition.
        /// </param>
        /// <returns>
        /// A new read-only masked hex partition map with nullable per-hex cell identifiers and a semantic cell list.
        /// Excluded hexes have no assignment and return <see langword="null"/> from the result map.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// </exception>
        public MaskedVoronoiHexPartitionMap Partition(
            HexCenterMap hexCenters,
            IHexMap<bool> participationMask)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));

            if (hexCenters.Topology != participationMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(participationMask));

            var count = hexCenters.Topology.Count;
            var cellIndexes = new int[count];
            var participationMaskValues = new bool[count];
            var hexIndexBuckets = CreateHexIndexBuckets(_sites.Length);

            int flatIndex = 0;
            for (int y = 0; y < hexCenters.Topology.Resolution.Y; y++)
            {
                for (int x = 0; x < hexCenters.Topology.Resolution.X; x++)
                {
                    if (!participationMask[flatIndex])
                    {
                        flatIndex++;
                        continue;
                    }

                    participationMaskValues[flatIndex] = true;

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

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite);

            var assignments = new int?[count];
            for (int i = 0; i < assignments.Length; i++)
            {
                if (participationMaskValues[i])
                    assignments[i] = cellIndexesBySite[cellIndexes[i]];
            }

            return new MaskedVoronoiHexPartitionMap(hexCenters, assignments, cells, participationMaskValues);
        }

        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site in the same mask region.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="regionsMask">
        /// The map of region identifiers with the same topology as <paramref name="hexCenters"/>.
        /// Equal values identify the same region, including disconnected hexes. All integer values,
        /// including zero and negative values, are valid region identifiers; no hexes are excluded.
        /// </param>
        /// <returns>
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// </returns>
        /// <remarks>
        /// A site's region is the mask value at the hex containing its position, using the center map's
        /// geometry. Sites outside the map receive no hexes and follow the configured empty-cell policy.
        /// Weighted distances and tie-breaking follow the same rules as the unmasked partition.
        /// </remarks>
        /// <exception cref="ArgumentNullException">A map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The maps have different topologies.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A hex center coordinate is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public VoronoiHexPartitionMap Partition(
            HexCenterMap hexCenters,
            IHexMap<int> regionsMask)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));

            if (hexCenters.Topology != regionsMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(regionsMask));

            var siteIndexesByRegion = GroupSiteIndexesByRegion(hexCenters.Geometry, regionsMask);
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

                    int region = regionsMask[flatIndex];
                    if (!siteIndexesByRegion.TryGetValue(region, out var siteIndexes))
                        throw new InvalidOperationException($"Region {region} contains no Voronoi sites.");

                    int cellIndex = GetNearestWeightedCellIndex(center, siteIndexes);
                    if (cellIndex < 0)
                        throw new InvalidOperationException($"Region {region} has no eligible Voronoi site for hex ({x}, {y}).");

                    cellIndexes[flatIndex] = cellIndex;
                    hexIndexBuckets[cellIndex].Add(new VectorXYInt(x, y));
                    flatIndex++;
                }
            }

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite);

            var assignments = new int[count];
            for (int i = 0; i < assignments.Length; i++)
            {
                assignments[i] = cellIndexesBySite[cellIndexes[i]];
            }

            return new VoronoiHexPartitionMap(hexCenters, assignments, cells);
        }

        /// <summary>
        /// Assigns participating hex centers to their nearest weighted Voronoi site in the same region.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="participationMask">The Boolean map indicating which hex centers participate.</param>
        /// <param name="regionsMask">
        /// The region identifiers, with the same topology as the center map. Equal integer values
        /// identify the same region, including disconnected hexes; zero and negative values are valid.
        /// </param>
        /// <returns>
        /// A new read-only masked partition map of nullable cell identifiers with a semantic cell list. Excluded hexes return null.
        /// </returns>
        /// <remarks>
        /// A site's region is determined by the hex containing its position, regardless of that hex's
        /// participation. Sites outside the map receive no hexes. Empty-cell handling applies to all sites.
        /// </remarks>
        /// <exception cref="ArgumentNullException">A map is null.</exception>
        /// <exception cref="ArgumentException">The maps have different topologies.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A participating center coordinate is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A participating hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public MaskedVoronoiHexPartitionMap Partition(
            HexCenterMap hexCenters,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));

            if (hexCenters.Topology != participationMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(participationMask));

            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));

            if (hexCenters.Topology != regionsMask.Topology)
                throw new ArgumentException("Hex center map and regions mask must have the same topology.", nameof(regionsMask));

            var siteIndexesByRegion = GroupSiteIndexesByRegion(hexCenters.Geometry, regionsMask);
            var cellIndexes = new int[hexCenters.Topology.Count];
            var participationMaskValues = new bool[hexCenters.Topology.Count];
            var hexIndexBuckets = CreateHexIndexBuckets(_sites.Length);

            int flatIndex = 0;
            for (int y = 0; y < hexCenters.Topology.Resolution.Y; y++)
            {
                for (int x = 0; x < hexCenters.Topology.Resolution.X; x++)
                {
                    if (!participationMask[flatIndex])
                    {
                        flatIndex++;
                        continue;
                    }

                    participationMaskValues[flatIndex] = true;
                    PointXY center = hexCenters[flatIndex];
                    if (float.IsNaN(center.X) || float.IsInfinity(center.X) ||
                        float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                        throw new ArgumentOutOfRangeException(nameof(hexCenters), "Hex center coordinates must be finite.");

                    int region = regionsMask[flatIndex];
                    if (!siteIndexesByRegion.TryGetValue(region, out var siteIndexes))
                        throw new InvalidOperationException($"Region {region} contains no Voronoi sites.");

                    int cellIndex = GetNearestWeightedCellIndex(center, siteIndexes);
                    if (cellIndex < 0)
                        throw new InvalidOperationException($"Region {region} has no eligible Voronoi site for hex ({x}, {y}).");

                    cellIndexes[flatIndex] = cellIndex;
                    hexIndexBuckets[cellIndex].Add(new VectorXYInt(x, y));
                    flatIndex++;
                }
            }

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite);

            var assignments = new int?[hexCenters.Topology.Count];
            for (int i = 0; i < assignments.Length; i++)
                if (participationMaskValues[i])
                    assignments[i] = cellIndexesBySite[cellIndexes[i]];

            return new MaskedVoronoiHexPartitionMap(hexCenters, assignments, cells, participationMaskValues);
        }

        private Dictionary<int, List<int>> GroupSiteIndexesByRegion(HexMapGeometry geometry, IHexMap<int> regionsMask)
        {
            var siteIndexesByRegion = new Dictionary<int, List<int>>();
            for (int i = 0; i < _sites.Length; i++)
            {
                VectorXYInt index = _sites[i].Position.ToXYIndex(geometry.Radius, geometry.Origin, geometry.Topology.Layout);
                if (index.X < 0 || index.X >= geometry.Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= geometry.Topology.Resolution.Y)
                    continue;

                int region = regionsMask[index];
                if (!siteIndexesByRegion.TryGetValue(region, out var siteIndexes))
                {
                    siteIndexes = new List<int>();
                    siteIndexesByRegion.Add(region, siteIndexes);
                }

                siteIndexes.Add(i);
            }

            return siteIndexesByRegion;
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

        private VoronoiCell[] CreateCells(List<VectorXYInt>[] hexIndexBuckets, out int[] cellIndexesBySite)
        {
            var cells = new List<VoronoiCell>(_sites.Length);
            cellIndexesBySite = new int[_sites.Length];
            for (int i = 0; i < _sites.Length; i++)
            {
                if (hexIndexBuckets[i].Count == 0)
                {
                    if (_emptyCellPolicy == EmptyCellPolicy.Exclude)
                        continue;

                    if (_emptyCellPolicy != EmptyCellPolicy.LeaveAsIs)
                        throw new InvalidOperationException($"Couldn't tessellate by empty cells, empty cell: {_sites[i]}.");
                }

                cellIndexesBySite[i] = cells.Count;
                cells.Add(new VoronoiCell(cells.Count, _sites[i], hexIndexBuckets[i]));
            }

            return cells.ToArray();
        }

        private int GetNearestWeightedCellIndex(PointXY point, List<int>? siteIndexes = null)
        {
            double px = point.X;
            double py = point.Y;
            double bestWeightedDistance = double.PositiveInfinity;
            int bestWeightedIndex = -1;
            double bestInfiniteDistance = double.PositiveInfinity;
            int bestInfiniteIndex = -1;

            int count = siteIndexes?.Count ?? _sites.Length;
            for (int candidateIndex = 0; candidateIndex < count; candidateIndex++)
            {
                int i = siteIndexes == null ? candidateIndex : siteIndexes[candidateIndex];
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
