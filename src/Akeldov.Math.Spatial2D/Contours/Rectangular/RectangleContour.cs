using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents the closed boundary contour of an axis-aligned rectangle.
    /// </summary>
    public readonly struct RectangleContour : IContour, IEquatable<RectangleContour>
    {
        private readonly ParameterizedRectangleContour _contour;

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from two opposite corners.
        /// </summary>
        /// <param name="cornerA">The first rectangle corner.</param>
        /// <param name="cornerB">The opposite rectangle corner.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any corner coordinate is not finite, or when the rectangle width or height is zero.
        /// </exception>
        public RectangleContour(PointXY cornerA, PointXY cornerB)
        {
            _contour = new ParameterizedRectangleContour(cornerA, cornerB);
        }

        /// <summary>
        /// Initializes a new axis-aligned rectangular contour from the specified rectangular region.
        /// </summary>
        /// <param name="rectangle">The rectangular region whose boundary this contour represents.</param>
        public RectangleContour(Rectangle rectangle)
        {
            _contour = new ParameterizedRectangleContour(rectangle);
        }

        /// <summary>
        /// Gets the rectangular region bounded by this contour.
        /// </summary>
        public Rectangle Rectangle => ToRegion();

        /// <summary>
        /// Gets the corner with the minimum X and Y coordinates.
        /// </summary>
        public PointXY Min => _contour.Min;

        /// <summary>
        /// Gets the corner with the maximum X and Y coordinates.
        /// </summary>
        public PointXY Max => _contour.Max;

        /// <summary>
        /// Gets the rectangle width.
        /// </summary>
        public float Width => _contour.Width;

        /// <summary>
        /// Gets the rectangle height.
        /// </summary>
        public float Height => _contour.Height;

        /// <summary>
        /// Gets the rectangle size.
        /// </summary>
        public VectorXY Size => _contour.Size;

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center => _contour.Center;

        /// <summary>
        /// Gets the bottom-left corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomLeft => _contour.BottomLeft;

        /// <summary>
        /// Gets the bottom-right corner.
        /// Bottom means the smaller Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY BottomRight => _contour.BottomRight;

        /// <summary>
        /// Gets the top-left corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopLeft => _contour.TopLeft;

        /// <summary>
        /// Gets the top-right corner.
        /// Top means the greater Y coordinate in the rectangle coordinate system.
        /// </summary>
        public PointXY TopRight => _contour.TopRight;

        /// <summary>
        /// Gets the rectangle perimeter length.
        /// </summary>
        public float Length => _contour.Length;

        /// <inheritdoc/>
        public bool Encloses(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.Encloses(point, geometryEpsilon);
        }

        /// <inheritdoc/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.GetRayIntersections(ray, geometryEpsilon);
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            return _contour.Project(point);
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            return _contour.Distance(point);
        }

        /// <inheritdoc/>
        public float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _contour.SignedDistance(point, geometryEpsilon);
        }

        /// <summary>
        /// Creates a rectangular region bounded by this contour.
        /// </summary>
        /// <returns>The rectangular region bounded by this contour.</returns>
        public Rectangle ToRegion()
        {
            return _contour.ToRegion();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is RectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RectangleContour other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Min, Max);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "RectangleContour({0}, {1})", Min, Max);

        /// <summary>
        /// Converts a rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The rectangular contour to convert.</param>
        public static explicit operator Rectangle(RectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts a rectangular contour to a parameterized rectangular contour.
        /// </summary>
        /// <param name="contour">The rectangular contour to convert.</param>
        public static explicit operator ParameterizedRectangleContour(RectangleContour contour)
        {
            return contour._contour;
        }

        /// <summary>
        /// Indicates whether two rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(RectangleContour left, RectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(RectangleContour left, RectangleContour right) => !left.Equals(right);
    }
}
