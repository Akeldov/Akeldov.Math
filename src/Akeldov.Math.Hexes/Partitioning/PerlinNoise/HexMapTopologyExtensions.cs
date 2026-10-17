using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Provides territory partitioning by thresholds of a single Perlin-noise field.</summary>
    public static class HexMapTopologyExtensions
    {
        /// <summary>Partitions a topology into ordered noise bands with approximately the requested area shares.</summary>
        /// <param name="topology">The topology to cover completely.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares.
        /// At least one share must be positive. List order determines the bands from low to high noise;
        /// it does not have to match identifier order. Zero-share targets remain empty.
        /// </param>
        /// <param name="seed">The deterministic seed used by the Perlin-noise generator.</param>
        /// <param name="scale">The base noise feature scale in unit-hex-radius coordinate-space units. Larger values produce broader features.</param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">The finite amplitude multiplier between octaves, in the inclusive range [0, 1].</param>
        /// <param name="lacunarity">The finite, positive frequency multiplier between octaves.</param>
        /// <param name="offset">The finite sample-space offset in unit-hex-radius coordinate-space units.</param>
        /// <returns>
        /// A new mutable partition map owned by the caller. Each hex belongs to exactly one territory.
        /// New cell snapshots retain all identifiers in input order, including empty territories;
        /// their hex indexes are in row-major order.
        /// </returns>
        /// <remarks>
        /// Samples the existing CreatePerlinNoise generator once, then chooses quantile thresholds
        /// from this map's values. Each cumulative area target is rounded to the nearest attainable
        /// cumulative hex count, choosing the smaller count on a tie. Equal noise values are never
        /// split across territories. Thus shares are approximate, especially on small or constant
        /// fields, and positive-share territories may be empty. Disconnected territories and holes
        /// are allowed; no seeds or connectivity repair are used.
        /// Thresholds depend on the sampled map, so independently partitioned neighboring chunks
        /// need not have matching bands even when their underlying noise fields are continuous.
        /// Empty topologies retain every target as an empty cell. All noise parameters are validated
        /// even for empty topologies or a single territory. Cost is O(N log N + K log N + N log K)
        /// for N hexes and K territories, in addition to noise generation.
        /// </remarks>
        /// <example>
        /// <code>
        /// var partition = topology.ToPerlinNoiseHexPartitionMap(
        ///     new[]
        ///     {
        ///         new PerlinNoisePartitionTarget(7, 0.2),
        ///         new PerlinNoisePartitionTarget(42, 0.5),
        ///         new PerlinNoisePartitionTarget(99, 0.3)
        ///     },
        ///     seed: 42, scale: 12f);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">The target list is null.</exception>
        /// <exception cref="ArgumentException">The target list is empty, contains a null or duplicate identifier, or has no positive share.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A noise parameter is outside its documented range.</exception>
        public static HexPartitionMap<HexPartitionCell> ToPerlinNoiseHexPartitionMap(
            this HexMapTopology topology,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));
            if (targets.Count == 0)
                throw new ArgumentException("At least one territory target is required.", nameof(targets));

            var snapshot = new PerlinNoisePartitionTarget[targets.Count];
            var ids = new HashSet<int>();
            double maximumShare = 0d;
            for (int i = 0; i < snapshot.Length; i++)
            {
                var target = targets[i];
                if (target == null)
                    throw new ArgumentException("Territory targets must be non-null.", nameof(targets));
                if (!ids.Add(target.Id))
                    throw new ArgumentException($"Duplicate territory identifier: {target.Id}.", nameof(targets));
                snapshot[i] = target;
                maximumShare = System.Math.Max(maximumShare, target.AreaShare);
            }
            if (maximumShare == 0d)
                throw new ArgumentException("At least one area share must be positive.", nameof(targets));

            var noise = topology.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);
            var thresholds = CreateThresholds(noise, snapshot, maximumShare);
            var groups = new List<VectorXYInt>[snapshot.Length];
            for (int i = 0; i < groups.Length; i++)
                groups[i] = new List<VectorXYInt>();

            int width = topology.Resolution.X;
            for (int site = 0; site < topology.Count; site++)
            {
                int lower = 0;
                int upper = thresholds.Length;
                while (lower < upper)
                {
                    int middle = lower + (upper - lower) / 2;
                    if (noise[site] >= thresholds[middle])
                        lower = middle + 1;
                    else
                        upper = middle;
                }
                groups[lower].Add(new VectorXYInt(site % width, site / width));
            }

            var cells = new HexPartitionCell[snapshot.Length];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = new HexPartitionCell(snapshot[i].Id, groups[i]);
            return new HexPartitionMap<HexPartitionCell>(topology, cells);
        }

        private static float[] CreateThresholds(FloatHexMap noise, PerlinNoisePartitionTarget[] targets, double maximumShare)
        {
            var thresholds = new float[targets.Length - 1];
            int count = noise.Topology.Count;
            if (count == 0 || thresholds.Length == 0)
                return thresholds;

            var sorted = new float[count];
            for (int site = 0; site < count; site++)
                sorted[site] = noise[site];
            Array.Sort(sorted);

            // Boundaries may fall only between runs of equal values, never inside a run.
            var boundaries = new List<int> { 0 };
            for (int i = 1; i < count; i++)
                if (sorted[i] != sorted[i - 1])
                    boundaries.Add(i);
            boundaries.Add(count);

            // Scaling first avoids overflow when several finite shares are near double.MaxValue.
            double total = 0d;
            foreach (var target in targets)
                total += target.AreaShare / maximumShare;

            double cumulative = 0d;
            for (int i = 0; i < thresholds.Length; i++)
            {
                cumulative += targets[i].AreaShare / maximumShare;
                double desiredCount = System.Math.Min(count, cumulative / total * count);
                int upper = boundaries.BinarySearch((int)System.Math.Ceiling(desiredCount));
                if (upper < 0)
                    upper = ~upper;
                int boundary = boundaries[upper];
                if (upper > 0 && desiredCount - boundaries[upper - 1] <= boundary - desiredCount)
                    boundary = boundaries[upper - 1];
                thresholds[i] = boundary == count ? float.PositiveInfinity : sorted[boundary];
            }
            return thresholds;
        }
    }
}
