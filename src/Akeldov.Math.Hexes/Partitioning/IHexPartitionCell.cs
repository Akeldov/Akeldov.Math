using Akeldov.Math.Spatial2D;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents one cell of a hex-grid partition as a group of hex indexes.
    /// </summary>
    public interface IHexPartitionCell
    {
        /// <summary>
        /// Gets the non-negative identifier of this cell, unique within its partition.
        /// </summary>
        /// <remarks>
        /// The identifier need not match the cell's position in the partition's cell collection.
        /// </remarks>
        int Id { get; }

        /// <summary>
        /// Gets the read-only structural sequence of hex indexes belonging to this partition cell.
        /// </summary>
        IReadOnlyList<VectorXYInt> HexIndexes { get; }
    }
}
