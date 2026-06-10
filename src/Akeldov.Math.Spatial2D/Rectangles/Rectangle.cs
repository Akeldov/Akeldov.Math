using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents an axis-aligned rectangular region in two-dimensional space.
    /// </summary>
    public readonly struct Rectangle : IRegion, IEquatable<Rectangle>
    {
        /// <summary>
        /// Initializes a new axis-aligned rectangle from two opposite corners.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any corner coordinate is not finite, or when the rectangle width or height is zero.
        /// </exception>
        public Rectangle(PointXY cornerA, PointXY cornerB)
        {
            PointXYValidation.ThrowIfNotFinite(
                cornerA,
                nameof(cornerA),
                "Rectangle corner coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                cornerB,
                nameof(cornerB),
                "Rectangle corner coordinates must be finite.");

            PointXY min = new PointXY(MathF.Min(cornerA.X, cornerB.X), MathF.Min(cornerA.Y, cornerB.Y));
            PointXY max = new PointXY(MathF.Max(cornerA.X, cornerB.X), MathF.Max(cornerA.Y, cornerB.Y));

            if (max.X - min.X <= 0f || max.Y - min.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cornerB), cornerB, "Rectangle width and height must be positive.");

            Min = min;
            Max = max;
        }

        /// <summary>
        /// Gets the corner with the minimum X and Y coordinates.
        /// </summary>
        public PointXY Min { get; }

        /// <summary>
        /// Gets the corner with the maximum X and Y coordinates.
        /// </summary>
        public PointXY Max { get; }

        /// <summary>
        /// Gets the rectangle width.
        /// </summary>
        public float Width => Max.X - Min.X;

        /// <summary>
        /// Gets the rectangle height.
        /// </summary>
        public float Height => Max.Y - Min.Y;

        /// <summary>
        /// Gets the rectangle size.
        /// </summary>
        public VectorXY Size => Max - Min;

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center => Min + Size * 0.5f;

        /// <summary>
        /// Gets the bottom-left corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomLeft => Min;

        /// <summary>
        /// Gets the bottom-right corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomRight => new PointXY(Max.X, Min.Y);

        /// <summary>
        /// Gets the top-left corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopLeft => new PointXY(Min.X, Max.Y);

        /// <summary>
        /// Gets the top-right corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopRight => Max;

        /// <inheritdoc/>
        public FillRule FillRule => FillRule.EvenOdd;

        /// <inheritdoc/>
        public IReadOnlyList<IContour> Contours => Array.AsReadOnly(new IContour[] { ToContour() });

        /// <inheritdoc/>
        public bool Contains(
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            return point.X >= Min.X - geometryEpsilon &&
                point.X <= Max.X + geometryEpsilon &&
                point.Y >= Min.Y - geometryEpsilon &&
                point.Y <= Max.Y + geometryEpsilon;
        }

        /// <summary>
        /// Creates a closed contour representing this rectangle boundary.
        /// </summary>
        /// <returns>The rectangle boundary contour.</returns>
        public Contour ToContour()
        {
            return new Contour(new IFinitePath[]
            {
                new ParameterizedSegment(BottomLeft, BottomRight),
                new ParameterizedSegment(BottomRight, TopRight),
                new ParameterizedSegment(TopRight, TopLeft),
                new ParameterizedSegment(TopLeft, BottomLeft)
            });
        }

        /// <summary>
        /// Creates a contour-based region representing this rectangle.
        /// </summary>
        /// <returns>The rectangle as a contour-based region.</returns>
        public Region ToRegion()
        {
            return new Region(new IContour[] { ToContour() });
        }

        /// <summary>
        /// Indicates whether this rectangle has the same corners as another rectangle.
        /// </summary>
        /// <param name="other">The rectangle to compare with this rectangle.</param>
        /// <returns><see langword="true"/> if both rectangles have equal corners; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Rectangle other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Rectangle other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Min, Max);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "Rectangle({0}, {1})", Min, Max);

        /// <summary>
        /// Indicates whether two rectangles are equal.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><see langword="true"/> if both rectangles are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two rectangles are different.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><see langword="true"/> if the rectangles are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);
    }
}
