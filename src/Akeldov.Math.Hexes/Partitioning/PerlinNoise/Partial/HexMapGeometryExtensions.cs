using Akeldov.Math.Hexes.Geometry;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.PerlinNoise
{
    /// <summary>Provides territory partitioning by thresholds of a single Perlin-noise field.</summary>
    public static partial class HexMapGeometryExtensions
    {
        /// <summary>Partitions participating hexes into ordered noise bands with approximately the requested area shares.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares.
        /// At least one share must be positive. List order determines bands from low to high noise.
        /// Zero-share targets remain empty.
        /// </param>
        /// <param name="participationMask">A Boolean map with the same topology as the geometry. Only true hexes participate.</param>
        /// <param name="parameters">The immutable noise generator settings, including seed and world-space scale.</param>
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
        /// <exception cref="ArgumentNullException">The target list, participation mask, or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// The mask topology differs from the geometry topology, or the target list is empty,
        /// contains a null or duplicate identifier, or has no positive share.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The geometry is invalid or a sampled coordinate exceeds the supported noise lattice.
        /// </exception>
        public static PartialPerlinNoiseHexPartitionMap ToPerlinNoiseHexPartitionMap(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            IHexMap<bool> participationMask,
            PerlinNoisePartitionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != geometry.Topology)
                throw new ArgumentException("Geometry and participation mask must have the same topology.", nameof(participationMask));

            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var participationFlags = new bool[geometry.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
                participationFlags[i] = participationMask[i];

            var noise = geometry.CreatePerlinNoise(
                parameters.Seed, parameters.Scale, parameters.Octaves,
                parameters.Persistence, parameters.Lacunarity, parameters.Offset);
            return new PartialPerlinNoiseHexPartitionMap(geometry, partitioner.Partition(noise, participationFlags));
        }
    }
}
