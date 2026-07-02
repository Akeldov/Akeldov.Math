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
    }
}
