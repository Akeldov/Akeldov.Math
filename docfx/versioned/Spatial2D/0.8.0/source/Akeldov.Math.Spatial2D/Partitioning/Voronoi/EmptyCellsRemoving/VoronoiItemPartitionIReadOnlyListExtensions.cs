using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    internal static partial class VoronoiItemPartitionIReadOnlyListExtensions
    {
        /// <summary>
        /// Returns only Voronoi partitions that contain at least one item.
        /// </summary>
        /// <typeparam name="TItem">The positioned partition item type.</typeparam>
        /// <param name="cells">The Voronoi partitions to filter.</param>
        /// <returns>A new mutable list of non-empty partitions owned by the caller.</returns>
        public static List<VoronoiItemPartition<TItem>> ExcludeEmptyCells<TItem>(this IReadOnlyList<VoronoiItemPartition<TItem>> cells)
            where TItem : IHasPosition2D
        {
            var nonEmptyCells = new List<VoronoiItemPartition<TItem>>(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.Items.Count != 0)
                {
                    nonEmptyCells.Add(cell);
                }
            }
            return nonEmptyCells;
        }
    }
}
