using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
#pragma warning disable RS0026 // Target-area overloads intentionally share an optional random generator.
    /// <summary>Provides Cellular Potts evolution of partial hex partitions with optional connectivity preservation.</summary>
    public static class PartialHexPartitionMapExtensions
    {
        /// <summary>Evolves all assigned hexes while keeping unassigned hexes null.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Previously unassigned hexes remain null and never participate.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
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
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            return partition.ApplyCellularPotts(targetAreas: null, parameters, random);
        }

        /// <summary>Evolves all assigned hexes while keeping unassigned hexes null.</summary>
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
        /// A new mutable partial partition map owned by the caller. Previously unassigned hexes remain null and never participate.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source is neither changed nor retained.
        /// </returns>
        /// <remarks>
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
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
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
            return simulation.CreatePartialPartition();
        }

    }
#pragma warning restore RS0026
}
