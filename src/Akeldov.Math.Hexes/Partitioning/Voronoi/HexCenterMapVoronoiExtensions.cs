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
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// Empty cells are preserved.
        /// </returns>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites)
        {
            return hexCenters.ToVoronoiHexPartitionMap(sites, EmptyCellPolicy.LeaveAsIs);
        }

        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no hexes.</param>
        /// <returns>
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// </exception>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            EmptyCellPolicy emptyCellPolicy)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters);
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
        /// Empty cells are preserved.
        /// </returns>
        public static MaskedVoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask)
        {
            return hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, EmptyCellPolicy.LeaveAsIs);
        }

        /// <summary>
        /// Assigns participating hex centers to their nearest weighted Voronoi site with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="participationMask">
        /// The Boolean map that indicates which hex centers participate in the partition.
        /// </param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no participating hexes.</param>
        /// <returns>
        /// A new read-only masked hex partition map with per-hex assignments and a semantic cell list.
        /// Excluded hexes have no assignment and return <see langword="null"/> from the result map.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// </exception>
        public static MaskedVoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask,
            EmptyCellPolicy emptyCellPolicy)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters, participationMask);
        }

        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site in the same mask region.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="participationMask">
        /// The map of region identifiers with the same topology as <paramref name="hexCenters"/>.
        /// Equal values identify the same region, including disconnected hexes. All integer values,
        /// including zero and negative values, are valid region identifiers; no hexes are excluded.
        /// </param>
        /// <returns>
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// Empty cells are preserved.
        /// </returns>
        /// <remarks>
        /// A site's region is the mask value at the hex containing its position, using the center map's
        /// geometry. Sites outside the map receive no hexes.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// A hex has no eligible site in its region.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<int> participationMask)
        {
            return hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, EmptyCellPolicy.LeaveAsIs);
        }

        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site in the same mask region
        /// with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="regionsMask">
        /// The map of region identifiers with the same topology as <paramref name="hexCenters"/>.
        /// Equal values identify the same region, including disconnected hexes. All integer values,
        /// including zero and negative values, are valid region identifiers; no hexes are excluded.
        /// </param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no hexes.</param>
        /// <returns>
        /// A new read-only hex partition map with per-hex cell identifiers and a semantic cell list.
        /// </returns>
        /// <remarks>
        /// A site's region is the mask value at the hex containing its position, using the center map's
        /// geometry. Sites outside the map receive no hexes and follow the configured empty-cell policy.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// A hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<int> regionsMask,
            EmptyCellPolicy emptyCellPolicy)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters, regionsMask);
        }

        /// <summary>
        /// Assigns participating hex centers to their nearest weighted Voronoi site in the same region.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="emptyCellPolicy">The policy for cells receiving no participating hexes.</param>
        /// <param name="participationMask">The Boolean map indicating which hex centers participate.</param>
        /// <param name="regionsMask">
        /// The region identifiers, with the same topology as the center map. Equal integer values
        /// identify the same region, including disconnected hexes; zero and negative values are valid.
        /// </param>
        /// <returns>
        /// A new read-only masked partition map with a semantic cell list. Excluded hexes return null.
        /// </returns>
        /// <remarks>
        /// A site's region is determined by the hex containing its position, regardless of that hex's
        /// participation. Sites outside the map receive no hexes. Empty-cell handling applies to all sites.
        /// </remarks>
        /// <exception cref="ArgumentNullException">A map is null.</exception>
        /// <exception cref="ArgumentException">The maps have different topologies.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A participating center coordinate is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A participating hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public static MaskedVoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask,
            EmptyCellPolicy emptyCellPolicy)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters, participationMask, regionsMask);
        }
    }
}
