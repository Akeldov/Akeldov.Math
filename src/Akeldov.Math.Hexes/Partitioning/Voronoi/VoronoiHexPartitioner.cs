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
    #pragma warning disable RS0026 // Mask overloads intentionally expose the same optional exclave policy.
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

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite, _sites);

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
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
        /// <returns>
        /// A new read-only masked hex partition map with nullable per-hex cell identifiers and a semantic cell list.
        /// Excluded hexes have no assignment and return <see langword="null"/> from the result map.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// </exception>
        public MaskedVoronoiHexPartitionMap Partition(
            HexCenterMap hexCenters,
            IHexMap<bool> participationMask,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));

            if (hexCenters.Topology != participationMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(participationMask));

            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

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

            var partitionSites = _sites;
            if (exclavePolicy == ExclavePolicy.ReassignToClosestCell)
                partitionSites = ReassignExclaves(hexCenters, cellIndexes, ref hexIndexBuckets, participationMaskValues, null);

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite, partitionSites);

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
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
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
            IHexMap<int> regionsMask,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));

            if (hexCenters.Topology != regionsMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(regionsMask));

            var siteIndexesByRegion = GroupSiteIndexesByRegion(hexCenters.Geometry, regionsMask);
            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

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

            var partitionSites = _sites;
            if (exclavePolicy == ExclavePolicy.ReassignToClosestCell)
                partitionSites = ReassignExclaves(hexCenters, cellIndexes, ref hexIndexBuckets, null, regionsMask);

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite, partitionSites);

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
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
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
            IHexMap<int> regionsMask,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
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
            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

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

            var partitionSites = _sites;
            if (exclavePolicy == ExclavePolicy.ReassignToClosestCell)
                partitionSites = ReassignExclaves(hexCenters, cellIndexes, ref hexIndexBuckets, participationMaskValues, regionsMask);

            var cells = CreateCells(hexIndexBuckets, out var cellIndexesBySite, partitionSites);

            var assignments = new int?[hexCenters.Topology.Count];
            for (int i = 0; i < assignments.Length; i++)
                if (participationMaskValues[i])
                    assignments[i] = cellIndexesBySite[cellIndexes[i]];

            return new MaskedVoronoiHexPartitionMap(hexCenters, assignments, cells, participationMaskValues);
        }

        private Site[] ReassignExclaves(
            HexCenterMap centers,
            int[] cellIndexes,
            ref List<VectorXYInt>[] buckets,
            bool[]? participation,
            IHexMap<int>? regions)
        {
            var queue = new int[cellIndexes.Length];
            var (components, mainComponents) = FindCellComponents(centers, cellIndexes, participation, queue);
            bool[] settled = GrowMainComponents(centers.Topology, centers, cellIndexes, components, mainComponents, regions, queue);
            Site[] sites = CreateExclaveSites(centers, cellIndexes, components, settled, ref buckets);

            // Rebuild in row-major order before applying the empty-cell policy and compacting IDs.
            for (int i = 0; i < buckets.Length; i++)
                buckets[i].Clear();

            int width = centers.Topology.Resolution.X;
            for (int i = 0; i < cellIndexes.Length; i++)
                if (components[i] != 0)
                    buckets[cellIndexes[i]].Add(new VectorXYInt(i % width, i / width));

            return sites;
        }

        private (int[] Components, int[] MainComponents) FindCellComponents(
            HexCenterMap centers, int[] cellIndexes, bool[]? participation, int[] queue)
        {
            HexMapTopology topology = centers.Topology;
            var components = new int[cellIndexes.Length];
            var mainComponents = new int[_sites.Length];
            var closestDistances = new double[_sites.Length];
            for (int i = 0; i < closestDistances.Length; i++)
                closestDistances[i] = double.PositiveInfinity;

            int component = 0;
            for (int seed = 0; seed < cellIndexes.Length; seed++)
            {
                if (components[seed] != 0 || (participation != null && !participation[seed]))
                    continue;

                component++;
                int siteIndex = cellIndexes[seed];
                int head = 0;
                int tail = 0;
                components[seed] = component;
                queue[tail++] = seed;
                while (head < tail)
                {
                    int current = queue[head++];
                    double distance = GetDistanceSquared(centers[current], _sites[siteIndex].Position);
                    if (distance < closestDistances[siteIndex])
                    {
                        closestDistances[siteIndex] = distance;
                        mainComponents[siteIndex] = component;
                    }

                    for (int direction = 0; direction < 6; direction++)
                    {
                        if (!TryGetAdjacentFlatIndex(current, direction, topology, out int neighbor) ||
                            components[neighbor] != 0 || cellIndexes[neighbor] != siteIndex ||
                            (participation != null && !participation[neighbor]))
                            continue;

                        components[neighbor] = component;
                        queue[tail++] = neighbor;
                    }
                }
            }

            return (components, mainComponents);
        }

        private bool[] GrowMainComponents(
            HexMapTopology topology, HexCenterMap centers, int[] cellIndexes,
            int[] components, int[] mainComponents, IHexMap<int>? regions, int[] queue)
        {
            // Grow only from retained components so neighboring exclaves cannot exchange owners
            // or create another disconnected component. Each layer is committed simultaneously.
            var settled = new bool[cellIndexes.Length];
            var queued = new bool[cellIndexes.Length];
            int frontierHead = 0;
            int frontierTail = 0;
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                if (components[i] == 0 || components[i] != mainComponents[cellIndexes[i]])
                    continue;

                settled[i] = true;
                queued[i] = true;
                queue[frontierTail++] = i;
            }

            while (frontierHead < frontierTail)
            {
                int layerEnd = frontierTail;
                while (frontierHead < layerEnd)
                {
                    int current = queue[frontierHead++];
                    for (int direction = 0; direction < 6; direction++)
                    {
                        if (!TryGetAdjacentFlatIndex(current, direction, topology, out int neighbor) ||
                            queued[neighbor] || components[neighbor] == 0 ||
                            (regions != null && regions[current] != regions[neighbor]))
                            continue;

                        queued[neighbor] = true;
                        queue[frontierTail++] = neighbor;
                    }
                }

                for (int i = layerEnd; i < frontierTail; i++)
                {
                    int current = queue[i];
                    cellIndexes[current] = GetClosestAdjacentCell(current, topology, centers, cellIndexes, settled, regions);
                }

                for (int i = layerEnd; i < frontierTail; i++)
                    settled[queue[i]] = true;
            }

            return settled;
        }

        private int GetClosestAdjacentCell(
            int current, HexMapTopology topology, HexCenterMap centers,
            int[] cellIndexes, bool[] settled, IHexMap<int>? regions)
        {
            int bestSite = -1;
            double bestDistance = double.PositiveInfinity;
            for (int direction = 0; direction < 6; direction++)
            {
                if (!TryGetAdjacentFlatIndex(current, direction, topology, out int neighbor) ||
                    !settled[neighbor] ||
                    (regions != null && regions[current] != regions[neighbor]))
                    continue;

                int siteIndex = cellIndexes[neighbor];
                double distance = GetDistanceSquared(centers[current], _sites[siteIndex].Position);
                if (distance < bestDistance || (distance == bestDistance && siteIndex < bestSite))
                {
                    bestDistance = distance;
                    bestSite = siteIndex;
                }
            }

            return bestSite;
        }

        private Site[] CreateExclaveSites(
            HexCenterMap centers, int[] cellIndexes, int[] components, bool[] settled,
            ref List<VectorXYInt>[] buckets)
        {
            // A component blocked by a mask becomes its own cell with a center inside it.
            var sites = new List<Site>(_sites);
            var expandedBuckets = new List<List<VectorXYInt>>(buckets);
            var newSitesByComponent = new Dictionary<int, int>();
            var newSiteDistances = new List<double>();
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                if (components[i] == 0 || settled[i])
                    continue;

                int originalSite = cellIndexes[i];
                double distance = GetDistanceSquared(centers[i], _sites[originalSite].Position);
                if (!newSitesByComponent.TryGetValue(components[i], out int newSite))
                {
                    newSite = sites.Count;
                    newSitesByComponent.Add(components[i], newSite);
                    sites.Add(new Site(centers[i], _sites[originalSite].Weight));
                    expandedBuckets.Add(new List<VectorXYInt>());
                    newSiteDistances.Add(distance);
                }
                else if (distance < newSiteDistances[newSite - _sites.Length])
                {
                    sites[newSite] = new Site(centers[i], _sites[originalSite].Weight);
                    newSiteDistances[newSite - _sites.Length] = distance;
                }

                cellIndexes[i] = newSite;
            }

            buckets = expandedBuckets.ToArray();

            return sites.ToArray();
        }

        private static bool TryGetAdjacentFlatIndex(int flatIndex, int direction, HexMapTopology topology, out int neighbor)
        {
            int width = topology.Resolution.X;
            var index = new VectorXYInt(flatIndex % width, flatIndex / width);
            VectorXYInt adjacent = index.GetAdjacent((HexEdge)direction, topology.Layout);
            if ((uint)adjacent.X >= (uint)width || (uint)adjacent.Y >= (uint)topology.Resolution.Y)
            {
                neighbor = -1;
                return false;
            }

            neighbor = adjacent.Y * width + adjacent.X;
            return true;
        }

        private static double GetDistanceSquared(PointXY point, PointXY site)
        {
            double dx = (double)point.X - site.X;
            double dy = (double)point.Y - site.Y;
            return dx * dx + dy * dy;
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

        private VoronoiCell[] CreateCells(List<VectorXYInt>[] hexIndexBuckets, out int[] cellIndexesBySite, Site[] sites)
        {
            var cells = new List<VoronoiCell>(sites.Length);
            cellIndexesBySite = new int[sites.Length];
            for (int i = 0; i < sites.Length; i++)
            {
                if (hexIndexBuckets[i].Count == 0)
                {
                    if (_emptyCellPolicy == EmptyCellPolicy.Exclude)
                        continue;

                    if (_emptyCellPolicy != EmptyCellPolicy.LeaveAsIs)
                        throw new InvalidOperationException($"Couldn't tessellate by empty cells, empty cell: {sites[i]}.");
                }

                cellIndexesBySite[i] = cells.Count;
                cells.Add(new VoronoiCell(cells.Count, sites[i], hexIndexBuckets[i]));
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
    #pragma warning restore RS0026
}
