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
    public sealed class VoronoiCell : HexPartitionCell, IEquatable<VoronoiCell>
    {
        /// <summary>
        /// Initializes a new Voronoi cell.
        /// </summary>
        /// <param name="siteIndex">The zero-based cell index in the partition result.</param>
        /// <param name="site">The weighted site represented by this cell.</param>
        /// <param name="hexIndexes">The hex indexes assigned to this cell, copied into a read-only snapshot.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="siteIndex"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexIndexes"/> is null.
        /// </exception>
        public VoronoiCell(int siteIndex, Site site, IReadOnlyList<VectorXYInt> hexIndexes)
            : base(siteIndex >= 0 ? siteIndex : throw new ArgumentOutOfRangeException(nameof(siteIndex)), hexIndexes)
        {
            Site = site;
        }

        /// <summary>
        /// Gets the zero-based cell index in the partition result.
        /// </summary>
        /// <remarks>
        /// When empty cells are excluded, the partitioner renumbers the remaining cells without gaps.
        /// The index then may differ from the site's index in the original input list.
        /// This value is also the cell's <see cref="HexPartitionCell.Id"/>.
        /// </remarks>
        public int SiteIndex => Id;

        /// <summary>
        /// Gets the weighted site represented by this cell.
        /// </summary>
        public Site Site { get; }

        /// <summary>
        /// Gets the center point of this cell, taken from its site position.
        /// For a cell created from an isolated exclave, the site is a hex center inside that component.
        /// </summary>
        public PointXY Center => Site.Position;

        /// <summary>
        /// Gets the read-only semantic result of hex indexes assigned to this cell.
        /// </summary>
        /// <remarks>
        /// The indexes are stored by the base partition cell as a snapshot of the constructor input.
        /// </remarks>
        public new IReadOnlyList<VectorXYInt> HexIndexes => base.HexIndexes;

        /// <summary>
        /// Indicates whether this cell has the same site index and site as another cell.
        /// </summary>
        /// <param name="other">The cell to compare with this cell.</param>
        /// <returns><see langword="true"/> if both cells are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VoronoiCell? other)
        {
            return other != null &&
                SiteIndex == other.SiteIndex &&
                Site.Equals(other.Site);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is VoronoiCell other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(SiteIndex, Site);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "VoronoiCell(siteIndex: {0}, site: {1}, hexCount: {2})",
                SiteIndex,
                Site,
                HexIndexes.Count);
        }
    }
}
