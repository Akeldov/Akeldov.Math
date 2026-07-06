using Akeldov.Math.Graphs;
using System;
using System.Globalization;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Represents a directed edge between two adjacent Voronoi cells.
    /// </summary>
    public sealed class VoronoiCellEdge : IDirectedEdge<VoronoiCell, VoronoiCellEdge>, IEquatable<VoronoiCellEdge>
    {
        /// <summary>
        /// Initializes a new directed Voronoi cell edge.
        /// </summary>
        /// <param name="fromVertex">The source Voronoi cell.</param>
        /// <param name="toVertex">The target Voronoi cell.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fromVertex"/> or <paramref name="toVertex"/> is null.
        /// </exception>
        public VoronoiCellEdge(VoronoiCell fromVertex, VoronoiCell toVertex)
        {
            FromVertex = fromVertex ?? throw new ArgumentNullException(nameof(fromVertex));
            ToVertex = toVertex ?? throw new ArgumentNullException(nameof(toVertex));
        }

        /// <summary>
        /// Gets the source Voronoi cell.
        /// </summary>
        public VoronoiCell FromVertex { get; }

        /// <summary>
        /// Gets the target Voronoi cell.
        /// </summary>
        public VoronoiCell ToVertex { get; }

        /// <inheritdoc/>
        public VoronoiCell FirstVertex => FromVertex;

        /// <inheritdoc/>
        public VoronoiCell SecondVertex => ToVertex;

        /// <summary>
        /// Indicates whether this edge has the same source and target cells as another edge.
        /// </summary>
        /// <param name="other">The edge to compare with this edge.</param>
        /// <returns><see langword="true"/> if both edges are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VoronoiCellEdge? other) =>
            other != null &&
            FromVertex.Equals(other.FromVertex) &&
            ToVertex.Equals(other.ToVertex);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VoronoiCellEdge other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(FromVertex, ToVertex);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "VoronoiCellEdge(fromSiteIndex: {0}, toSiteIndex: {1})",
                FromVertex.SiteIndex,
                ToVertex.SiteIndex);
    }
}
