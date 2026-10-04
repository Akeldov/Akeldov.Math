using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition, storing its cells as a read-only snapshot.
    /// </summary>
    /// <remarks>
    /// The input collection is copied in its original order. Cell objects are retained as-is.
    /// Empty partitions and empty cells are allowed. Map coverage and overlap between cells are not validated.
    /// </remarks>
    public sealed class HexPartition : IHexPartition
    {
        private readonly IReadOnlyList<IHexPartitionCell> _cells;

        /// <summary>
        /// Initializes a new partition with a read-only copy of the supplied cells.
        /// </summary>
        /// <param name="cells">The partition cells to retain in the copied collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cells"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="cells"/> contains a null cell.
        /// </exception>
        public HexPartition(IReadOnlyList<IHexPartitionCell> cells)
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            var copy = new IHexPartitionCell[cells.Count];
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell == null)
                    throw new ArgumentException("Partition cells must be non-null.", nameof(cells));

                copy[i] = cell;
            }

            _cells = Array.AsReadOnly(copy);
        }

        /// <summary>
        /// Gets the read-only structural snapshot of cells that make up this partition.
        /// </summary>
        /// <remarks>
        /// Changes to the constructor's input collection do not affect this collection.
        /// The cell objects themselves are retained as-is rather than cloned.
        /// </remarks>
        public IReadOnlyList<IHexPartitionCell> Cells => _cells;
    }
}
