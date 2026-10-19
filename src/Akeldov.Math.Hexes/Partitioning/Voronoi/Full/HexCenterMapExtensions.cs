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
        /// <param name="parameters">The immutable relaxation and cell-handling settings, or null for defaults that preserve empty and disconnected cells.</param>
        /// <returns>
        /// A new mutable spatial partition map owned by the caller, retaining the input geometry,
        /// with per-hex cell identifiers and a read-only semantic cell list.
        /// </returns>
        /// <remarks>
        /// Initial identifiers equal the assigned cell's Id, SiteIndex, and index in Cells.
        /// Use <c>Cells[this[index]]</c> to access its site and grouped hex indexes.
        /// Empty cells may be excluded by policy; isolated exclave cells follow source cells in row-major component order.
        /// Indexer setters change only assignments, not the retained cells or their hex indexes.
        /// Use <c>partition.MapValues((int id) => id)</c> for an independent mutable spatial copy.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// The policy is <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// </exception>
        public static SpatialHexPartitionMap<VoronoiHexPartitionCell> PartitionByVoronoi(
            this HexCenterMap hexCenters,
            IReadOnlyList<Site> sites,
            VoronoiHexPartitionParameters? parameters = null)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            parameters ??= new VoronoiHexPartitionParameters();

            var validatedSites = CopyAndValidateSites(sites);

            var (_, cells, _) = PartitionCore(
                hexCenters, validatedSites, parameters.EmptyCellPolicy, null, null, null,
                parameters.ExclavePolicy, parameters.RelaxationIterations);
            return new SpatialHexPartitionMap<VoronoiHexPartitionCell>(hexCenters.Geometry, cells);
        }

        /// <summary>
        /// Assigns every hex center to its nearest weighted Voronoi site in the same mask region
        /// with empty-cell handling.
        /// </summary>
        /// <param name="hexCenters">The hex center map to partition.</param>
        /// <param name="regionsMask">
        /// The map of region identifiers with the same topology as <paramref name="hexCenters"/>.
        /// Equal values identify the same region, including disconnected hexes. All integer values,
        /// including zero and negative values, are valid region identifiers; no hexes are excluded.
        /// </param>
        /// <param name="sites">The Voronoi sites used for hex-center assignment.</param>
        /// <param name="parameters">The immutable relaxation and cell-handling settings, or null for defaults that preserve empty and disconnected cells.</param>
        /// <returns>
        /// A new mutable spatial partition map owned by the caller, retaining the input geometry,
        /// with per-hex cell identifiers and a read-only semantic cell list.
        /// </returns>
        /// <remarks>
        /// A site's region is the mask value at the hex containing its position, using the center map's
        /// geometry. Sites outside the map receive no hexes and follow the configured empty-cell policy.
        /// Initial identifiers equal the assigned cell's Id, SiteIndex, and index in Cells.
        /// Empty cells may be excluded by policy; isolated exclave cells follow source cells in row-major component order.
        /// Indexer setters change only assignments, not the retained cells or their hex indexes.
        /// Use <c>partition.MapValues((int id) => id)</c> for an independent mutable spatial copy.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A participating center is not finite.</exception>
        /// <exception cref="InvalidOperationException">
        /// A hex has no eligible site in its region, or the policy is
        /// <see cref="EmptyCellPolicy.ThrowException"/> and a cell receives no hexes.
        /// A zero-weight site is eligible only at its position within the geometry tolerance.
        /// </exception>
        public static SpatialHexPartitionMap<VoronoiHexPartitionCell> PartitionByVoronoi(
            this HexCenterMap hexCenters,
            IHexMap<int> regionsMask,
            IReadOnlyList<Site> sites,
            VoronoiHexPartitionParameters? parameters = null)
        {
            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            parameters ??= new VoronoiHexPartitionParameters();

            var validatedSites = CopyAndValidateSites(sites);
            if (regionsMask == null)
                throw new ArgumentNullException(nameof(regionsMask));

            if (hexCenters.Topology != regionsMask.Topology)
                throw new ArgumentException("Hex center map and participation mask must have the same topology.", nameof(regionsMask));

            var siteIndexesByRegion = GroupSiteIndexesByRegion(validatedSites, hexCenters.Geometry, regionsMask);

            var (_, cells, _) = PartitionCore(
                hexCenters, validatedSites, parameters.EmptyCellPolicy, null, regionsMask, siteIndexesByRegion,
                parameters.ExclavePolicy, parameters.RelaxationIterations);
            return new SpatialHexPartitionMap<VoronoiHexPartitionCell>(hexCenters.Geometry, cells);
        }
    }
#pragma warning restore RS0026
}
