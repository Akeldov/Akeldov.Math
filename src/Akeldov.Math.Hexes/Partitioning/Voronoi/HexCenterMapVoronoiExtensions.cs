using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Provides Voronoi partitioning extensions for hex center maps.
    /// </summary>
    public static class HexCenterMapVoronoiExtensions
    {
        /// <summary>
        /// Assigns every center from the specified hex center map to its nearest weighted Voronoi site.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <returns>
        /// A new read-only hex partition map with per-hex assignments and a semantic cell list.
        /// </returns>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites).Partition(hexCenters);
        }

        /// <summary>
        /// Assigns participating centers from the specified hex center map to their nearest weighted
        /// Voronoi site.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="participationMask">
        /// The Boolean map that indicates which hex centers participate in the partition.
        /// </param>
        /// <returns>
        /// A new read-only masked hex partition map with per-hex assignments and a semantic cell list.
        /// Excluded hexes have no assignment and return <see langword="null"/> from the result map.
        /// </returns>
        public static MaskedVoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites).Partition(hexCenters, participationMask);
        }
    }
}
