using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    internal static partial class VoronoiItemPartitionIReadOnlyListExtensions
    {
        /// <summary>
        /// Creates weighted Voronoi sites at the centroids of non-empty partitions, preserving the original sites for empty partitions.
        /// </summary>
        /// <typeparam name="TItem">The positioned partition item type.</typeparam>
        /// <param name="cells">The Voronoi partitions to convert to centroid sites.</param>
        /// <returns>A new array of centroid sites owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cells"/> is null.</exception>
        public static Site[] ToCentroidSites<TItem>(this IReadOnlyList<VoronoiItemPartition<TItem>> cells)
            where TItem : IHasPosition2D
        {
            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            var newSites = new Site[cells.Count];

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var site = cell.Site;
                var items = cell.Items;
                if (items.Count == 0) { newSites[i] = site; continue; }
                var centroid = items.GetCentroid();
                newSites[i] = new Site(centroid, site.Weight);
            }

            return newSites;
        }
    }
}
