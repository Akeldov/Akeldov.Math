using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Represents positioned items assigned to a Voronoi site.
    /// </summary>
    /// <typeparam name="TItem">The positioned item type assigned to the partition.</typeparam>
    public class VoronoiItemPartition<TItem> : IPartition<TItem>
        where TItem : IHasPosition2D
    {
        /// <summary>
        /// Initializes a new Voronoi item partition.
        /// </summary>
        /// <param name="site">The site that owns this partition.</param>
        /// <param name="items">The items assigned to this partition.</param>
        public VoronoiItemPartition(Site site, IReadOnlyList<TItem> items)
        {
            Site = site;
            Items = CopyItems(items);
        }

        /// <summary>
        /// Gets the site that owns this partition.
        /// </summary>
        public Site Site { get; }

        /// <summary>
        /// Gets the read-only structural view of the items assigned to this partition.
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }

        private static IReadOnlyList<TItem> CopyItems(IReadOnlyList<TItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var copy = new TItem[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is null)
                    throw new ArgumentException("Voronoi item partition items cannot contain null elements.", nameof(items));

                copy[i] = item;
            }

            return Array.AsReadOnly(copy);
        }
    }
}
