using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition as a read-only map of cell identifiers and a collection of cells.
    /// </summary>
    /// <remarks>
    /// Map values are the assigned cells' non-negative <see cref="IHexPartitionCell.Id"/> values.
    /// Every hex in the topology must be assigned to exactly one partition cell.
    /// Cell identifiers need not match positions in <see cref="Cells"/>.
    /// </remarks>
    /// <typeparam name="THexPartitionCell">The type of cell in this partition.</typeparam>
    public interface IHexPartitionMap<out THexPartitionCell> : IHexMap<int>
        where THexPartitionCell : IHexPartitionCell
    {
        /// <summary>
        /// Gets the read-only structural collection of cells that make up this partition.
        /// </summary>
        IReadOnlyList<THexPartitionCell> Cells { get; }
    }
}
