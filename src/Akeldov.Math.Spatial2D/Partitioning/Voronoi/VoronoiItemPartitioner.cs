using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Assigns positioned items to weighted Voronoi sites.
    /// </summary>
    /// <typeparam name="TItem">The positioned item type to partition.</typeparam>
    public class VoronoiItemPartitioner<TItem> : IPartitioner<VoronoiItemPartition<TItem>, TItem>
        where TItem : IHasPosition2D
    {
        private readonly Site[] _sites;
        private readonly int _relaxationIterations;
        private readonly EmptyCellPolicy _emptyCellPolicy;

        /// <summary>Initializes a Voronoi item partitioner with relaxation and empty-partition settings.</summary>
        /// <param name="sites">The initial Voronoi sites, copied into retained structural state.</param>
        /// <param name="parameters">The immutable partition settings, or null to disable relaxation and throw on empty partitions.</param>
        public VoronoiItemPartitioner(IReadOnlyList<Site> sites, VoronoiPartitionParameters? parameters = null)
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));

            if (sites.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(sites));

            parameters ??= new VoronoiPartitionParameters();

            bool hasNonZeroWeight = false;

            _sites = new Site[sites.Count];
            for (int i = 0; i < sites.Count; i++)
            {
                if (!PointXYValidation.IsFinite(sites[i].Position))
                    throw new ArgumentOutOfRangeException(nameof(sites), "Site position coordinates must be finite.");

                if (sites[i].Weight < 0f || float.IsNaN(sites[i].Weight))
                    throw new ArgumentOutOfRangeException(nameof(sites), "Site weight must be non-negative and not NaN.");

                if (sites[i].Weight > 0f)
                    hasNonZeroWeight = true;

                _sites[i] = sites[i];
            }

            if (!hasNonZeroWeight)
                throw new ArgumentException("At least one site weight must be positive.", nameof(sites));

            _relaxationIterations = parameters.RelaxationIterations;
            _emptyCellPolicy = parameters.EmptyCellPolicy;
        }

        /// <summary>
        /// Assigns the specified items to Voronoi site partitions.
        /// </summary>
        /// <param name="items">The positioned items to partition.</param>
        /// <returns>
        /// A read-only list of generated Voronoi item partitions that preserves the partitioner result invariants.
        /// </returns>
        public IReadOnlyList<VoronoiItemPartition<TItem>> Partition(IReadOnlyList<TItem> items)
        {
            IReadOnlyList<VoronoiItemPartition<TItem>> cells = PartitionInternal(_sites, items);

            for (int i = 0; i < _relaxationIterations; i++)
            {
                var centroidSites = cells.ToCentroidSites();
                cells = PartitionInternal(centroidSites, items);
            }

            switch (_emptyCellPolicy)
            {
                case EmptyCellPolicy.Exclude:
                    return cells.ExcludeEmptyCells();
                case EmptyCellPolicy.LeaveAsIs:
                    return cells;
                default:
                    return cells.ThrowIfAnyEmptyCell();
            }
        }

        private static List<VoronoiItemPartition<TItem>> PartitionInternal(Site[] sites, IReadOnlyList<TItem> items)
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));

            if (sites.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(sites));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(items));

            var buckets = new List<TItem>[sites.Length];
            for (int i = 0; i < sites.Length; i++) buckets[i] = new List<TItem>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is null)
                    throw new ArgumentException("Partition items cannot contain null elements.", nameof(items));

                if (!PointXYValidation.IsFinite(item.Position))
                    throw new ArgumentException("Partition item positions must be finite.", nameof(items));

                int siteIndex = sites.GetNearestWeightedSiteIndex(item.Position);
                buckets[siteIndex].Add(item);
            }

            var cells = new List<VoronoiItemPartition<TItem>>(sites.Length);
            for (int i = 0; i < sites.Length; i++)
            {
                var site = sites[i];
                var siteItems = buckets[i];
                var cell = new VoronoiItemPartition<TItem>(site, siteItems);
                cells.Add(cell);
            }

            return cells;
        }
    }
}
