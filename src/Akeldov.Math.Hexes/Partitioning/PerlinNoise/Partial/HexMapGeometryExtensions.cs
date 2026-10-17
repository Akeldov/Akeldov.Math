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

        /// <summary>Partitions hexes with non-null region identifiers into Perlin-noise bands independently within each region.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares, applied to every region.
        /// At least one share must be positive. List order determines bands from low to high noise.
        /// Zero-share targets remain empty.
        /// </param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the geometry. Null excludes a hex.
        /// Equal values identify the same region, including disconnected hexes; zero and negative values are valid.
        /// </param>
        /// <param name="parameters">The immutable noise generator settings, including seed and world-space scale.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller, retaining the geometry.
        /// Excluded hexes contain null identifiers. New read-only cell snapshots retain all target identifiers
        /// in input order, including empty territories, with hex indexes in row-major order.
        /// Each target's cell combines its hexes from all regions. The mask is neither modified nor retained.
        /// </returns>
        /// <remarks>
        /// Noise is generated once over the whole geometry. Quantile thresholds and area shares are calculated
        /// separately for each region using only its participating hexes. Cumulative targets are rounded to
        /// the nearest attainable count, choosing the smaller count on a tie. Equal noise values within a region
        /// are never split between territories. Disconnected territories and holes are allowed.
        /// An empty mask or topology retains every target as an empty cell. Geometry and targets are still validated.
        /// A single region gives the same result as the Boolean-mask overload with the same participating hexes.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The target list, regions mask, or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// The mask topology differs from the geometry topology, or the target list is empty,
        /// contains a null or duplicate identifier, or has no positive share.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The geometry is invalid or a sampled coordinate exceeds the supported noise lattice.</exception>
        public static PartialPerlinNoiseHexPartitionMap ToPerlinNoiseHexPartitionMap(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            IHexMap<int?> regionsMask,
            PerlinNoisePartitionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != geometry.Topology)
                throw new ArgumentException("Geometry and regions mask must have the same topology.", nameof(regionsMask));

            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var participationFlags = new bool[geometry.Topology.Count];
            var regions = new int[geometry.Topology.Count];
            for (int i = 0; i < regions.Length; i++)
            {
                int? region = regionsMask[i];
                participationFlags[i] = region.HasValue;
                regions[i] = region.GetValueOrDefault();
            }

            var noise = geometry.CreatePerlinNoise(
                parameters.Seed, parameters.Scale, parameters.Octaves,
                parameters.Persistence, parameters.Lacunarity, parameters.Offset);
            return new PartialPerlinNoiseHexPartitionMap(geometry, partitioner.Partition(noise, participationFlags, regions));
        }

        /// <summary>Partitions participating hexes into Perlin-noise bands independently within each mask region.</summary>
        /// <param name="geometry">The geometry whose world-space hex centers are sampled.</param>
        /// <param name="targets">
        /// A non-empty ordered list of unique territory identifiers and relative area shares, applied to every region.
        /// At least one share must be positive. List order determines bands from low to high noise.
        /// Zero-share targets remain empty.
        /// </param>
        /// <param name="participationMask">A Boolean map with the same topology as the geometry. Only true hexes participate.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the geometry. Equal values identify the same region,
        /// including disconnected hexes. All integer values, including zero and negative values, are valid.
        /// Region values at excluded hexes have no effect on the result.
        /// </param>
        /// <param name="parameters">The immutable noise generator settings, including seed and world-space scale.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller, retaining the geometry.
        /// Excluded hexes contain null identifiers. New read-only cell snapshots retain all target identifiers
        /// in input order, including empty territories, with hex indexes in row-major order.
        /// Each target's cell combines its hexes from all regions. Neither mask is modified or retained.
        /// </returns>
        /// <remarks>
        /// Noise is generated once over the whole geometry. Quantile thresholds and area shares are calculated
        /// separately for each region using only its participating hexes. Cumulative targets are rounded to
        /// the nearest attainable count, choosing the smaller count on a tie. Equal noise values within a region
        /// are never split between territories. Disconnected territories and holes are allowed.
        /// An empty mask or topology retains every target as an empty cell. Geometry and targets are still validated.
        /// This is equivalent to a nullable region mask containing null wherever participation is false.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The target list, either mask, or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs from the geometry topology, or the target list is empty,
        /// contains a null or duplicate identifier, or has no positive share.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The geometry is invalid or a sampled coordinate exceeds the supported noise lattice.</exception>
        public static PartialPerlinNoiseHexPartitionMap ToPerlinNoiseHexPartitionMap(
            this HexMapGeometry geometry,
            IReadOnlyList<PerlinNoisePartitionTarget> targets,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask,
            PerlinNoisePartitionParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != geometry.Topology)
                throw new ArgumentException("Geometry and participation mask must have the same topology.", nameof(participationMask));
            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != geometry.Topology)
                throw new ArgumentException("Geometry and regions mask must have the same topology.", nameof(regionsMask));

            var partitioner = new PerlinNoiseHexPartitioner(targets);
            var participationFlags = new bool[geometry.Topology.Count];
            var regions = new int[geometry.Topology.Count];
            for (int i = 0; i < regions.Length; i++)
            {
                participationFlags[i] = participationMask[i];
                if (participationFlags[i])
                    regions[i] = regionsMask[i];
            }

            var noise = geometry.CreatePerlinNoise(
                parameters.Seed, parameters.Scale, parameters.Octaves,
                parameters.Persistence, parameters.Lacunarity, parameters.Offset);
            return new PartialPerlinNoiseHexPartitionMap(geometry, partitioner.Partition(noise, participationFlags, regions));
        }
    }
}
