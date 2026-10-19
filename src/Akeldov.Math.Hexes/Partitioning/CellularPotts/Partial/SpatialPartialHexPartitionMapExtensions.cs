using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
#pragma warning disable RS0026 // Region and target-area overloads intentionally share an optional random generator.
    /// <summary>Provides Cellular Potts diffusion of partial spatial hex partitions with optional connectivity preservation.</summary>
    public static class SpatialPartialHexPartitionMapExtensions
    {
        /// <summary>Diffuses partition cells across assigned hexes while keeping unassigned hexes null.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial spatial partition map owned by the caller, preserving the source geometry. Previously unassigned hexes remain null and never participate.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Diffusion changes hex ownership through copy attempts governed by energy and the connectivity policy.
        /// Target areas are the initial assigned areas at the start of this call.
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes. Unassigned hexes contribute no edge energy.
        /// Areas count only participating hexes.
        /// In Preserve mode, each non-empty participating cell must initially be six-connected and cannot
        /// vanish or fragment. Disconnected cells cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// Source cells or assignments are inconsistent, or a participating cell is disconnected
        /// in Preserve mode.
        /// </exception>
        public static SpatialPartialHexPartitionMap<HexPartitionCell> DiffuseByCellularPotts<THexPartitionCell>(
            this SpatialPartialHexPartitionMap<THexPartitionCell> partition,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            return partition.DiffuseByCellularPotts(targetAreas: null, parameters, random);
        }

        /// <summary>Diffuses partition cells across assigned hexes while keeping unassigned hexes null.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="targetAreas">
        /// Desired participating hex counts keyed by source cell identifier, or null to use initial areas.
        /// Missing identifiers use their initial participating area at the start of this call.
        /// Values must be finite and between zero and int.MaxValue; fractional counts are allowed.
        /// Targets are soft energy preferences, not guaranteed final areas. The dictionary is neither modified
        /// nor retained; target values are captured before the first random draw.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial spatial partition map owned by the caller, preserving the source geometry. Previously unassigned hexes remain null and never participate.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Diffusion changes hex ownership through copy attempts governed by energy and the connectivity policy.
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes. Unassigned hexes contribute no edge energy.
        /// Areas count only participating hexes. Missing explicit target areas use each cell's initial participating area.
        /// In Preserve mode, each non-empty participating cell must initially be six-connected and cannot
        /// vanish or fragment. Disconnected cells cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// Source cells or assignments are inconsistent, or a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is negative or absent from the source partition.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">A target area is non-finite, negative, or greater than int.MaxValue.</exception>
        public static SpatialPartialHexPartitionMap<HexPartitionCell> DiffuseByCellularPotts<THexPartitionCell>(
            this SpatialPartialHexPartitionMap<THexPartitionCell> partition,
            IReadOnlyDictionary<int, float>? targetAreas,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var simulation = HexPartitionMapExtensions.CreateSimulation(partition, partition.Cells, id => id, targetAreas, parameters);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return new SpatialPartialHexPartitionMap<HexPartitionCell>(partition.Geometry, simulation.CreateCells());
        }

        /// <summary>Diffuses partition cells while forbidding copies between different regions.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Null hexes do not participate.</param>
        /// <param name="regions">
        /// Region identifiers with the same topology as the partition. A hex may copy a neighbor's cell identifier
        /// only when both hexes have the same region identifier. All integer identifiers are valid, including zero
        /// and negative values. The map is copied before the first random draw and is neither modified nor retained.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial spatial partition map owned by the caller, preserving the source geometry. Unassigned hexes remain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Diffusion changes hex ownership through copy attempts governed by energy and the connectivity policy.
        /// Regions restrict copying only. Areas, initial target areas, boundary energy, and connectivity are evaluated
        /// across the entire source map, including edges across region boundaries. A cell may span multiple regions.
        /// Each step makes one attempt per assigned hex, sampled with replacement; a participating edge-adjacent
        /// neighbor is sampled uniformly. Attempts across region boundaries are rejected and still count toward the step.
        /// Unassigned hexes contribute no edge energy. Preserve mode requires initially six-connected non-empty cells
        /// and prevents their disappearance or fragmentation. None mode permits disconnected cells and disappearance.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, regions, or parameters are null.</exception>
        /// <exception cref="ArgumentException">The region topology differs, source cells or assignments are inconsistent, or a non-empty cell is disconnected in Preserve mode.</exception>
        public static SpatialPartialHexPartitionMap<HexPartitionCell> DiffuseByCellularPotts<THexPartitionCell>(
            this SpatialPartialHexPartitionMap<THexPartitionCell> partition,
            HexMap<int> regions,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            return partition.DiffuseByCellularPotts(regions, targetAreas: null, parameters, random);
        }

        /// <summary>Diffuses partition cells with target areas while forbidding copies between different regions.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Null hexes do not participate.</param>
        /// <param name="regions">
        /// Region identifiers with the same topology as the partition. A hex may copy a neighbor's cell identifier
        /// only when both hexes have the same region identifier. All integer identifiers are valid, including zero
        /// and negative values. The map is copied before the first random draw and is neither modified nor retained.
        /// </param>
        /// <param name="targetAreas">
        /// Desired assigned hex counts keyed by source cell identifier, or null to use initial areas.
        /// Missing identifiers use their initial assigned area across the entire map, not separately in each region.
        /// Values must be finite and between zero and int.MaxValue; fractional counts are allowed.
        /// Targets are soft energy preferences, not guaranteed final areas. The dictionary is neither modified
        /// nor retained; target values are captured before the first random draw.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial spatial partition map owned by the caller, preserving the source geometry. Unassigned hexes remain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Diffusion changes hex ownership through copy attempts governed by energy and the connectivity policy.
        /// Regions restrict copying only. Areas, boundary energy, and connectivity are evaluated across the entire
        /// source map, including edges across region boundaries. A cell may span multiple regions.
        /// Each step makes one attempt per assigned hex, sampled with replacement; a participating edge-adjacent
        /// neighbor is sampled uniformly. Attempts across region boundaries are rejected and still count toward the step.
        /// Unassigned hexes contribute no edge energy. Preserve mode requires initially six-connected non-empty cells
        /// and prevents their disappearance or fragmentation. None mode permits disconnected cells and disappearance.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, regions, or parameters are null.</exception>
        /// <exception cref="ArgumentException">The region topology differs, source cells or assignments are inconsistent, a non-empty cell is disconnected in Preserve mode, or a target identifier is negative or absent from the source partition.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A target area is non-finite, negative, or greater than int.MaxValue.</exception>
        public static SpatialPartialHexPartitionMap<HexPartitionCell> DiffuseByCellularPotts<THexPartitionCell>(
            this SpatialPartialHexPartitionMap<THexPartitionCell> partition,
            HexMap<int> regions,
            IReadOnlyDictionary<int, float>? targetAreas,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (regions is null)
                throw new ArgumentNullException(nameof(regions));
            if (regions.Topology != partition.Topology)
                throw new ArgumentException("Partition and regions must have the same topology.", nameof(regions));

            var regionIds = new int[partition.Topology.Count];
            for (int i = 0; i < regionIds.Length; i++)
                regionIds[i] = regions[i];

            var simulation = HexPartitionMapExtensions.CreateSimulation(partition, partition.Cells, id => id, targetAreas, parameters, regionIds);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return new SpatialPartialHexPartitionMap<HexPartitionCell>(partition.Geometry, simulation.CreateCells());
        }
    }
#pragma warning restore RS0026
}
