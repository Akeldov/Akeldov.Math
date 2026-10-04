using Akeldov.Math.Spatial2D;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a group of hex indexes in a partition of a hex grid.
    /// </summary>
    public interface IPartition
    {
        /// <summary>
        /// Gets the read-only structural sequence of hex indexes belonging to this partition.
        /// </summary>
        IReadOnlyList<VectorXYInt> HexIndexes { get; }
    }
}
