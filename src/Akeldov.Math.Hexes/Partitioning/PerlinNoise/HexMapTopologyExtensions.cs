using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Provides territory partitioning by thresholds of a single Perlin-noise field.</summary>
    public static class HexMapTopologyExtensions
    {
        /// <summary>
        /// Partitions a topology into ordered noise bands with approximately the requested area shares.
        /// The hex radius (center to vertex) is assumed to be 1.
        /// </summary>
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
        /// A topology has no spatial size or position. Sampling uses a hex radius of 1 and the layout's
        /// default zero-hex origin. Both <paramref name="scale"/> and <paramref name="offset"/> are
        /// expressed in this unit-radius coordinate system.
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
            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var noise = topology.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);
            return new HexPartitionMap<HexPartitionCell>(topology, partitioner.Partition(noise));
        }
    }
}
