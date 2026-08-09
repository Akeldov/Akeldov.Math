using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents an axis-aligned subrectangle of a source rectangle using normalized coordinates.
    /// The subrectangle's axes are aligned with the source rectangle's axes, and all coordinates
    /// are relative to the source rectangle in the inclusive range [0, 1].
    /// </summary>
    public readonly struct NormalizedSubrectangle : IEquatable<NormalizedSubrectangle>
    {
        /// <summary>
        /// Initializes a new subrectangle of a source rectangle.
        /// </summary>
        /// <param name="cornerA">
        /// The first corner of the subrectangle, normalized relative to the source rectangle.
        /// Each coordinate must be in the inclusive range [0, 1].
        /// </param>
        /// <param name="cornerB">
        /// The opposite corner of the subrectangle, normalized relative to the source rectangle.
        /// Each coordinate must be in the inclusive range [0, 1].
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any normalized coordinate is outside the inclusive range [0, 1].
        /// </exception>
        public NormalizedSubrectangle(PointXY cornerA, PointXY cornerB)
        {
            ThrowIfNotNormalized(cornerA, nameof(cornerA));
            ThrowIfNotNormalized(cornerB, nameof(cornerB));

            Min = new PointXY(MathF.Min(cornerA.X, cornerB.X), MathF.Min(cornerA.Y, cornerB.Y));
            Max = new PointXY(MathF.Max(cornerA.X, cornerB.X), MathF.Max(cornerA.Y, cornerB.Y));
        }

        /// <summary>
        /// Gets the corner with the minimum X and Y coordinates, normalized relative to the source rectangle.
        /// </summary>
        public PointXY Min { get; }

        /// <summary>
        /// Gets the corner with the maximum X and Y coordinates, normalized relative to the source rectangle.
        /// </summary>
        public PointXY Max { get; }

        /// <summary>
        /// Gets the bottom-left corner, normalized relative to the source rectangle.
        /// Bottom means the smaller Y coordinate in the source rectangle coordinate system.
        /// </summary>
        public PointXY BottomLeft => Min;

        /// <summary>
        /// Gets the bottom-right corner, normalized relative to the source rectangle.
        /// Bottom means the smaller Y coordinate in the source rectangle coordinate system.
        /// </summary>
        public PointXY BottomRight => new PointXY(Max.X, Min.Y);

        /// <summary>
        /// Gets the top-left corner, normalized relative to the source rectangle.
        /// Top means the greater Y coordinate in the source rectangle coordinate system.
        /// </summary>
        public PointXY TopLeft => new PointXY(Min.X, Max.Y);

        /// <summary>
        /// Gets the top-right corner, normalized relative to the source rectangle.
        /// Top means the greater Y coordinate in the source rectangle coordinate system.
        /// </summary>
        public PointXY TopRight => Max;

        /// <summary>
        /// Gets the center point, normalized relative to the source rectangle.
        /// </summary>
        public PointXY Center => Min + Size * 0.5f;

        /// <summary>
        /// Gets the size of this subrectangle in normalized units.
        /// </summary>
        public VectorXY Size => Max - Min;

        /// <summary>
        /// Gets the full larger rectangle in normalized coordinates.
        /// </summary>
        public static NormalizedSubrectangle Full =>
            new NormalizedSubrectangle(new PointXY(0f, 0f), new PointXY(1f, 1f));

        /// <summary>
        /// Indicates whether this subrectangle contains the specified normalized point.
        /// </summary>
        /// <param name="point">
        /// The point in normalized coordinates relative to the source rectangle.
        /// Each coordinate must be in the inclusive range [0, 1].
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> is inside this subrectangle,
        /// including its boundary; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any normalized coordinate is outside the inclusive range [0, 1].
        /// </exception>
        public bool Contains(PointXY point)
        {
            ThrowIfNotNormalized(point, nameof(point));

            return point.X >= Min.X && point.X <= Max.X &&
                point.Y >= Min.Y && point.Y <= Max.Y;
        }

        /// <summary>
        /// Deconstructs this subrectangle into its normalized corners.
        /// </summary>
        /// <param name="min">
        /// The corner with the minimum X and Y coordinates, normalized relative to the source rectangle.
        /// </param>
        /// <param name="max">
        /// The corner with the maximum X and Y coordinates, normalized relative to the source rectangle.
        /// </param>
        public void Deconstruct(out PointXY min, out PointXY max)
        {
            min = Min;
            max = Max;
        }

        /// <summary>
        /// Indicates whether this subrectangle has the same normalized corners as another subrectangle.
        /// </summary>
        /// <param name="other">The subrectangle to compare with this subrectangle.</param>
        /// <returns><see langword="true"/> if both normalized corners are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(NormalizedSubrectangle other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is NormalizedSubrectangle other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Min, Max);

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}, {1}]", Min, Max);
        }

        /// <summary>
        /// Indicates whether two subrectangles have equal normalized corners.
        /// </summary>
        /// <param name="left">The first subrectangle.</param>
        /// <param name="right">The second subrectangle.</param>
        /// <returns><see langword="true"/> if both normalized corners are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(NormalizedSubrectangle left, NormalizedSubrectangle right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two subrectangles have different normalized corners.
        /// </summary>
        /// <param name="left">The first subrectangle.</param>
        /// <param name="right">The second subrectangle.</param>
        /// <returns><see langword="true"/> if any normalized corner differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(NormalizedSubrectangle left, NormalizedSubrectangle right) => !left.Equals(right);

        private static void ThrowIfNotNormalized(PointXY point, string parameterName)
        {
            if (point.X < 0f || point.X > 1f || float.IsNaN(point.X) || float.IsInfinity(point.X))
                throw new ArgumentOutOfRangeException(parameterName, point, "Normalized point coordinates must be in the inclusive range [0, 1].");

            if (point.Y < 0f || point.Y > 1f || float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(parameterName, point, "Normalized point coordinates must be in the inclusive range [0, 1].");
        }
    }
}
