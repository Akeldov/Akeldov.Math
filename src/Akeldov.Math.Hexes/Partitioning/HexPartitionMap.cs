using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition with a read-only snapshot of cells and mutable per-hex cell identifiers.
    /// </summary>
    /// <typeparam name="THexPartitionCell">The type of cells retained by the partition.</typeparam>
    /// <remarks>
    /// The input collection is copied in its original order. Cell objects are retained as-is.
    /// Empty cells are allowed. An empty partition requires an empty topology.
    /// At construction, cell identifiers must be unique and non-negative, and every hex must belong to exactly one cell.
    /// Custom cell implementations must keep their identifiers and indexes stable to remain consistent
    /// with the identifier map.
    /// Inherited indexer setters change only the identifier map; they do not update the retained cells
    /// or their hex indexes. Callers must keep map assignments consistent with the retained cells.
    /// </remarks>
    public class HexPartitionMap<THexPartitionCell> : IntHexMap, IHexPartitionMap<THexPartitionCell>
        where THexPartitionCell : IHexPartitionCell
    {
        private readonly IReadOnlyList<THexPartitionCell> _cells;

        /// <summary>
        /// Initializes a new partition with a read-only copy of the supplied cells and their assignments.
        /// </summary>
        /// <param name="topology">The topology containing the partition's hex indexes.</param>
        /// <param name="cells">The partition cells to retain in the copied collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cells"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="cells"/> contains a null cell, a negative or duplicate identifier,
        /// null hex indexes, an index outside <paramref name="topology"/>, a hex assigned to different cells,
        /// or does not cover every hex in <paramref name="topology"/>.
        /// </exception>
        public HexPartitionMap(HexMapTopology topology, IReadOnlyList<THexPartitionCell> cells)
            : base(topology)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            var copy = new THexPartitionCell[cells.Count];
            var ids = new HashSet<int>();
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell == null)
                    throw new ArgumentException("Partition cells must be non-null.", nameof(cells));

                int id = cell.Id;
                if (id < 0)
                    throw new ArgumentException("Partition cell identifiers must be non-negative.", nameof(cells));

                if (!ids.Add(id))
                    throw new ArgumentException($"Duplicate partition cell identifier: {id}.", nameof(cells));

                var hexIndexes = cell.HexIndexes;
                if (hexIndexes == null)
                    throw new ArgumentException("Partition cell hex indexes must be non-null.", nameof(cells));

                copy[i] = cell;
            }

            _cells = Array.AsReadOnly(copy);
            BuildAssignments(copy);
        }

        /// <summary>
        /// Gets the read-only structural snapshot of cells that make up this partition.
        /// </summary>
        /// <remarks>
        /// Changes to the constructor's input collection do not affect this collection.
        /// The cell objects themselves are retained as-is rather than cloned.
        /// </remarks>
        public IReadOnlyList<THexPartitionCell> Cells => _cells;

        private void BuildAssignments(THexPartitionCell[] cells)
        {
            for (int i = 0; i < Topology.Count; i++)
                this[i] = -1;

            for (int i = 0; i < cells.Length; i++)
            {
                int id = cells[i].Id;
                var hexIndexes = cells[i].HexIndexes;
                for (int j = 0; j < hexIndexes.Count; j++)
                {
                    var index = hexIndexes[j];
                    if (index.X < 0 || index.X >= Topology.Resolution.X ||
                        index.Y < 0 || index.Y >= Topology.Resolution.Y)
                        throw new ArgumentException($"Partition hex index out of bounds: {index}.", nameof(cells));

                    int flatIndex = index.Y * Topology.Resolution.X + index.X;
                    if (this[flatIndex] >= 0 && this[flatIndex] != id)
                        throw new ArgumentException($"Hex index belongs to different partition cells: {index}.", nameof(cells));

                    this[flatIndex] = id;
                }
            }

            for (int i = 0; i < Topology.Count; i++)
            {
                if (this[i] < 0)
                    throw new ArgumentException($"Hex at flat index {i} has no assigned partition cell.", nameof(cells));
            }
        }
    }
}
