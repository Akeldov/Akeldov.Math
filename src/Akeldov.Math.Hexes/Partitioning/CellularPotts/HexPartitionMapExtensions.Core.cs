using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    public static partial class HexPartitionMapExtensions
    {
        private static CellularPottsSimulation CreateSimulation<TIdentifier, THexPartitionCell>(
            IHexMap<TIdentifier> partition, IReadOnlyList<THexPartitionCell> cells, Func<TIdentifier, int?> toNullable,
            CellularPottsParameters parameters, bool[]? participationFlags = null, int[]? regions = null)
            where THexPartitionCell : IHexPartitionCell
        {
            var topology = partition.Topology;
            // Revalidate all source cells and assignments before masking, including excluded hexes.
            var snapshot = new PartialHexPartitionMap<THexPartitionCell>(topology, cells);
            var ids = new int[cells.Count];
            var cellIndexById = new Dictionary<int, int>(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = snapshot.Cells[i].Id;
                cellIndexById.Add(ids[i], i);
            }
            foreach (int id in parameters.TargetAreas.Keys)
                if (!cellIndexById.ContainsKey(id))
                    throw new ArgumentException($"Target-area identifier {id} is absent from the partition.", nameof(parameters));

            var assignments = new int[topology.Count];
            for (int i = 0; i < assignments.Length; i++)
            {
                int? id = toNullable(partition[i]);
                if (id != snapshot[i])
                    throw new ArgumentException("Partition assignments must agree with the retained cell indexes.", nameof(partition));
                assignments[i] = id.HasValue && (participationFlags == null || participationFlags[i])
                    ? cellIndexById[id.Value] : -1;
            }

            var simulation = new CellularPottsSimulation(topology, assignments, ids, parameters, regions);
            if (parameters.Connectivity == CellularPottsConnectivity.Preserve && !simulation.HasConnectedCells())
                throw new ArgumentException("Every non-empty participating cell must be six-connected within each region.", nameof(partition));
            return simulation;
        }
    }
}
