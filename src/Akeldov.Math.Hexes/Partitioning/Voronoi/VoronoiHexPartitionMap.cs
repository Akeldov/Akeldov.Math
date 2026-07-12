using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
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
    public sealed class VoronoiHexPartitionMap : IHexMap<VoronoiCell>
    {
        private readonly VoronoiCell[] _assignments;

        internal VoronoiHexPartitionMap(HexCenterMap centers, VoronoiCell[] assignments, VoronoiCell[] cells)
        {
            Centers = centers ?? throw new ArgumentNullException(nameof(centers));

            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            int count = checked(centers.Width * centers.Height);
            if (assignments.Length != count)
                throw new ArgumentException("Assignment count must match center map dimensions.", nameof(assignments));

            Topology = CreateTopology(centers);
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
        public IndexSeptupletMap Topology { get; }

        /// <summary>
        /// Gets the map width in hexes.
        /// </summary>
        public int Width => Topology.Width;

        /// <summary>
        /// Gets the map height in hexes.
        /// </summary>
        public int Height => Topology.Height;

        /// <summary>
        /// Gets the map resolution in hexes.
        /// </summary>
        public VectorXYInt Resolution => new VectorXYInt(Topology.Width, Topology.Height);

        /// <summary>
        /// Gets the hex layout used by the partition map.
        /// </summary>
        public Layout Layout => Topology.Layout;

        /// <summary>
        /// Gets the Voronoi cell assigned to the specified hex index.
        /// </summary>
        /// <param name="index">The zero-based hex index.</param>
        public VoronoiCell this[VectorXYInt index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index.X < 0 || index.X >= Topology.Width ||
                    index.Y < 0 || index.Y >= Topology.Height)
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
            return new HexMap<VoronoiCell>(Width, Height, Layout, CopyAssignments(_assignments));
        }

        private static IndexSeptupletMap CreateTopology(HexCenterMap centers)
        {
            if (centers == null)
                throw new ArgumentNullException(nameof(centers));

            return new IndexSeptupletMap(centers.Width, centers.Height, centers.Layout);
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

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Width + index.X;
    }
}
