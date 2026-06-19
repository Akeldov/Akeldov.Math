using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents the closed boundary contour of an oriented rectangle.
    /// </summary>
    public readonly struct OrientedRectangleContour : IContour, IEquatable<OrientedRectangleContour>
    {
        private readonly ParameterizedOrientedRectangleContour _contour;

        /// <summary>
        /// Initializes a new oriented rectangular contour.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, or when any size component is not positive.
        /// </exception>
        public OrientedRectangleContour(PointXY center, VectorXY size, float rotation)
        {
            _contour = new ParameterizedOrientedRectangleContour(center, size, rotation);
        }

        /// <summary>
        /// Initializes a new oriented rectangular contour from the specified rectangular region.
        /// </summary>
        /// <param name="rectangle">The rectangular region whose boundary this contour represents.</param>
        public OrientedRectangleContour(OrientedRectangle rectangle)
        {
            _contour = new ParameterizedOrientedRectangleContour(rectangle);
        }

        /// <summary>
        /// Gets the oriented rectangular region bounded by this contour.
        /// </summary>
        public OrientedRectangle Rectangle => ToRegion();

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center => _contour.Center;

        /// <summary>
        /// Gets the rectangle size along its local X and Y axes.
        /// </summary>
        public VectorXY Size => _contour.Size;

        /// <summary>
        /// Gets the counterclockwise rotation of the local X axis, in radians.
        /// </summary>
        public float Rotation => _contour.Rotation;

        /// <summary>
        /// Gets the rectangle width along its local X axis.
        /// </summary>
        public float Width => _contour.Width;

        /// <summary>
        /// Gets the rectangle height along its local Y axis.
        /// </summary>
        public float Height => _contour.Height;

        /// <summary>
        /// Gets the unit vector of the rectangle local X axis.
        /// </summary>
        public VectorXY AxisX => _contour.AxisX;

        /// <summary>
        /// Gets the unit vector of the rectangle local Y axis.
        /// </summary>
        public VectorXY AxisY => _contour.AxisY;

        /// <summary>
        /// Gets the bottom-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomLeft => _contour.BottomLeft;

        /// <summary>
        /// Gets the bottom-right corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomRight => _contour.BottomRight;

        /// <summary>
        /// Gets the top-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY TopLeft => _contour.TopLeft;

        /// <summary>
        /// Gets the top-right corner in the rectangle local coordinate system.
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
        /// Creates an oriented rectangular region bounded by this contour.
        /// </summary>
        /// <returns>The oriented rectangular region bounded by this contour.</returns>
        public OrientedRectangle ToRegion()
        {
            return _contour.ToRegion();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is OrientedRectangleContour other && Equals(other);

        /// <summary>
        /// Indicates whether this contour has the same rectangle as another contour.
        /// </summary>
        /// <param name="other">The contour to compare with this contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(OrientedRectangleContour other) =>
            Center.Equals(other.Center) &&
            Size.Equals(other.Size) &&
            Rotation.Equals(other.Rotation);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Size, Rotation);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "OrientedRectangleContour(center: {0}, size: {1}, rotation: {2} rad)",
                Center,
                Size,
                Rotation);

        /// <summary>
        /// Converts an oriented rectangular contour to its bounded rectangular region.
        /// </summary>
        /// <param name="contour">The oriented rectangular contour to convert.</param>
        public static explicit operator OrientedRectangle(OrientedRectangleContour contour)
        {
            return contour.Rectangle;
        }

        /// <summary>
        /// Converts an oriented rectangular contour to a parameterized oriented rectangular contour.
        /// </summary>
        /// <param name="contour">The oriented rectangular contour to convert.</param>
        public static explicit operator ParameterizedOrientedRectangleContour(OrientedRectangleContour contour)
        {
            return contour._contour;
        }

        /// <summary>
        /// Indicates whether two oriented rectangular contours are equal.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if both contours are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(OrientedRectangleContour left, OrientedRectangleContour right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two oriented rectangular contours are different.
        /// </summary>
        /// <param name="left">The first contour.</param>
        /// <param name="right">The second contour.</param>
        /// <returns><see langword="true"/> if the contours are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(OrientedRectangleContour left, OrientedRectangleContour right) => !left.Equals(right);
    }
}
