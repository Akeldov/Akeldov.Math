using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
#pragma warning disable RS0026 // Mask overloads intentionally share optional partitioning policies.
    /// <summary>
    /// Provides Voronoi partitioning extensions for hex center maps.
    /// </summary>
    /// <remarks>
    /// Relaxation moves non-empty sites to the arithmetic mean of assigned participating hex centers,
    /// preserving weights and retaining empty-site positions. Region membership is fixed by each site's
    /// initial position. Empty-cell and exclave policies apply to the final partition. Input sites are unchanged.
    /// </remarks>
    public static partial class HexCenterMapExtensions
    {
        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Defaults to zero, disabling relaxation.</param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no hexes.</param>
        /// <param name="exclavePolicy">The policy for disconnected cell components in the final partition.</param>
        /// <returns>
        /// A new mutable hex partition map with per-hex cell identifiers and a read-only semantic cell list.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative, the exclave policy is invalid, or a participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// </exception>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            var validatedSites = CopyAndValidateSites(sites);
            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

            var (assignments, cells, _) = PartitionCore(hexCenters, validatedSites, emptyCellPolicy, null, null, null, exclavePolicy, relaxationIterations);
            return new VoronoiHexPartitionMap(hexCenters, assignments, cells);
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
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Defaults to zero, disabling relaxation.</param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no hexes.</param>
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
        /// <returns>
        /// A new mutable hex partition map with per-hex cell identifiers and a read-only semantic cell list.
        /// </returns>
        /// <remarks>
        /// A site's region is the mask value at the hex containing its position, using the center map's
        /// geometry. Sites outside the map receive no hexes and follow the configured empty-cell policy.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative, the exclave policy is invalid, or a participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public static VoronoiHexPartitionMap ToVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<int> regionsMask,
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            var validatedSites = CopyAndValidateSites(sites);
            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));

            if (hexCenters.Topology != regionsMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(regionsMask));

            var siteIndexesByRegion = GroupSiteIndexesByRegion(validatedSites, hexCenters.Geometry, regionsMask);
            if (exclavePolicy != ExclavePolicy.LeaveAsIs && exclavePolicy != ExclavePolicy.ReassignToClosestCell)
                throw new ArgumentOutOfRangeException(nameof(exclavePolicy));

            var (assignments, cells, _) = PartitionCore(hexCenters, validatedSites, emptyCellPolicy, null, regionsMask, siteIndexesByRegion, exclavePolicy, relaxationIterations);
            return new VoronoiHexPartitionMap(hexCenters, assignments, cells);
        }
    }
#pragma warning restore RS0026
}
