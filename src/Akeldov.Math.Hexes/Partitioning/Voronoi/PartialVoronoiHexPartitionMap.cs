using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA2201 // Hex map indexers use IndexOutOfRangeException for out-of-bounds indexes.
#pragma warning disable MA0012 // Preserve the established hex-map indexer exception behavior.

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Stores nullable Voronoi cell identifiers for a hex center map.
    /// </summary>
    /// <remarks>
    /// The initial assignments and read-only cell collection represent the partitioner's result.
    /// Hexes included by the participation mask initially receive a Voronoi cell identifier;
    /// excluded hexes initially contain null. Initial identifiers equal
    /// <see cref="VoronoiHexPartitionCell.SiteIndex"/> and index <see cref="PartialHexPartitionMap{VoronoiHexPartitionCell}.Cells"/>.
    /// Inherited indexer setters change only the identifier map; they do not update the retained cells,
    /// their hex indexes, or the original participation mask.
    /// Use <see cref="ToMutableHexMap"/> to create an independent mutable copy of the assignments.
    /// Empty cells may be excluded by the partitioner's policy.
    /// Cells created for isolated exclaves follow the source cells in row-major component order.
    /// </remarks>
    public sealed class PartialVoronoiHexPartitionMap : PartialHexPartitionMap<VoronoiHexPartitionCell>, ISpatialHexMap<int?>
    {
        private readonly bool[] _participationMask;

        internal PartialVoronoiHexPartitionMap(
            HexCenterMap centers,
            int?[] assignments,
            VoronoiHexPartitionCell[] cells,
            bool[] participationMask)
            : base((centers ?? throw new ArgumentNullException(nameof(centers))).Topology, cells)
        {
            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));

            if (participationMask == null)
                throw new ArgumentNullException(nameof(participationMask));

            if (assignments.Length != Topology.Count)
                throw new ArgumentException("Assignment count must match center map dimensions.", nameof(assignments));

            if (participationMask.Length != Topology.Count)
                throw new ArgumentException("Participation mask count must match center map dimensions.", nameof(participationMask));

            for (int i = 0; i < assignments.Length; i++)
            {
                if (participationMask[i] && assignments[i] == null)
                    throw new ArgumentException("Participating hex assignments must be non-null.", nameof(assignments));

                if (!participationMask[i] && assignments[i] != null)
                    throw new ArgumentException("Excluded hex assignments must be null.", nameof(assignments));

                if (assignments[i] != this[i])
                    throw new ArgumentException($"Assignment at flat index {i} must match the cell's hex indexes.", nameof(assignments));
            }

            Centers = centers;
            _participationMask = CopyParticipationMask(participationMask);
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
        /// Creates a spatial facade sharing this partition map's geometry and nullable assignment array.
        /// </summary>
        /// <param name="map">The source partition map, or <see langword="null"/>.</param>
        /// <returns>
        /// A new mutable spatial facade sharing the source map's nullable assignments, or <see langword="null"/>
        /// if <paramref name="map"/> is null. Changes through either map are visible through the other.
        /// </returns>
        /// <remarks>
        /// The conversion takes constant time and allocates only the facade object. Assignments are not copied.
        /// Changes through the facade do not update the partition's retained cells, their hex indexes,
        /// or the original participation mask.
        /// Use <see cref="ToMutableHexMap"/> to create an independent mutable copy.
        /// </remarks>
        [return: NotNullIfNotNull("map")]
        public static implicit operator SpatialHexMap<int?>?(PartialVoronoiHexPartitionMap? map)
        {
            return map is null ? null : new SpatialHexMap<int?>(map.Geometry, map.BackingValues);
        }

        /// <summary>
        /// Returns whether the specified hex index was included by the original participation mask.
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
        /// Returns whether the specified flat hex index was included by the original participation mask.
        /// </summary>
        /// <param name="index">The zero-based flat hex index.</param>
        public bool Participates(int index) => _participationMask[index];

        /// <summary>
        /// Creates a new mutable caller-owned hex map of nullable cell identifiers copied from this partition map.
        /// </summary>
        /// <returns>
        /// A new mutable hex map of nullable cell identifiers. Mutating the returned map does not affect this partition map or
        /// the <see cref="PartialHexPartitionMap{VoronoiHexPartitionCell}.Cells"/> semantic result.
        /// </returns>
        public HexMap<int?> ToMutableHexMap()
        {
            var assignments = new int?[Topology.Count];
            for (int i = 0; i < assignments.Length; i++)
                assignments[i] = this[i];

            return new HexMap<int?>(Topology, assignments);
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

        private static bool[] CopyParticipationMask(bool[] participationMask)
        {
            var copy = new bool[participationMask.Length];
            Array.Copy(participationMask, copy, participationMask.Length);
            return copy;
        }

        private int GetFlatIndex(VectorXYInt index) => index.Y * Topology.Resolution.X + index.X;
    }
}
