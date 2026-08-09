using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    internal static partial class VoronoiItemPartitionIReadOnlyListExtensions
    {
        /// <summary>
        /// Verifies that every Voronoi partition contains at least one item.
        /// </summary>
        /// <typeparam name="TItem">The positioned partition item type.</typeparam>
        /// <param name="cells">The read-only Voronoi partition result to validate.</param>
        /// <returns>The same <paramref name="cells"/> instance after validation.</returns>
        public static IReadOnlyList<VoronoiItemPartition<TItem>> ThrowIfAnyEmptyCell<TItem>(this IReadOnlyList<VoronoiItemPartition<TItem>> cells)
            where TItem : IHasPosition2D
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.Items.Count == 0)
                {
                    throw new InvalidOperationException($"Couldn't tessellate by empty cells, empty cell: {cell.Site}.");
                }
            }
            return cells;
        }
    }
}
