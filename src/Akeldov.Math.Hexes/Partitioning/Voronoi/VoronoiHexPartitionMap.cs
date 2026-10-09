using Akeldov.Math.Hexes.Geometry;
using System;

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
