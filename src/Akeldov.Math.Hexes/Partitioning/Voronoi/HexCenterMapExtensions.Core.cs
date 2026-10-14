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
    public static partial class HexCenterMapExtensions
    {
        private static (int[] Assignments, VoronoiHexPartitionCell[] Cells, bool[]? Participation) PartitionCore(
            HexCenterMap centers, Site[] sites, EmptyCellPolicy emptyCellPolicy,
            IHexMap<bool>? participationMask, IHexMap<int>? regionsMask,
            Dictionary<int, List<int>>? siteIndexesByRegion, ExclavePolicy exclavePolicy, int relaxationIterations)
        {
            var cellIndexes = new int[centers.Topology.Count];
            var participation = participationMask == null ? null : new bool[cellIndexes.Length];
            var buckets = CreateHexIndexBuckets(sites.Length);

            // Region membership remains fixed even when a centroid moves outside its original region.
            for (int remainingIterations = relaxationIterations; ; remainingIterations--)
            {
                AssignHexCenters(centers, participationMask, regionsMask, siteIndexesByRegion,
                    sites, cellIndexes, buckets, participation);
                if (remainingIterations == 0)
                    break;

                sites = CreateCentroidSites(centers, buckets, sites);
                for (int i = 0; i < buckets.Length; i++)
                    buckets[i].Clear();
            }

            if (exclavePolicy == ExclavePolicy.ReassignToClosestCell)
                sites = ReassignExclaves(centers, cellIndexes, ref buckets, participation, regionsMask, sites);

            var cells = CreateCells(buckets, out var cellIndexesBySite, sites, emptyCellPolicy);
            for (int i = 0; i < cellIndexes.Length; i++)
                if (participation == null || participation[i])
                    cellIndexes[i] = cellIndexesBySite[cellIndexes[i]];

            return (cellIndexes, cells, participation);
        }

        private static void AssignHexCenters(
            HexCenterMap hexCenters, IHexMap<bool>? participationMask, IHexMap<int>? regionsMask,
            Dictionary<int, List<int>>? siteIndexesByRegion, Site[] sites, int[] cellIndexes,
            List<VectorXYInt>[] buckets, bool[]? participation)
        {
            int flatIndex = 0;
            for (int y = 0; y < hexCenters.Topology.Resolution.Y; y++)
            {
                for (int x = 0; x < hexCenters.Topology.Resolution.X; x++)
                {
                    if (participationMask != null && !participationMask[flatIndex])
                    {
                        flatIndex++;
                        continue;
                    }

                    if (participation != null)
                        participation[flatIndex] = true;
                    PointXY center = hexCenters[flatIndex];
                    if (float.IsNaN(center.X) || float.IsInfinity(center.X) ||
                        float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                        throw new ArgumentOutOfRangeException(nameof(hexCenters), "Hex center coordinates must be finite.");

                    List<int>? candidates = null;
                    int region = regionsMask == null ? 0 : regionsMask[flatIndex];
                    if (siteIndexesByRegion != null && !siteIndexesByRegion.TryGetValue(region, out candidates))
                        throw new InvalidOperationException($"Region {region} contains no Voronoi sites.");

                    int cellIndex = GetNearestWeightedCellIndex(center, candidates, sites);
                    if (cellIndex < 0)
                        throw new InvalidOperationException($"Region {region} has no eligible Voronoi site for hex ({x}, {y}).");

                    cellIndexes[flatIndex] = cellIndex;
                    buckets[cellIndex].Add(new VectorXYInt(x, y));
                    flatIndex++;
                }
            }
        }

        private static Site[] CreateCentroidSites(HexCenterMap centers, List<VectorXYInt>[] buckets, Site[] sites)
        {
            var centroidSites = new Site[sites.Length];
            for (int i = 0; i < sites.Length; i++)
            {
                var bucket = buckets[i];
                if (bucket.Count == 0)
                {
                    centroidSites[i] = sites[i];
                    continue;
                }

                double x = 0d;
                double y = 0d;
                foreach (var index in bucket)
                {
                    var center = centers[index];
                    x += center.X;
                    y += center.Y;
                }

                centroidSites[i] = new Site(new PointXY((float)(x / bucket.Count), (float)(y / bucket.Count)), sites[i].Weight);
            }

            return centroidSites;
        }

        private static Site[] ReassignExclaves(
            HexCenterMap centers,
            int[] cellIndexes,
            ref List<VectorXYInt>[] buckets,
            bool[]? participation,
            IHexMap<int>? regions,
            Site[] sites)
        {
            var queue = new int[cellIndexes.Length];
            var (components, mainComponents) = FindCellComponents(centers, cellIndexes, participation, queue, sites);
            bool[] settled = GrowMainComponents(centers.Topology, centers, cellIndexes, components, mainComponents, regions, queue, sites);
            sites = CreateExclaveSites(centers, cellIndexes, components, settled, ref buckets, sites);

            // Rebuild in row-major order before applying the empty-cell policy and compacting IDs.
            for (int i = 0; i < buckets.Length; i++)
                buckets[i].Clear();

            int width = centers.Topology.Resolution.X;
            for (int i = 0; i < cellIndexes.Length; i++)
                if (components[i] != 0)
                    buckets[cellIndexes[i]].Add(new VectorXYInt(i % width, i / width));

            return sites;
        }

        private static (int[] Components, int[] MainComponents) FindCellComponents(
            HexCenterMap centers, int[] cellIndexes, bool[]? participation, int[] queue, Site[] sites)
        {
            HexMapTopology topology = centers.Topology;
            var components = new int[cellIndexes.Length];
            var mainComponents = new int[sites.Length];
            var closestDistances = new double[sites.Length];
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
                    double distance = GetDistanceSquared(centers[current], sites[siteIndex].Position);
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

        private static bool[] GrowMainComponents(
            HexMapTopology topology, HexCenterMap centers, int[] cellIndexes,
            int[] components, int[] mainComponents, IHexMap<int>? regions, int[] queue, Site[] sites)
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
                    cellIndexes[current] = GetClosestAdjacentCell(current, topology, centers, cellIndexes, settled, regions, sites);
                }

                for (int i = layerEnd; i < frontierTail; i++)
                    settled[queue[i]] = true;
            }

            return settled;
        }

        private static int GetClosestAdjacentCell(
            int current, HexMapTopology topology, HexCenterMap centers,
            int[] cellIndexes, bool[] settled, IHexMap<int>? regions, Site[] sites)
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
                double distance = GetDistanceSquared(centers[current], sites[siteIndex].Position);
                if (distance < bestDistance || (distance == bestDistance && siteIndex < bestSite))
                {
                    bestDistance = distance;
                    bestSite = siteIndex;
                }
            }

            return bestSite;
        }

        private static Site[] CreateExclaveSites(
            HexCenterMap centers, int[] cellIndexes, int[] components, bool[] settled,
            ref List<VectorXYInt>[] buckets, Site[] originalSites)
        {
            // A component blocked by a mask becomes its own cell with a center inside it.
            var sites = new List<Site>(originalSites);
            var expandedBuckets = new List<List<VectorXYInt>>(buckets);
            var newSitesByComponent = new Dictionary<int, int>();
            var newSiteDistances = new List<double>();
            for (int i = 0; i < cellIndexes.Length; i++)
            {
                if (components[i] == 0 || settled[i])
                    continue;

                int originalSite = cellIndexes[i];
                double distance = GetDistanceSquared(centers[i], originalSites[originalSite].Position);
                if (!newSitesByComponent.TryGetValue(components[i], out int newSite))
                {
                    newSite = sites.Count;
                    newSitesByComponent.Add(components[i], newSite);
                    sites.Add(new Site(centers[i], originalSites[originalSite].Weight));
                    expandedBuckets.Add(new List<VectorXYInt>());
                    newSiteDistances.Add(distance);
                }
                else if (distance < newSiteDistances[newSite - originalSites.Length])
                {
                    sites[newSite] = new Site(centers[i], originalSites[originalSite].Weight);
                    newSiteDistances[newSite - originalSites.Length] = distance;
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

        private static Dictionary<int, List<int>> GroupSiteIndexesByRegion(
            Site[] sites, HexMapGeometry geometry, IHexMap<int> regionsMask, BoolHexMap? participationMask = null)
        {
            var siteIndexesByRegion = new Dictionary<int, List<int>>();
            for (int i = 0; i < sites.Length; i++)
            {
                VectorXYInt index = sites[i].Position.ToXYIndex(geometry.Radius, geometry.Origin, geometry.Topology.Layout);
                if (index.X < 0 || index.X >= geometry.Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= geometry.Topology.Resolution.Y)
                    continue;

                if (!(participationMask is null) && !participationMask[index])
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

        private static VoronoiHexPartitionCell[] CreateCells(List<VectorXYInt>[] hexIndexBuckets, out int[] cellIndexesBySite, Site[] sites, EmptyCellPolicy emptyCellPolicy)
        {
            var cells = new List<VoronoiHexPartitionCell>(sites.Length);
            cellIndexesBySite = new int[sites.Length];
            for (int i = 0; i < sites.Length; i++)
            {
                if (hexIndexBuckets[i].Count == 0)
                {
                    if (emptyCellPolicy == EmptyCellPolicy.Exclude)
                        continue;

                    if (emptyCellPolicy != EmptyCellPolicy.LeaveAsIs)
                        throw new InvalidOperationException($"Couldn't tessellate by empty cells, empty cell: {sites[i]}.");
                }

                cellIndexesBySite[i] = cells.Count;
                cells.Add(new VoronoiHexPartitionCell(cells.Count, sites[i], hexIndexBuckets[i]));
            }

            return cells.ToArray();
        }

        private static int GetNearestWeightedCellIndex(PointXY point, List<int>? siteIndexes, Site[] sites)
        {
            double px = point.X;
            double py = point.Y;
            double bestWeightedDistance = double.PositiveInfinity;
            int bestWeightedIndex = -1;
            double bestInfiniteDistance = double.PositiveInfinity;
            int bestInfiniteIndex = -1;

            int count = siteIndexes?.Count ?? sites.Length;
            for (int candidateIndex = 0; candidateIndex < count; candidateIndex++)
            {
                int i = siteIndexes == null ? candidateIndex : siteIndexes[candidateIndex];
                ref readonly var site = ref sites[i];
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
