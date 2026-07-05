using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Akeldov.Math.Spatial2D.Partitioning.Voronoi
{
    /// <summary>
    /// Represents a weighted Voronoi site.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Site : IEquatable<Site>, IHasPosition2D
    {
        /// <summary>
        /// Initializes a new weighted Voronoi site.
        /// </summary>
        /// <param name="position">The site position.</param>
        /// <param name="weight">The non-negative site weight.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="weight"/> is negative or NaN.
        /// </exception>
        public Site(PointXY position, float weight)
        {
            PointXYValidation.ThrowIfNotFinite(
                position,
                nameof(position),
                "Site position coordinates must be finite.");

            if (weight < 0f || float.IsNaN(weight))
                throw new ArgumentOutOfRangeException(nameof(weight), "Site weight must be non-negative and not NaN.");

            Position = position;
            Weight = weight;
        }

        /// <summary>
        /// Gets the site position.
        /// </summary>
        public PointXY Position { get; }

        /// <summary>
        /// Gets the site weight used by weighted-distance comparison.
        /// </summary>
        public float Weight { get; }

        /// <summary>
        /// Indicates whether this site has the same point and weight as another site.
        /// </summary>
        /// <param name="other">The site to compare with this site.</param>
        /// <returns><see langword="true"/> if both sites are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Site other) => Position.Equals(other.Position) && Weight.Equals(other.Weight);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Site other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Position, Weight);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1})", Position, Weight);

        /// <summary>
        /// Indicates whether two sites are equal.
        /// </summary>
        /// <param name="left">The first site.</param>
        /// <param name="right">The second site.</param>
        /// <returns><see langword="true"/> if the sites are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Site left, Site right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two sites are different.
        /// </summary>
        /// <param name="left">The first site.</param>
        /// <param name="right">The second site.</param>
        /// <returns><see langword="true"/> if the sites are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Site left, Site right) => !left.Equals(right);
    }
}
