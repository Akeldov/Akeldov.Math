using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Represents a rectangular region with arbitrary orientation in two-dimensional space.
    /// </summary>
    public readonly struct OrientedRectangle : IRegion, IEquatable<OrientedRectangle>
    {
        private readonly VectorXY _axisX;
        private readonly VectorXY _axisY;

        /// <summary>
        /// Initializes a new oriented rectangle.
        /// </summary>
        /// <param name="center">The rectangle center.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="center"/>, <paramref name="size"/>, or <paramref name="rotation"/>
        /// contains a non-finite value, or when any size component is not positive.
        /// </exception>
        public OrientedRectangle(PointXY center, VectorXY size, float rotation)
        {
            PointXYValidation.ThrowIfNotFinite(
                center,
                nameof(center),
                "Rectangle center coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            GeometryConstants.ValidateFiniteAngle(rotation, nameof(rotation));

            Center = center;
            Size = size;
            Rotation = rotation;

            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);
            _axisX = new VectorXY(cos, sin);
            _axisY = new VectorXY(-sin, cos);
        }

        /// <summary>
        /// Gets the rectangle center.
        /// </summary>
        public PointXY Center { get; }

        /// <summary>
        /// Gets the rectangle size along its local X and Y axes.
        /// </summary>
        public VectorXY Size { get; }

        /// <summary>
        /// Gets the counterclockwise rotation of the local X axis, in radians.
        /// </summary>
        public float Rotation { get; }

        /// <summary>
        /// Gets the rectangle width along its local X axis.
        /// </summary>
        public float Width => Size.X;

        /// <summary>
        /// Gets the rectangle height along its local Y axis.
        /// </summary>
        public float Height => Size.Y;

        /// <summary>
        /// Gets the unit vector of the rectangle local X axis.
        /// </summary>
        public VectorXY AxisX => _axisX;

        /// <summary>
        /// Gets the unit vector of the rectangle local Y axis.
        /// </summary>
        public VectorXY AxisY => _axisY;

        /// <summary>
        /// Gets the bottom-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomLeft => Center - AxisX * (Width * 0.5f) - AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the bottom-right corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY BottomRight => Center + AxisX * (Width * 0.5f) - AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the top-left corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY TopLeft => Center - AxisX * (Width * 0.5f) + AxisY * (Height * 0.5f);

        /// <summary>
        /// Gets the top-right corner in the rectangle local coordinate system.
        /// </summary>
        public PointXY TopRight => Center + AxisX * (Width * 0.5f) + AxisY * (Height * 0.5f);

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

            VectorXY centered = point - Center;
            float localX = VectorXY.Dot(centered, AxisX);
            float localY = VectorXY.Dot(centered, AxisY);

            return localX >= -Width * 0.5f - geometryEpsilon &&
                localX <= Width * 0.5f + geometryEpsilon &&
                localY >= -Height * 0.5f - geometryEpsilon &&
                localY <= Height * 0.5f + geometryEpsilon;
        }

        /// <summary>
        /// Returns the point coordinates in this rectangle's local coordinate system, relative to the center.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <returns>The point coordinates along the local axes, relative to <see cref="Center"/>.</returns>
        public VectorXY GetCenteredLocalCoordinates(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY centered = point - Center;
            return new VectorXY(
                VectorXY.Dot(centered, AxisX),
                VectorXY.Dot(centered, AxisY));
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
        /// Creates an oriented rectangle from its bottom-left corner, size, and rotation.
        /// </summary>
        /// <param name="bottomLeft">The bottom-left corner in the rectangle local coordinate system.</param>
        /// <param name="size">The rectangle size along its local X and Y axes.</param>
        /// <param name="rotation">The counterclockwise rotation of the local X axis, in radians.</param>
        /// <returns>The oriented rectangle.</returns>
        public static OrientedRectangle FromBottomLeft(PointXY bottomLeft, VectorXY size, float rotation)
        {
            PointXYValidation.ThrowIfNotFinite(
                bottomLeft,
                nameof(bottomLeft),
                "Rectangle corner coordinates must be finite.");

            if (!size.IsFinite || size.X <= 0f || size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Rectangle size components must be finite and positive.");

            GeometryConstants.ValidateFiniteAngle(rotation, nameof(rotation));

            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);
            var axisX = new VectorXY(cos, sin);
            var axisY = new VectorXY(-sin, cos);
            PointXY center = bottomLeft + axisX * (size.X * 0.5f) + axisY * (size.Y * 0.5f);

            return new OrientedRectangle(center, size, rotation);
        }

        /// <summary>
        /// Indicates whether this rectangle has the same center, size, and rotation as another rectangle.
        /// </summary>
        /// <param name="other">The rectangle to compare with this rectangle.</param>
        /// <returns><see langword="true"/> if both rectangles are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(OrientedRectangle other) =>
            Center.Equals(other.Center) &&
            Size.Equals(other.Size) &&
            Rotation.Equals(other.Rotation);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is OrientedRectangle other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Size, Rotation);

        /// <inheritdoc/>
        public override string ToString() => $"OrientedRectangle(center: {Center}, size: {Size}, rotation: {Rotation} rad)";

        /// <summary>
        /// Indicates whether two oriented rectangles are equal.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><see langword="true"/> if both rectangles are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(OrientedRectangle left, OrientedRectangle right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two oriented rectangles are different.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><see langword="true"/> if the rectangles are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(OrientedRectangle left, OrientedRectangle right) => !left.Equals(right);
    }
}
