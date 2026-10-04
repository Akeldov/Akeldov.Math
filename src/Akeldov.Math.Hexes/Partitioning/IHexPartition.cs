using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition as a read-only collection of partition cells.
    /// </summary>
    public interface IHexPartition
    {
        /// <summary>
        /// Gets the read-only structural collection of cells that make up this partition.
        /// </summary>
        IReadOnlyList<IHexPartitionCell> Cells { get; }
    }
}
