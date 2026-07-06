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
    }
}
