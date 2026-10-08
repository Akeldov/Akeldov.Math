using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

#pragma warning disable CA2201 // Preserve the established hex-map indexer exception behavior.
#pragma warning disable MA0012 // Hex map indexers use IndexOutOfRangeException for out-of-bounds indexes.

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition with a read-only snapshot of cells and per-hex cell identifiers.
    /// </summary>
    /// <remarks>
    /// The input collection is copied in its original order. Cell objects are retained as-is.
    /// Empty cells are allowed. An empty partition requires an empty topology.
    /// Cell identifiers must be unique and non-negative, and every hex must belong to exactly one cell.
    /// Custom cell implementations must keep their identifiers and indexes stable to remain consistent
    /// with the assignment snapshot.
    /// </remarks>
    public sealed class HexPartitionMap : IHexPartitionMap<IHexPartitionCell>
    {
        private readonly IReadOnlyList<IHexPartitionCell> _cells;
        private readonly int[] _assignments;

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
        public HexPartitionMap(HexMapTopology topology, IReadOnlyList<IHexPartitionCell> cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            var copy = new IHexPartitionCell[cells.Count];
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
            _assignments = BuildAssignments(topology, copy);
            Topology = topology;
        }

        /// <summary>
        /// Gets the topology used by this partition.
        /// </summary>
        public HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the assigned partition cell identifier.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is outside the topology.</exception>
        public int this[VectorXYInt index]
        {
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _assignments[index.Y * Topology.Resolution.X + index.X];
            }
        }

        /// <summary>
        /// Gets the assigned partition cell identifier at a row-major flat index.
        /// </summary>
        /// <param name="index">The zero-based row-major index.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the index is outside the topology.</exception>
        public int this[int index] => _assignments[index];

        /// <summary>
        /// Gets the read-only structural snapshot of cells that make up this partition.
        /// </summary>
        /// <remarks>
        /// Changes to the constructor's input collection do not affect this collection.
        /// The cell objects themselves are retained as-is rather than cloned.
        /// </remarks>
        public IReadOnlyList<IHexPartitionCell> Cells => _cells;

        private static int[] BuildAssignments(HexMapTopology topology, IHexPartitionCell[] cells)
        {
            var assignments = new int[topology.Count];
            Array.Fill(assignments, -1);
            for (int i = 0; i < cells.Length; i++)
            {
                int id = cells[i].Id;
                var hexIndexes = cells[i].HexIndexes;
                for (int j = 0; j < hexIndexes.Count; j++)
                {
                    var index = hexIndexes[j];
                    if (index.X < 0 || index.X >= topology.Resolution.X ||
                        index.Y < 0 || index.Y >= topology.Resolution.Y)
                        throw new ArgumentException($"Partition hex index out of bounds: {index}.", nameof(cells));

                    int flatIndex = index.Y * topology.Resolution.X + index.X;
                    if (assignments[flatIndex] >= 0 && assignments[flatIndex] != id)
                        throw new ArgumentException($"Hex index belongs to different partition cells: {index}.", nameof(cells));

                    assignments[flatIndex] = id;
                }
            }

            for (int i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] < 0)
                    throw new ArgumentException($"Hex at flat index {i} has no assigned partition cell.", nameof(cells));
            }

            return assignments;
        }
    }
}
