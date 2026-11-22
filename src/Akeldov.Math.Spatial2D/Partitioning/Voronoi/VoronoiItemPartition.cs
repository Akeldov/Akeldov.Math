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
        /// <param name="items">The caller-owned list of assigned items. Changes to its contents
        /// are visible through <see cref="Items"/>. The caller must keep its elements non-null.</param>
        /// <exception cref="ArgumentNullException">The list is null.</exception>
        /// <exception cref="ArgumentException">The list contains a null item.</exception>
#pragma warning disable MA0016 // The partition exposes a live view of this list.
        public VoronoiItemPartition(Site site, IReadOnlyList<TItem> items)
#pragma warning restore MA0016
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Site = site;
            Items = items;
        }

        /// <summary>
        /// Gets the site that owns this partition.
        /// </summary>
        public Site Site { get; }

        /// <summary>
        /// Gets a live, read-only view of the items in the caller-owned list.
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }
    }
}
