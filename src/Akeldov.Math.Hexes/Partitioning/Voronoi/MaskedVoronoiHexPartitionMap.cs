using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable CA2201 // Hex map indexers use IndexOutOfRangeException for out-of-bounds indexes.
#pragma warning disable MA0012 // Preserve the established hex-map indexer exception behavior.

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores masked Voronoi cell assignments for a hex center map.
    /// </summary>
    /// <remarks>
    /// The map is a read-only semantic result produced by the partitioner. Hexes included by the
    /// participation mask receive a Voronoi cell assignment; excluded hexes return
    /// <see langword="null"/>. Per-hex assignments and <see cref="Cells"/> are kept consistent
    /// with the original partition result. Use <see cref="ToMutableHexMap"/> to create a new
    /// mutable caller-owned copy of the assignments.
    /// </remarks>
    public sealed class MaskedVoronoiHexPartitionMap : ISpatialHexMap<VoronoiCell?>
    {
        private readonly VoronoiCell?[] _assignments;
        private readonly bool[] _participationMask;

        internal MaskedVoronoiHexPartitionMap(
            HexCenterMap centers,
            VoronoiCell?[] assignments,
            VoronoiCell[] cells,
            bool[] participationMask)
        {
            Centers = centers ?? throw new ArgumentNullException(nameof(centers));

            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));

            int count = centers.Topology.Count;
            if (assignments.Length != count)
                throw new ArgumentException("Assignment count must match center map dimensions.", nameof(assignments));

            if (participationMask.Length != count)
                throw new ArgumentException("Participation mask count must match center map dimensions.", nameof(participationMask));

            for (int i = 0; i < assignments.Length; i++)
            {
                if (participationMask[i] && assignments[i] == null)
                    throw new ArgumentException("Participating hex assignments must be non-null.", nameof(assignments));

                if (!participationMask[i] && assignments[i] != null)
                    throw new ArgumentException("Excluded hex assignments must be null.", nameof(assignments));
            }

            Topology = centers.Topology;
            _assignments = CopyAssignments(assignments);
            _participationMask = CopyParticipationMask(participationMask);
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
        /// Gets the Voronoi cell assigned to the specified participating hex index, or
        /// <see langword="null"/> when the hex was excluded by the participation mask.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public VoronoiCell? this[VectorXYInt index]
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
        /// Gets the Voronoi cell assigned to the specified participating flat hex index, or
        /// <see langword="null"/> when the hex was excluded by the participation mask.
        /// </summary>
        /// <param name="index">The zero-based flat hex index.</param>
        public VoronoiCell? this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _assignments[index];
        }

        /// <summary>
        /// Gets the read-only semantic result of Voronoi cells, one per source site.
        /// </summary>
        /// <remarks>
        /// This list represents the partitioner's cells and their grouped participating hex indexes.
        /// It remains consistent with this map's read-only per-hex assignments.
        /// </remarks>
        public IReadOnlyList<VoronoiCell> Cells { get; }

        /// <summary>
        /// Returns whether the specified hex index was included by the participation mask.
        /// </summary>
        /// <param name="index">The X/Y coordinates of the hex cell.</param>
        public bool Participates(VectorXYInt index)
        {
            if (index.X < 0 || index.X >= Topology.Resolution.X ||
                index.Y < 0 || index.Y >= Topology.Resolution.Y)
                throw new IndexOutOfRangeException($"Hex index out of bounds: {index}");

            return _participationMask[GetFlatIndex(index)];
        }

        /// <summary>
        /// Returns whether the specified flat hex index was included by the participation mask.
        /// </summary>
        /// <param name="index">The zero-based flat hex index.</param>
        public bool Participates(int index) => _participationMask[index];

        /// <summary>
        /// Creates a new mutable caller-owned hex map initialized from this partition map's assignments.
        /// </summary>
        /// <returns>
        /// A new mutable hex map. Mutating the returned map does not affect this partition map or
        /// the <see cref="Cells"/> semantic result.
        /// </returns>
        public HexMap<VoronoiCell?> ToMutableHexMap()
        {
            return new HexMap<VoronoiCell?>(Topology, CopyAssignments(_assignments));
        }

        /// <summary>
        /// Creates a new mutable caller-owned Boolean mask from the participating hexes.
        /// </summary>
        /// <returns>
        /// A new mutable Boolean hex map whose <see langword="true"/> cells are the hexes included
        /// by the original participation mask.
        /// </returns>
        public BoolHexMap ToMutableParticipationMask()
        {
            return new BoolHexMap(Topology, CopyParticipationMask(_participationMask));
        }

        private static VoronoiCell?[] CopyAssignments(VoronoiCell?[] assignments)
        {
            var copy = new VoronoiCell?[assignments.Length];
            Array.Copy(assignments, copy, assignments.Length);
            return copy;
        }

        private static bool[] CopyParticipationMask(bool[] participationMask)
        {
            var copy = new bool[participationMask.Length];
            Array.Copy(participationMask, copy, participationMask.Length);
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
