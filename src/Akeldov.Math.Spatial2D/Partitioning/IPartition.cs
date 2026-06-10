using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning
{
    /// <summary>
    /// Represents a partition containing assigned items.
    /// </summary>
    /// <typeparam name="TItem">The partition item type.</typeparam>
    public interface IPartition<TItem>
    {
        /// <summary>
        /// Gets the read-only structural view of the items assigned to this partition.
        /// </summary>
        IReadOnlyList<TItem> Items { get; }
    }
}
