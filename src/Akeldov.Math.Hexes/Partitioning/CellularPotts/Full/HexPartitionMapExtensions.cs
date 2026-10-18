using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
#pragma warning disable RS0026 // Region and target-area overloads intentionally share an optional random generator.
    /// <summary>Provides Cellular Potts evolution of hex partitions with optional connectivity preservation.</summary>
    public static partial class HexPartitionMapExtensions
    {
        /// <summary>Applies the Cellular Potts model with the specified cell connectivity policy.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partition map owned by the caller, with new cell snapshots consistent with its assignments.
        /// Cell identifiers and order, including empty cells, are preserved; source-specific cell metadata is not copied.
        /// The source map and its cells are unchanged.
        /// </returns>
        /// <remarks>
        /// Target areas are the initial cell areas at the start of this call.
        /// In both modes, each attempt selects a uniformly random map hex and then a uniformly random
        /// edge-adjacent hex inside the map. The first hex may copy the second hex's identifier.
        /// Repeated neighboring identifiers retain their multiplicity; choosing the current identifier
        /// leaves the map unchanged. Energy consists of unlike shared edges and squared area deviations.
        /// With <see cref="CellularPottsConnectivity.Preserve"/>, initially non-empty cells must be
        /// six-connected and cannot disappear or fragment. Each transfer checks only that the donor's
        /// remaining neighbors form one connected run in the six-hex ring. This constant-time check
        /// conservatively rejects moves whose only connecting paths leave the ring.
        /// The receiver stays connected through the sampled neighbor. Holes are allowed; the local check
        /// can reject globally safe moves around them.
        /// With <see cref="CellularPottsConnectivity.None"/>, no connectivity checks are made:
        /// disconnected input, fragmentation, holes, and cell disappearance are allowed.
        /// Metropolis acceptance uses min(1, exp(-delta energy / temperature)).
        /// Empty cells remain empty in either mode; vanished cells cannot reappear.
        /// Map edges are closed: outside hexes do not participate, there is no wrapping or exterior energy.
        /// Rejected and unchanged attempts still count toward the step duration.
        /// </remarks>
        /// <example>
        /// <code>
        /// var evolved = partition.ApplyCellularPotts(
        ///     new CellularPottsParameters(stepCount: 100, temperature: 5, boundaryEnergy: 2, areaPenalty: 1),
        ///     random: new Random(42));
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">The partition or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// Source cells or assignments are inconsistent, or a non-empty cell is disconnected in Preserve mode.
        /// </exception>
        public static HexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            return partition.ApplyCellularPotts(targetAreas: null, parameters, random);
        }

        /// <summary>Applies the Cellular Potts model with the specified cell connectivity policy.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
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
        /// A new mutable partition map owned by the caller, with new cell snapshots consistent with its assignments.
        /// Cell identifiers and order, including empty cells, are preserved; source-specific cell metadata is not copied.
        /// The source map and its cells are unchanged.
        /// </returns>
        /// <remarks>
        /// In both modes, each attempt selects a uniformly random map hex and then a uniformly random
        /// edge-adjacent hex inside the map. The first hex may copy the second hex's identifier.
        /// Repeated neighboring identifiers retain their multiplicity; choosing the current identifier
        /// leaves the map unchanged. Energy consists of unlike shared edges and squared area deviations.
        /// With <see cref="CellularPottsConnectivity.Preserve"/>, initially non-empty cells must be
        /// six-connected and cannot disappear or fragment. Each transfer checks only that the donor's
        /// remaining neighbors form one connected run in the six-hex ring. This constant-time check
        /// conservatively rejects moves whose only connecting paths leave the ring.
        /// The receiver stays connected through the sampled neighbor. Holes are allowed; the local check
        /// can reject globally safe moves around them.
        /// With <see cref="CellularPottsConnectivity.None"/>, no connectivity checks are made:
        /// disconnected input, fragmentation, holes, and cell disappearance are allowed.
        /// Metropolis acceptance uses min(1, exp(-delta energy / temperature)).
        /// Empty cells remain empty in either mode; vanished cells cannot reappear.
        /// Map edges are closed: outside hexes do not participate, there is no wrapping or exterior energy.
        /// Rejected and unchanged attempts still count toward the step duration.
        /// </remarks>
        /// <example>
        /// <code>
        /// var evolved = partition.ApplyCellularPotts(
        ///     new Dictionary&lt;int, float&gt; { [7] = 100f, [42] = 250f },
        ///     new CellularPottsParameters(stepCount: 100, temperature: 5, boundaryEnergy: 2, areaPenalty: 1),
        ///     random: new Random(42));
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">The partition or parameters are null.</exception>
        /// <exception cref="ArgumentException">
        /// Source cells or assignments are inconsistent, a non-empty cell is disconnected in Preserve mode,
        /// or an explicit target identifier is negative or absent from the partition.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">A target area is non-finite, negative, or greater than int.MaxValue.</exception>
        public static HexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            IReadOnlyDictionary<int, float>? targetAreas,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var simulation = CreateSimulation(partition, partition.Cells, id => id, targetAreas, parameters);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartition();
        }

        /// <summary>Evolves assigned hexes while forbidding copies between different regions.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <param name="regions">
        /// Region identifiers with the same topology as the partition. A hex may copy a neighbor's cell identifier
        /// only when both hexes have the same region identifier. All integer identifiers are valid, including zero
        /// and negative values. The map is copied before the first random draw and is neither modified nor retained.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partition map owned by the caller, with assignments consistent with its new cell snapshots.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Regions restrict copying only. Areas, initial target areas, boundary energy, and connectivity are evaluated
        /// across the entire source map, including edges across region boundaries. A cell may span multiple regions.
        /// Each step makes one attempt per assigned hex, sampled with replacement; a participating edge-adjacent
        /// neighbor is sampled uniformly. Attempts across region boundaries are rejected and still count toward the step.
        /// Preserve mode requires initially six-connected non-empty cells
        /// and prevents their disappearance or fragmentation. None mode permits disconnected cells and disappearance.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, regions, or parameters are null.</exception>
        /// <exception cref="ArgumentException">The region topology differs, source cells or assignments are inconsistent, or a non-empty cell is disconnected in Preserve mode.</exception>
        public static HexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            HexMap<int> regions,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            return partition.ApplyCellularPotts(regions, targetAreas: null, parameters, random);
        }

        /// <summary>Evolves assigned hexes with target areas while forbidding copies between different regions.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
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
        /// A new mutable partition map owned by the caller, with assignments consistent with its new cell snapshots.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Regions restrict copying only. Areas, boundary energy, and connectivity are evaluated across the entire
        /// source map, including edges across region boundaries. A cell may span multiple regions.
        /// Each step makes one attempt per assigned hex, sampled with replacement; a participating edge-adjacent
        /// neighbor is sampled uniformly. Attempts across region boundaries are rejected and still count toward the step.
        /// Preserve mode requires initially six-connected non-empty cells
        /// and prevents their disappearance or fragmentation. None mode permits disconnected cells and disappearance.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, regions, or parameters are null.</exception>
        /// <exception cref="ArgumentException">The region topology differs, source cells or assignments are inconsistent, a non-empty cell is disconnected in Preserve mode, or a target identifier is negative or absent from the source partition.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A target area is non-finite, negative, or greater than int.MaxValue.</exception>
        public static HexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
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

            var simulation = CreateSimulation(partition, partition.Cells, id => id, targetAreas, parameters, regionIds);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartition();
        }
    }
#pragma warning restore RS0026
}
