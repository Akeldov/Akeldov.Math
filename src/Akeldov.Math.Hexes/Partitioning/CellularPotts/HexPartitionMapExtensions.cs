using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    /// <summary>Provides Cellular Potts evolution of hex partitions with optional connectivity preservation.</summary>
    public static class HexPartitionMapExtensions
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
        /// Source cells or assignments are inconsistent, a non-empty cell is disconnected in Preserve mode,
        /// or an explicit target identifier is absent from the partition.
        /// </exception>
        /// <exception cref="InvalidOperationException">Energy coefficients are too large to calculate an energy change.</exception>
        public static HexPartitionMap<HexPartitionCell> ApplyCellularPotts<THexPartitionCell>(
            this HexPartitionMap<THexPartitionCell> partition,
            CellularPottsParameters parameters,
            Random? random = null)
            where THexPartitionCell : IHexPartitionCell
        {
            if (partition is null)
                throw new ArgumentNullException(nameof(partition));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            // Revalidate custom cells and compare the mutable map with their current structural snapshot.
            var snapshot = new HexPartitionMap<THexPartitionCell>(partition.Topology, partition.Cells);
            var ids = new int[snapshot.Cells.Count];
            var cellIndexById = new Dictionary<int, int>(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = snapshot.Cells[i].Id;
                cellIndexById.Add(ids[i], i);
            }

            foreach (int id in parameters.TargetAreas.Keys)
            {
                if (!cellIndexById.ContainsKey(id))
                    throw new ArgumentException($"Target-area identifier {id} is absent from the partition.", nameof(parameters));
            }

            var assignments = new int[snapshot.Topology.Count];
            for (int i = 0; i < assignments.Length; i++)
            {
                if (partition[i] != snapshot[i])
                    throw new ArgumentException("Partition assignments must agree with the retained cell indexes.", nameof(partition));
                assignments[i] = cellIndexById[snapshot[i]];
            }

            var simulation = new CellularPottsSimulation(partition.Topology, assignments, ids, parameters);
            if (parameters.Connectivity == CellularPottsConnectivity.Preserve && !simulation.HasConnectedCells())
                throw new ArgumentException("Every non-empty partition cell must be six-connected.", nameof(partition));

            simulation.Run(parameters.StepCount, random ?? new Random());
            return simulation.CreatePartition();
        }
    }
}
