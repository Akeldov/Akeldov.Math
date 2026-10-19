using Akeldov.Math.Hexes.Geometry;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Provides territory partitioning by thresholds of a single Perlin-noise field.</summary>
    public static partial class HexMapGeometryExtensions
    {
        /// <summary>Partitions a geometry into ordered noise bands with approximately the requested area shares.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares.
        /// At least one share must be positive. List order determines the bands from low to high noise;
        /// it does not have to match identifier order. Zero-share targets remain empty.
        /// </param>
        /// <param name="parameters">The immutable noise generator settings, including world-space scale.</param>
        /// <param name="seed">The deterministic noise generator seed.</param>
        /// <returns>
        /// A new mutable spatial partition map owned by the caller that retains the geometry. Each hex belongs to exactly one territory type.
        /// New cell snapshots retain all identifiers in input order, including empty territories;
        /// their hex indexes are in row-major order.
        /// </returns>
        /// <remarks>
        /// Indexer setters change only the identifier map, not the retained cells or their hex indexes.
        /// Samples the existing CreatePerlinNoise generator once, then chooses quantile thresholds
        /// from this map's values. Each cumulative area target is rounded to the nearest attainable
        /// cumulative hex count, choosing the smaller count on a tie. Equal noise values are never
        /// split across territories. Thus shares are approximate, especially on small or constant
        /// fields, and positive-share territories may be empty. Disconnected territories and holes
        /// are allowed; each identifier represents a territory type and may cover several components.
        /// Thresholds depend on the sampled map, so independently partitioned neighboring chunks
        /// need not have matching bands even when their underlying noise fields are continuous.
        /// Empty topologies retain every target as an empty cell. All noise parameters are validated
        /// even for empty topologies or a single territory. Cost is O(N log N + K log N + N log K)
        /// for N hexes and K territories, in addition to noise generation.
        /// </remarks>
        /// <example>
        /// <code>
        /// var partition = geometry.PartitionByPerlinNoise(
        ///     new[]
        ///     {
        ///         new PerlinNoisePartitionTarget(7, 0.2),
        ///         new PerlinNoisePartitionTarget(42, 0.5),
        ///         new PerlinNoisePartitionTarget(99, 0.3)
        ///     },
        ///     new PerlinNoisePartitionParameters(scale: 12f), seed: 42);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">The target list or parameters are null.</exception>
        /// <exception cref="ArgumentException">The target list is empty, contains a null or duplicate identifier, or has no positive share.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The geometry is invalid or a sampled coordinate exceeds the supported noise lattice.</exception>
        public static SpatialHexPartitionMap<HexPartitionCell> PartitionByPerlinNoise(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            PerlinNoisePartitionParameters parameters,
            int seed)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var noise = geometry.CreatePerlinNoise(
                seed, parameters.Scale, parameters.Octaves,
                parameters.Persistence, parameters.Lacunarity, parameters.Offset);
            return new SpatialHexPartitionMap<HexPartitionCell>(geometry, partitioner.Partition(noise));
        }
    }
}
