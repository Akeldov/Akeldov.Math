using Akeldov.Math.Hexes.Geometry;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores the identifier of the Voronoi cell assigned to each hex center.
    /// </summary>
    /// <remarks>
    /// The initial assignments and read-only cell collection represent the partitioner's result.
    /// Inherited indexer setters change only the identifier map; they do not update the retained cells
    /// or their hex indexes. Use <see cref="ToMutableHexMap"/> to create an independent mutable copy.
    /// Initial identifiers equal the assigned cell's <see cref="HexPartitionCell.Id"/>,
    /// its <see cref="VoronoiHexPartitionCell.SiteIndex"/>, and its index in <see cref="HexPartitionMap{VoronoiHexPartitionCell}.Cells"/>.
    /// Use <c>Cells[this[index]]</c> to access the assigned cell's site and grouped hex indexes.
    /// Empty cells may be excluded by the partitioner's policy.
    /// Cells created for isolated exclaves follow the source cells in row-major component order.
    /// </remarks>
    public sealed class VoronoiHexPartitionMap : HexPartitionMap<VoronoiHexPartitionCell>, ISpatialHexMap<int>
    {
        internal VoronoiHexPartitionMap(HexCenterMap centers, int[] assignments, VoronoiHexPartitionCell[] cells)
            : base((centers ?? throw new ArgumentNullException(nameof(centers))).Topology, cells)
        {
            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));

            if (assignments.Length != Topology.Count)
                throw new ArgumentException("Assignment count must match center map dimensions.", nameof(assignments));

            for (int i = 0; i < assignments.Length; i++)
            {
                if (assignments[i] != this[i])
                    throw new ArgumentException($"Assignment at flat index {i} must match the cell's hex indexes.", nameof(assignments));
            }

            Centers = centers;
        }

        /// <summary>
        /// Gets the hex center map used to create this partition map.
        /// </summary>
        public HexCenterMap Centers { get; }

        /// <summary>
        /// Gets the spatial geometry used by the partition map.
        /// </summary>
        public HexMapGeometry Geometry => Centers.Geometry;

        /// <summary>
        /// Creates a spatial facade sharing this partition map's geometry and assignment array.
        /// </summary>
        /// <param name="map">The source partition map, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable spatial facade sharing the source map's assignments, or <see langword="null"/>
        /// if <paramref name="map"/> is null. Changes through either map are visible through the other.
        /// </returns>
        /// <remarks>
        /// The conversion takes constant time and allocates only the facade object. Assignments are not copied.
        /// Changes through the facade do not update the partition's retained cells or their hex indexes.
        /// Use <see cref="ToMutableHexMap"/> to create an independent mutable copy.
        /// </remarks>
        [return: NotNullIfNotNull("map")]
        public static implicit operator SpatialHexMap<int>?(VoronoiHexPartitionMap? map)
        {
            return map is null ? null : new SpatialHexMap<int>(map.Geometry, map.BackingValues);
        }

        /// <summary>
        /// Creates a new mutable caller-owned hex map of cell identifiers copied from this partition map.
        /// </summary>
        /// <returns>
        /// A new mutable hex map of cell identifiers. Mutating the returned map does not affect this partition map or
        /// the <see cref="HexPartitionMap{VoronoiHexPartitionCell}.Cells"/> semantic result.
        /// </returns>
        public HexMap<int> ToMutableHexMap()
        {
            var assignments = new int[Topology.Count];
            for (int i = 0; i < assignments.Length; i++)
                assignments[i] = this[i];

            return new HexMap<int>(Topology, assignments);
        }
    }
}
