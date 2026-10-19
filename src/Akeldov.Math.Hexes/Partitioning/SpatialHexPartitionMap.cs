using Akeldov.Math.Hexes.Geometry;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Represents a hex-grid partition with spatial geometry, a read-only snapshot of cells,
    /// and mutable per-hex cell identifiers.
    /// </summary>
    /// <typeparam name="THexPartitionCell">The type of cells retained by the partition.</typeparam>
    /// <remarks>
    /// The input collection is copied in its original order. Cell objects are retained as-is.
    /// Inherited indexer setters change only the identifier map; they do not update the retained cells
    /// or their hex indexes. Callers must keep map assignments consistent with the retained cells.
    /// </remarks>
    public class SpatialHexPartitionMap<THexPartitionCell> : HexPartitionMap<THexPartitionCell>, ISpatialIntHexMap
        where THexPartitionCell : IHexPartitionCell
    {
        /// <summary>
        /// Initializes a new spatial partition with a read-only copy of the supplied cells and their assignments.
        /// </summary>
        /// <param name="geometry">The spatial geometry containing the partition's hex indexes.</param>
        /// <param name="cells">The partition cells to retain in the copied collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cells"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="cells"/> contains a null cell, a negative or duplicate identifier,
        /// null hex indexes, an index outside the geometry topology, a hex assigned to different cells,
        /// or does not cover every hex in the geometry topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialHexPartitionMap(HexMapGeometry geometry, IReadOnlyList<THexPartitionCell> cells)
            : base(geometry.Topology, cells)
        {
            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            Geometry = geometry;
        }

        /// <summary>
        /// Gets the spatial geometry of the partition. Its topology equals this map's topology.
        /// </summary>
        public HexMapGeometry Geometry { get; }
    }
}
