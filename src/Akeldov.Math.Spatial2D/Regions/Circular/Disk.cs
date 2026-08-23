using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Regions
{
    /// <summary>
    /// Represents a filled circular region in two-dimensional space.
    /// </summary>
    public readonly struct Disk : IRegion, IEquatable<Disk>
    {
        private readonly PointXY _center;
        private readonly float _radius;

        /// <summary>
        /// Initializes a new disk with the specified center and radius.
        /// </summary>
        /// <param name="center">The disk center.</param>
        /// <param name="radius">The disk radius.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/> contains a non-finite coordinate, or when
        /// <paramref name="radius"/> is negative, NaN, or infinite.
        /// </exception>
        public Disk(PointXY center, float radius)
        {
            PointXYValidation.ThrowIfNotFinite(
                center,
                nameof(center),
                "Disk center coordinates must be finite.");

            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Disk radius must be finite and non-negative.");

            _center = center;
            _radius = radius;
        }

        /// <summary>
        /// Gets the disk center.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the disk radius.
        /// </summary>
        public float Radius => _radius;

        /// <summary>
        /// Gets the disk diameter.
        /// </summary>
        public float Diameter => 2f * Radius;

        /// <inheritdoc/>
        public bool Contains(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return point.Distance(Center) <= Radius;
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return MathF.Abs(point.Distance(Center) - Radius);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point)
        {
            float distance = Distance(point);
            return point.Distance(Center) <= Radius ? -distance : distance;
        }

        /// <summary>
        /// Creates a closed contour representing this disk boundary.
        /// </summary>
        /// <returns>The disk boundary contour.</returns>
        public Circle ToContour()
        {
            return new Circle(Center, Radius);
        }

        /// <summary>
        /// Indicates whether this disk has the same center and radius as another disk.
        /// </summary>
        /// <param name="other">The disk to compare with this disk.</param>
        /// <returns><see langword="true"/> if both disks are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Disk other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Disk other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Radius);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "Disk(center: {0}, radius: {1})", Center, Radius);

        /// <summary>
        /// Indicates whether two disks are equal.
        /// </summary>
        /// <param name="left">The first disk.</param>
        /// <param name="right">The second disk.</param>
        /// <returns><see langword="true"/> if both disks are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Disk left, Disk right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two disks are different.
        /// </summary>
        /// <param name="left">The first disk.</param>
        /// <param name="right">The second disk.</param>
        /// <returns><see langword="true"/> if the disks are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Disk left, Disk right) => !left.Equals(right);
    }
}
