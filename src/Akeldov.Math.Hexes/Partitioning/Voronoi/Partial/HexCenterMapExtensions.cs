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
        /// Assigns participating hex centers to their nearest weighted Voronoi site with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="participationMask">
        /// The Boolean map that indicates which hex centers participate in the partition.
        /// </param>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Defaults to zero, disabling relaxation.</param>
        /// <param name="emptyCellPolicy">The policy used for cells that receive no participating hexes.</param>
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
        /// <returns>
        /// A new mutable partial hex partition map with nullable per-hex cell identifiers and a read-only semantic cell list.
        /// Excluded hexes have no assignment and return <see langword="null"/> from the result map.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative, the exclave policy is invalid, or a participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no participating hexes.
        /// </exception>
        public static PartialVoronoiHexPartitionMap ToPartialVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask,
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters, participationMask, exclavePolicy, relaxationIterations);
        }

        /// <summary>
        /// Assigns hex centers with non-null region identifiers to weighted Voronoi sites in their region
        /// with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="regionsMask">
        /// Region identifiers with the same topology as the center map. Null excludes a hex;
        /// all integer values, including zero and negative values, identify participating regions.
        /// </param>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Defaults to zero, disabling relaxation.</param>
        /// <param name="emptyCellPolicy">The policy for cells receiving no participating hexes.</param>
        /// <param name="exclavePolicy">The policy for disconnected cell components; region boundaries are preserved.</param>
        /// <returns>A new mutable partial partition map with a read-only semantic cell list. Excluded hexes return null.</returns>
        /// <remarks>
        /// Sites outside the map or in a hex with a null region receive no hexes and follow the empty-cell policy.
        /// </remarks>
        /// <exception cref="ArgumentNullException">A map or the site list is null.</exception>
        /// <exception cref="ArgumentException">The maps have different topologies.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The iteration count is negative, the exclave policy is invalid, or a participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A participating hex has no eligible site in its region, or the empty-cell policy requires throwing.
        /// </exception>
        public static PartialVoronoiHexPartitionMap ToPartialVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<int?> regionsMask,
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy).Partition(hexCenters, regionsMask, exclavePolicy, relaxationIterations);
        }

        /// <summary>
        /// Assigns participating hex centers to weighted Voronoi sites in the same region,
        /// applying centroid relaxation before empty-cell and exclave handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="sites">The initial Voronoi sites; their weights and input values are preserved.</param>
        /// <param name="participationMask">The Boolean map indicating which hex centers participate.</param>
        /// <param name="regionsMask">Region identifiers with the same topology as the center map.</param>
        /// <param name="relaxationIterations">The non-negative number of centroid relaxation iterations. Defaults to zero, disabling relaxation.</param>
        /// <param name="emptyCellPolicy">The policy for cells receiving no participating hexes in the final partition.</param>
        /// <param name="exclavePolicy">The policy for disconnected components in the final partition.</param>
        /// <returns>A new mutable partial partition map with a read-only semantic cell list. Excluded hexes return null.</returns>
        /// <remarks>
        /// Each iteration moves non-empty sites to the arithmetic mean of their assigned participating
        /// hex centers and partitions again. Empty sites retain their positions. A site's region is fixed
        /// by its initial position, even if its centroid lies outside that region or the participation mask.
        /// Sites outside the map receive no hexes. Exclave handling runs after all relaxation iterations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">A map or the site list is null.</exception>
        /// <exception cref="ArgumentException">The maps have different topologies or all site weights are zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The iteration count is negative, the exclave policy is invalid, a participating center or site
        /// position is not finite, the site list is empty, or a site weight is negative or NaN.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// A participating hex has no eligible site in its region, or the final partition contains an
        /// empty cell when the empty-cell policy requires throwing.
        /// </exception>
        public static PartialVoronoiHexPartitionMap ToPartialVoronoiHexPartitionMap(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            IHexMap<bool> participationMask,
            IHexMap<int> regionsMask,
            int relaxationIterations = 0,
            EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
            ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (relaxationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(relaxationIterations));

            return new VoronoiHexPartitioner(sites, emptyCellPolicy)
                .Partition(hexCenters, participationMask, regionsMask, exclavePolicy, relaxationIterations);
        }
    }
#pragma warning restore RS0026
}
