using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
#pragma warning disable RS0026 // Full and partial overloads intentionally share an optional random generator.
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
        /// Target areas are the initial participating areas at the start of this call, separately within each region.
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
        /// Each explicit value applies independently in every region where the cell is initially present.
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
    }
#pragma warning restore RS0026
}
