using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Hexes.Partitioning.Voronoi
{
    /// <summary>
    /// Represents a Voronoi cell associated with one weighted site.
    /// </summary>
    public sealed class VoronoiCell : IEquatable<VoronoiCell>
    {
        /// <summary>
        /// Initializes a new Voronoi cell.
        /// </summary>
        /// <param name="siteIndex">The zero-based index of the source site.</param>
        /// <param name="site">The weighted site represented by this cell.</param>
        /// <param name="hexIndexes">The hex indexes assigned to this cell.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="siteIndex"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexIndexes"/> is null.
        /// </exception>
        public VoronoiCell(int siteIndex, Site site, IReadOnlyList<VectorXYInt> hexIndexes)
        {
            if (siteIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(siteIndex));

            SiteIndex = siteIndex;
            Site = site;
            HexIndexes = CopyHexIndexes(hexIndexes);
            Adjacents = Array.AsReadOnly(Array.Empty<VoronoiCell>());
            IncomingVertices = Array.AsReadOnly(Array.Empty<VoronoiCell>());
            IncidentEdges = Array.AsReadOnly(Array.Empty<VoronoiCellEdge>());
            IncomingEdges = Array.AsReadOnly(Array.Empty<VoronoiCellEdge>());
            OutgoingEdges = Array.AsReadOnly(Array.Empty<VoronoiCellEdge>());
        }

        /// <summary>
        /// Gets the zero-based index of the source site.
        /// </summary>
        public int SiteIndex { get; }

        /// <summary>
        /// Gets the weighted site represented by this cell.
        /// </summary>
        public Site Site { get; }

        /// <summary>
        /// Gets the center point of this cell, taken from the source site position.
        /// </summary>
        public PointXY Center => Site.Position;

        /// <summary>
        /// Gets the read-only semantic result of hex indexes assigned to this cell.
        /// </summary>
        public IReadOnlyList<VectorXYInt> HexIndexes { get; }

        /// <summary>
        /// Gets the read-only semantic result of outgoing adjacent cells sharing at least one hex edge with this cell.
        /// </summary>
        /// <remarks>
        /// Cells produced by <see cref="VoronoiHexPartitioner"/> reference neighboring cell
        /// instances from the same partition result. Cells constructed directly start with an
        /// empty adjacency list.
        /// </remarks>
        public IReadOnlyList<VoronoiCell> Adjacents { get; private set; }

        /// <summary>
        /// Gets the read-only semantic result of cells with directed edges targeting this cell.
        /// </summary>
        /// <remarks>
        /// Cells produced by <see cref="VoronoiHexPartitioner"/> reference neighboring cell
        /// instances from the same partition graph. Cells constructed directly start with an
        /// empty incoming-vertex list.
        /// </remarks>
        public IReadOnlyList<VoronoiCell> IncomingVertices { get; private set; }

        /// <summary>
        /// Gets the read-only semantic result of cells targeted by directed edges from this cell.
        /// </summary>
        public IReadOnlyList<VoronoiCell> OutgoingVertices => Adjacents;

        /// <summary>
        /// Gets the read-only semantic result of directed edges incident to this cell.
        /// </summary>
        /// <remarks>
        /// Cells produced by <see cref="VoronoiHexPartitioner"/> expose incoming edges followed
        /// by outgoing edges. Cells constructed directly start with an empty list.
        /// </remarks>
        public IReadOnlyList<VoronoiCellEdge> IncidentEdges { get; private set; }

        /// <summary>
        /// Gets the read-only semantic result of directed edges targeting this cell.
        /// </summary>
        /// <remarks>
        /// Cells produced by <see cref="VoronoiHexPartitioner"/> reference edge instances
        /// from the same partition graph. Cells constructed directly start with an empty list.
        /// </remarks>
        public IReadOnlyList<VoronoiCellEdge> IncomingEdges { get; private set; }

        /// <summary>
        /// Gets the read-only semantic result of directed edges originating from this cell.
        /// </summary>
        /// <remarks>
        /// Cells produced by <see cref="VoronoiHexPartitioner"/> reference edge instances
        /// from the same partition graph. Cells constructed directly start with an empty list.
        /// </remarks>
        public IReadOnlyList<VoronoiCellEdge> OutgoingEdges { get; private set; }

        /// <summary>
        /// Indicates whether this cell has the same site index and site as another cell.
        /// </summary>
        /// <param name="other">The cell to compare with this cell.</param>
        /// <returns><see langword="true"/> if both cells are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VoronoiCell? other) =>
            other != null &&
            SiteIndex == other.SiteIndex &&
            Site.Equals(other.Site);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VoronoiCell other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(SiteIndex, Site);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "VoronoiCell(siteIndex: {0}, site: {1}, hexCount: {2})",
                SiteIndex,
                Site,
                HexIndexes.Count);

        private static IReadOnlyList<VectorXYInt> CopyHexIndexes(IReadOnlyList<VectorXYInt> hexIndexes)
        {
            if (hexIndexes == null)
                throw new ArgumentNullException(nameof(hexIndexes));

            var copy = new VectorXYInt[hexIndexes.Count];
            for (int i = 0; i < hexIndexes.Count; i++)
            {
                copy[i] = hexIndexes[i];
            }

            return Array.AsReadOnly(copy);
        }

        internal void SetAdjacents(IReadOnlyList<VoronoiCell> adjacents)
        {
            if (adjacents == null)
                throw new ArgumentNullException(nameof(adjacents));

            var copy = new VoronoiCell[adjacents.Count];
            for (int i = 0; i < adjacents.Count; i++)
            {
                copy[i] = adjacents[i];
            }

            Adjacents = Array.AsReadOnly(copy);
        }

        internal void SetEdges(IReadOnlyList<VoronoiCellEdge> incomingEdges, IReadOnlyList<VoronoiCellEdge> outgoingEdges)
        {
            if (incomingEdges == null)
                throw new ArgumentNullException(nameof(incomingEdges));

            if (outgoingEdges == null)
                throw new ArgumentNullException(nameof(outgoingEdges));

            IncomingEdges = CopyEdges(incomingEdges);
            OutgoingEdges = CopyEdges(outgoingEdges);
            IncidentEdges = CopyIncidentEdges(IncomingEdges, OutgoingEdges);
            IncomingVertices = CopyIncomingVertices(IncomingEdges);
        }

        private static IReadOnlyList<VoronoiCellEdge> CopyEdges(IReadOnlyList<VoronoiCellEdge> edges)
        {
            var copy = new VoronoiCellEdge[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                copy[i] = edges[i];
            }

            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<VoronoiCellEdge> CopyIncidentEdges(
            IReadOnlyList<VoronoiCellEdge> incomingEdges,
            IReadOnlyList<VoronoiCellEdge> outgoingEdges)
        {
            var copy = new VoronoiCellEdge[incomingEdges.Count + outgoingEdges.Count];
            for (int i = 0; i < incomingEdges.Count; i++)
            {
                copy[i] = incomingEdges[i];
            }

            for (int i = 0; i < outgoingEdges.Count; i++)
            {
                copy[incomingEdges.Count + i] = outgoingEdges[i];
            }

            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<VoronoiCell> CopyIncomingVertices(IReadOnlyList<VoronoiCellEdge> incomingEdges)
        {
            var copy = new VoronoiCell[incomingEdges.Count];
            for (int i = 0; i < incomingEdges.Count; i++)
            {
                copy[i] = incomingEdges[i].FromVertex;
            }

            return Array.AsReadOnly(copy);
        }
    }
}
