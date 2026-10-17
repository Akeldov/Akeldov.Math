using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
#pragma warning disable RS0026 // Full and masked overloads intentionally share optional noise parameters.
    /// <summary>Provides territory partitioning by thresholds of a single Perlin-noise field.</summary>
    public static class HexMapGeometryExtensions
    {
        /// <summary>Partitions a geometry into ordered noise bands with approximately the requested area shares.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares.
        /// At least one share must be positive. List order determines the bands from low to high noise;
        /// it does not have to match identifier order. Zero-share targets remain empty.
        /// </param>
        /// <param name="seed">The deterministic seed used by the Perlin-noise generator.</param>
        /// <param name="scale">The base noise feature scale in world-space units. Larger values produce broader features.</param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">The finite amplitude multiplier between octaves, in the inclusive range [0, 1].</param>
        /// <param name="lacunarity">The finite, positive frequency multiplier between octaves.</param>
        /// <param name="offset">The finite sample-space offset in world-space units.</param>
        /// <returns>
        /// A new mutable partition map owned by the caller that retains the geometry. Each hex belongs to exactly one territory type.
        /// New cell snapshots retain all identifiers in input order, including empty territories;
        /// their hex indexes are in row-major order.
        /// </returns>
        /// <remarks>
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
        /// var partition = geometry.ToPerlinNoiseHexPartitionMap(
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
        /// <exception cref="ArgumentOutOfRangeException">The geometry is invalid, a noise parameter is outside its documented range, or a sampled coordinate exceeds the supported noise lattice.</exception>
        public static PerlinNoiseHexPartitionMap ToPerlinNoiseHexPartitionMap(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var noise = geometry.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);
            return new PerlinNoiseHexPartitionMap(geometry, partitioner.Partition(noise));
        }

        /// <summary>Partitions participating hexes into ordered noise bands with approximately the requested area shares.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares.
        /// At least one share must be positive. List order determines bands from low to high noise.
        /// Zero-share targets remain empty.
        /// </param>
        /// <param name="participationMask">A Boolean map with the same topology as the geometry. Only true hexes participate.</param>
        /// <param name="seed">The deterministic seed used by the Perlin-noise generator.</param>
        /// <param name="scale">The base noise feature scale in world-space units. Larger values produce broader features.</param>
        /// <param name="octaves">The positive number of noise layers to combine.</param>
        /// <param name="persistence">The finite amplitude multiplier between octaves, in the inclusive range [0, 1].</param>
        /// <param name="lacunarity">The finite, positive frequency multiplier between octaves.</param>
        /// <param name="offset">The finite sample-space offset in world-space units.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller, retaining the geometry.
        /// Participating hexes belong to exactly one territory; excluded hexes contain null identifiers.
        /// New read-only cell snapshots retain all target identifiers in input order, including empty
        /// territories, with hex indexes in row-major order. The input mask is neither modified nor retained.
        /// </returns>
        /// <remarks>
        /// Noise is generated over the whole geometry, but quantile thresholds and area shares use only
        /// participating hexes. Each cumulative area target is rounded to the nearest attainable count,
        /// choosing the smaller count on a tie. Equal noise values are never split between territories.
        /// Disconnected territories and holes are allowed. An empty mask or topology retains all targets
        /// as empty cells. Geometry, targets, and noise parameters are validated even with no participants.
        /// A mask containing only true values produces the same assignments as the full overload.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The target list or participation mask is null.</exception>
        /// <exception cref="ArgumentException">
        /// The mask topology differs from the geometry topology, or the target list is empty,
        /// contains a null or duplicate identifier, or has no positive share.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The geometry is invalid, a noise parameter is outside its documented range,
        /// or a sampled coordinate exceeds the supported noise lattice.
        /// </exception>
        public static PartialPerlinNoiseHexPartitionMap ToPerlinNoiseHexPartitionMap(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            IHexMap<bool> participationMask,
            int seed,
            float scale,
            int octaves = 4,
            float persistence = 0.5f,
            float lacunarity = 2f,
            VectorXY offset = default)
        {
            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != geometry.Topology)
                throw new ArgumentException("Geometry and participation mask must have the same topology.", nameof(participationMask));

            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var participationFlags = new bool[geometry.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
                participationFlags[i] = participationMask[i];

            var noise = geometry.CreatePerlinNoise(seed, scale, octaves, persistence, lacunarity, offset);
            return new PartialPerlinNoiseHexPartitionMap(geometry, partitioner.Partition(noise, participationFlags));
        }
    }
#pragma warning restore RS0026
}
