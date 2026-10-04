using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a partial hex-grid partition as a read-only map of nullable cell identifiers
    /// and a collection of partition cells.
    /// </summary>
    /// <remarks>
    /// Assigned hexes contain their cells' non-negative <see cref="IHexPartitionCell.Id"/> values.
    /// Unassigned hexes contain <see langword="null"/>. Each assigned hex belongs to exactly one cell.
    /// Cell identifiers are unique within the partition and need not match positions in <see cref="Cells"/>.
    /// </remarks>
    public interface IPartialHexPartition : IHexMap<int?>
    {
        /// <summary>
        /// Gets the read-only structural collection of cells that make up this partial partition.
        /// </summary>
        IReadOnlyList<IHexPartitionCell> Cells { get; }
    }
}
