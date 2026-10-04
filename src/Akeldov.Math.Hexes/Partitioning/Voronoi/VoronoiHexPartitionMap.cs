using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores the identifier of the Voronoi cell assigned to each hex center.
    /// </summary>
    /// <remarks>
    /// The map is a read-only semantic result produced by the partitioner. Per-hex assignments and
    /// <see cref="Cells"/> are kept consistent with the original partition result. Use
    /// <see cref="ToMutableHexMap"/> to create a new mutable caller-owned copy of the assignments.
    /// Map indexers return the assigned cell's <see cref="HexPartitionCell.Id"/>, equal to its
    /// <see cref="VoronoiCell.SiteIndex"/> and its index in <see cref="Cells"/>.
    /// Use <c>Cells[this[index]]</c> to access the assigned cell's site and grouped hex indexes.
    /// </remarks>
    public sealed class VoronoiHexPartitionMap : ISpatialHexMap<int>, IHexPartition
    {
        private readonly int[] _assignments;

        internal VoronoiHexPartitionMap(HexCenterMap centers, int[] assignments, VoronoiCell[] cells)
        {
            Centers = centers ?? throw new ArgumentNullException(nameof(centers));

            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            int count = centers.Topology.Count;
            if (assignments.Length != count)
                throw new ArgumentException("Assignment count must match center map dimensions.", nameof(assignments));

            Topology = centers.Topology;
            _assignments = CopyAssignments(assignments);
            Cells = Array.AsReadOnly(CopyCells(cells));
        }

        /// <summary>
        /// Gets the hex center map used to create this partition map.
        /// </summary>
        public HexCenterMap Centers { get; }

        /// <summary>
        /// Gets the topology used by the partition map.
        /// </summary>
        public HexMapTopology Topology { get; }

        /// <summary>
        /// Gets the spatial geometry used by the partition map.
        /// </summary>
        public HexMapGeometry Geometry => Centers.Geometry;

        /// <summary>
        /// Gets the identifier of the Voronoi cell assigned to the specified hex index.
        /// </summary>
        /// <param name="index">The zero-based hex index.</param>
        public int this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Resolution.X ||
                    index.Y < 0 || index.Y >= Topology.Resolution.Y)
                    throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

                return _assignments[GetFlatIndex(index)];
            }
        }

        /// <summary>
        /// Gets the identifier of the Voronoi cell assigned to the specified flat hex index.
        /// </summary>
        /// <param name="index">The zero-based flat hex index.</param>
        public int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _assignments[index];
        }

        /// <summary>
        /// Gets the read-only semantic result of Voronoi cells in source-site order.
        /// </summary>
        /// <remarks>
        /// This list represents the partitioner's cells and their grouped hex indexes. It remains
        /// consistent with this map's read-only per-hex assignments.
        /// Empty cells may be excluded by the partitioner's policy. Each cell's
        /// <see cref="VoronoiCell.SiteIndex"/> is its zero-based index in this list, without gaps.
        /// </remarks>
        public IReadOnlyList<VoronoiCell> Cells { get; }

        IReadOnlyList<IHexPartitionCell> IHexPartition.Cells => Cells;

        /// <summary>
        /// Creates a new mutable caller-owned hex map of cell identifiers copied from this partition map.
        /// </summary>
        /// <returns>
        /// A new mutable hex map of cell identifiers. Mutating the returned map does not affect this partition map or
        /// the <see cref="Cells"/> semantic result.
        /// </returns>
        public HexMap<int> ToMutableHexMap()
        {
            return new HexMap<int>(Topology, CopyAssignments(_assignments));
        }

        private static int[] CopyAssignments(int[] assignments)
        {
            var copy = new int[assignments.Length];
            Array.Copy(assignments, copy, assignments.Length);
            return copy;
        }

        private static VoronoiCell[] CopyCells(VoronoiCell[] cells)
        {
            var copy = new VoronoiCell[cells.Length];
            Array.Copy(cells, copy, cells.Length);
            return copy;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;
    }
}
