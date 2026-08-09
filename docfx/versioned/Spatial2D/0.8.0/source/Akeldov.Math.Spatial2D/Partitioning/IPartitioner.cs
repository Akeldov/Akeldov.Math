using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning
{
    /// <summary>
    /// Represents an object that partitions items into groups.
    /// </summary>
    /// <typeparam name="TPartition">The partition result type.</typeparam>
    /// <typeparam name="TItem">The item type to partition.</typeparam>
    public interface IPartitioner<TPartition, TItem>
        where TPartition : IPartition<TItem>
    {
        /// <summary>
        /// Partitions the specified items.
        /// </summary>
        /// <param name="items">The items to partition.</param>
        /// <returns>
        /// A read-only list of generated partitions whose collection-level invariants are owned by the partitioner.
        /// </returns>
        IReadOnlyList<TPartition> Partition(IReadOnlyList<TItem> items);
    }
}
