using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores the Voronoi cell assigned to each hex center.
    /// </summary>
    /// <remarks>
    /// The map is a read-only semantic result produced by the partitioner. Per-hex assignments and
    /// <see cref="Cells"/> are kept consistent with the original partition result. Use
    /// <see cref="ToMutableHexMap"/> to create a new mutable caller-owned copy of the assignments.
    /// </remarks>
    public sealed class VoronoiHexPartitionMap : ISpatialHexMap<VoronoiCell>
    {
        private readonly VoronoiCell[] _assignments;

        internal VoronoiHexPartitionMap(HexCenterMap centers, VoronoiCell[] assignments, VoronoiCell[] cells)
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
        /// Gets the Voronoi cell assigned to the specified hex index.
        /// </summary>
        /// <param name="index">The zero-based hex index.</param>
        public VoronoiCell this[VectorXYInt index]
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
        /// Gets the Voronoi cell assigned to the specified flat hex index.
        /// </summary>
        /// <param name="index">The zero-based flat hex index.</param>
        public VoronoiCell this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _assignments[index];
        }

        /// <summary>
        /// Gets the read-only semantic result of Voronoi cells, one per source site.
        /// </summary>
        /// <remarks>
        /// This list represents the partitioner's cells and their grouped hex indexes. It remains
        /// consistent with this map's read-only per-hex assignments.
        /// </remarks>
        public IReadOnlyList<VoronoiCell> Cells { get; }

        /// <summary>
        /// Creates a new mutable caller-owned hex map initialized from this partition map's assignments.
        /// </summary>
        /// <returns>
        /// A new mutable hex map. Mutating the returned map does not affect this partition map or
        /// the <see cref="Cells"/> semantic result.
        /// </returns>
        public HexMap<VoronoiCell> ToMutableHexMap()
        {
            return new HexMap<VoronoiCell>(Topology, CopyAssignments(_assignments));
        }

        private static VoronoiCell[] CopyAssignments(VoronoiCell[] assignments)
        {
            var copy = new VoronoiCell[assignments.Length];
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
