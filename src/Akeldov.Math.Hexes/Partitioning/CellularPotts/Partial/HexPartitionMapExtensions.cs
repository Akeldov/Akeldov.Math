using System;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
#pragma warning disable RS0026 // Full and partial overloads intentionally share an optional random generator.
    /// <summary>Provides Cellular Potts evolution of hex partitions with optional connectivity preservation.</summary>
    public static partial class HexPartitionMapExtensions
    {
        /// <summary>Evolves assigned participating hexes while keeping excluded hexes unassigned.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <param name="participationMask">A Boolean map with the same topology. Only true hexes with source assignments participate.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes. Excluded hexes contribute no edge energy.
        /// Areas count only participating hexes. Missing explicit target areas use each cell's initial participating area.
        /// In Preserve mode, each non-empty participating cell must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            IHexMap<bool> participationMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (participationMask is null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(participationMask));

            var participationFlags = new bool[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
                participationFlags[i] = participationMask[i];

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes independently within each mask region.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the partition. Null excludes a hex. Equal values identify the same region,
        /// including disconnected hexes. All integer values, including zero and negative values, are valid.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes in the same region. Excluded hexes and region boundaries contribute no edge energy.
        /// Areas and connectivity are tracked separately for each cell in each region. Explicit target areas apply
        /// independently to every region where that cell is initially present; otherwise the initial participating
        /// area in that region is used. The returned cell combines its hexes from all regions.
        /// In Preserve mode, each non-empty participating cell within a region must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            IHexMap<int?> regionsMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (regionsMask is null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(regionsMask));

            var participationFlags = new bool[partition.Topology.Count];
            var regions = new int[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
            {
                int? region = regionsMask[i];
                participationFlags[i] = region.HasValue;
                regions[i] = region.GetValueOrDefault();
            }

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags, regions);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes independently within each mask region.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The full partition whose assignments must agree with its retained cells.</param>
        /// <param name="participationMask">A Boolean map with the same topology. Only true hexes with source assignments participate.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the partition. Equal values identify the same region,
        /// including disconnected hexes. All integer values, including zero and negative values, are valid.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes in the same region. Excluded hexes and region boundaries contribute no edge energy.
        /// Areas and connectivity are tracked separately for each cell in each region. Explicit target areas apply
        /// independently to every region where that cell is initially present; otherwise the initial participating
        /// area in that region is used. The returned cell combines its hexes from all regions.
        /// In Preserve mode, each non-empty participating cell within a region must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (participationMask is null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(participationMask));
            if (regionsMask is null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(regionsMask));

            var participationFlags = new bool[partition.Topology.Count];
            var regions = new int[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
            {
                participationFlags[i] = participationMask[i];
                if (participationFlags[i])
                    regions[i] = regionsMask[i];
            }

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags, regions);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes while keeping excluded hexes unassigned.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes. Excluded hexes contribute no edge energy.
        /// Areas count only participating hexes. Missing explicit target areas use each cell's initial participating area.
        /// In Preserve mode, each non-empty participating cell must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes while keeping excluded hexes unassigned.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="participationMask">A Boolean map with the same topology. Only true hexes with source assignments participate.</param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes. Excluded hexes contribute no edge energy.
        /// Areas count only participating hexes. Missing explicit target areas use each cell's initial participating area.
        /// In Preserve mode, each non-empty participating cell must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
            IHexMap<bool> participationMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (participationMask is null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(participationMask));

            var participationFlags = new bool[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
                participationFlags[i] = participationMask[i];

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes independently within each mask region.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the partition. Null excludes a hex. Equal values identify the same region,
        /// including disconnected hexes. All integer values, including zero and negative values, are valid.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes in the same region. Excluded hexes and region boundaries contribute no edge energy.
        /// Areas and connectivity are tracked separately for each cell in each region. Explicit target areas apply
        /// independently to every region where that cell is initially present; otherwise the initial participating
        /// area in that region is used. The returned cell combines its hexes from all regions.
        /// In Preserve mode, each non-empty participating cell within a region must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
            IHexMap<int?> regionsMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (regionsMask is null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(regionsMask));

            var participationFlags = new bool[partition.Topology.Count];
            var regions = new int[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
            {
                int? region = regionsMask[i];
                participationFlags[i] = region.HasValue;
                regions[i] = region.GetValueOrDefault();
            }

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags, regions);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

        /// <summary>Evolves assigned participating hexes independently within each mask region.</summary>
        /// <typeparam name="THexPartitionCell">The source partition cell type.</typeparam>
        /// <param name="partition">The partial partition whose assignments must agree with its retained cells. Unassigned hexes cannot participate.</param>
        /// <param name="participationMask">A Boolean map with the same topology. Only true hexes with source assignments participate.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the partition. Equal values identify the same region,
        /// including disconnected hexes. All integer values, including zero and negative values, are valid.
        /// </param>
        /// <param name="parameters">The immutable duration, energy, temperature, and connectivity settings.</param>
        /// <param name="random">The random generator, or null for a new generator. Supply a seeded generator for reproducible runs.</param>
        /// <returns>
        /// A new mutable partial partition map owned by the caller. Excluded and previously unassigned hexes contain null.
        /// New cell snapshots preserve source identifiers and order, including empty cells, with indexes in row-major order.
        /// Source-specific cell metadata is not copied. The source and masks are neither changed nor retained.
        /// </returns>
        /// <remarks>
        /// Each step makes one copy attempt per participating hex, sampled with replacement. A neighbor is sampled
        /// uniformly among participating edge-adjacent hexes in the same region. Excluded hexes and region boundaries contribute no edge energy.
        /// Areas and connectivity are tracked separately for each cell in each region. Explicit target areas apply
        /// independently to every region where that cell is initially present; otherwise the initial participating
        /// area in that region is used. The returned cell combines its hexes from all regions.
        /// In Preserve mode, each non-empty participating cell within a region must initially be six-connected and cannot
        /// vanish or fragment. Masking can invalidate connectivity and cause an exception even at zero steps.
        /// In None mode, disconnected cells and cell disappearance are allowed. Empty cells cannot acquire hexes.
        /// All source assignments and target identifiers are validated even when no hexes participate.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The partition, parameters, or a required mask are null.</exception>
        /// <exception cref="ArgumentException">
        /// A mask topology differs, source cells or assignments are inconsistent, a participating cell is disconnected
        /// in Preserve mode, or an explicit target identifier is absent from the source partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static PartialHexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this PartialHexPartitionMap<THexPartitionCell> partition,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));
            if (participationMask is null)
                throw new ArgumentNullException(nameof(participationMask));
            if (participationMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(participationMask));
            if (regionsMask is null)
                throw new ArgumentNullException(nameof(regionsMask));
            if (regionsMask.Topology != partition.Topology)
                throw new ArgumentException("Partition and mask must have the same topology.", nameof(regionsMask));

            var participationFlags = new bool[partition.Topology.Count];
            var regions = new int[partition.Topology.Count];
            for (int i = 0; i < participationFlags.Length; i++)
            {
                participationFlags[i] = participationMask[i];
                if (participationFlags[i])
                    regions[i] = regionsMask[i];
            }

            var simulation = CreateSimulation(partition, partition.Cells, id => id, parameters, participationFlags, regions);
            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartialPartition();
        }

    }
#pragma warning restore RS0026
}
