using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.CellularPotts
{
    public static partial class HexPartitionMapExtensions
    {
        internal static CellularPottsSimulation CreateSimulation<TIdentifier, THexPartitionCell>(
            IHexMap<TIdentifier> partition, IReadOnlyList<THexPartitionCell> cells, Func<TIdentifier, int?> toNullable,
            IReadOnlyDictionary<int, float>? targetAreas, CellularPottsParameters parameters)
            where THexPartitionCell : IHexPartitionCell
        {
            var topology = partition.Topology;
            // Revalidate all source cells and assignments, including unassigned hexes.
            var snapshot = new PartialHexPartitionMap<THexPartitionCell>(topology, cells);
            var ids = new int[cells.Count];
            var cellIndexById = new Dictionary<int, int>(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = snapshot.Cells[i].Id;
                cellIndexById.Add(ids[i], i);
            }
            if (targetAreas != null)
            {
                foreach (var pair in targetAreas)
                {
                    if (pair.Key < 0 || !cellIndexById.ContainsKey(pair.Key))
                        throw new ArgumentException($"Target-area identifier {pair.Key} is absent from the partition.", nameof(targetAreas));
                    if (!float.IsFinite(pair.Value) || pair.Value < 0f || (double)pair.Value > int.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(targetAreas), "Target areas must be finite hex counts between zero and int.MaxValue.");
                }
            }

            var assignments = new int[topology.Count];
            for (int i = 0; i < assignments.Length; i++)
            {
                int? id = toNullable(partition[i]);
                if (id != snapshot[i])
                    throw new ArgumentException("Partition assignments must agree with the retained cell indexes.", nameof(partition));
                assignments[i] = id.HasValue
                    ? cellIndexById[id.Value] : -1;
            }

            var simulation = new CellularPottsSimulation(topology, assignments, ids, targetAreas, parameters);
            if (parameters.Connectivity == CellularPottsConnectivity.Preserve && !simulation.HasConnectedCells())
                throw new ArgumentException("Every non-empty participating cell must be six-connected.", nameof(partition));
            return simulation;
        }
    }
}
