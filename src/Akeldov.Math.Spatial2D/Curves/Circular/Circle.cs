using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a circle in two-dimensional space.
    /// </summary>
    public readonly struct Circle : IFiniteCurve, IEquatable<Circle>
    {
        private readonly PointXY _center;
        private readonly float _radius;

        /// <summary>
        /// Initializes a new circle with the specified center and radius.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The circle radius.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radius"/> is negative, NaN, or infinite.</exception>
        public Circle(PointXY center, float radius)
        {
            PointXYValidation.ThrowIfNotFinite(
                center,
                nameof(center),
                "Circle center coordinates must be finite.");

            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Circle radius must be finite and non-negative.");

            _center = center;
            _radius = radius;
        }

        /// <summary>
        /// Gets the center of the circle.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the circle radius.
        /// </summary>
        public float Radius => _radius;

        /// <summary>
        /// Gets the circumference length.
        /// </summary>
        public float Length => 2f * MathF.PI * _radius;

        /// <summary>
        /// Returns the shortest distance from the specified point to the circle circumference.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The absolute distance to the circle circumference.</returns>
        public float Distance(PointXY point)
        {
            return Project(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this circle.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this circle.</returns>
        public CurveProjection Project(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = point - _center;

            if (_radius <= GeometryConstants.GeometryEpsilon)
                return new CurveProjection(_center, point.Distance(_center));

            PointXY projected = toPoint.SquaredLength <= GeometryConstants.GeometryEpsilonSquared
                ? _center + new VectorXY(_radius, 0f)
                : _center + toPoint.Normalize() * _radius;

            return new CurveProjection(projected, point.Distance(projected));
        }

        /// <summary>
        /// Returns point intersections between this circle and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with the circle.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            List<PointXY> intersections = new List<PointXY>();

            VectorXY d = ray.Direction;
            VectorXY f = ray.Origin - _center;

            float a = 1f;
            float b = 2 * VectorXY.Dot(f, d);
            float c = f.SquaredLength - _radius * _radius;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < -geometryEpsilon)
            {
                return intersections;
            }

            if (discriminant < 0f)
                discriminant = 0f;

            discriminant = MathF.Sqrt(discriminant);

            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0)
            {
                intersections.AddDistinct(ray.Origin + d * t1, geometryEpsilon);
            }

            if (t2 >= 0 && !t2.AlmostEquals(t1, geometryEpsilon))
            {
                intersections.AddDistinct(ray.Origin + d * t2, geometryEpsilon);
            }

            return intersections;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Circle other && Equals(other);

        /// <summary>
        /// Indicates whether this circle has the same center and radius as another circle.
        /// </summary>
        /// <param name="other">The circle to compare with this circle.</param>
        /// <returns><see langword="true"/> if both circles are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Circle other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Radius);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "Circle(center: {0}, radius: {1})", Center, Radius);

        /// <summary>
        /// Indicates whether two circles are equal.
        /// </summary>
        /// <param name="left">The first circle.</param>
        /// <param name="right">The second circle.</param>
        /// <returns><see langword="true"/> if the circles are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Circle left, Circle right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two circles are different.
        /// </summary>
        /// <param name="left">The first circle.</param>
        /// <param name="right">The second circle.</param>
        /// <returns><see langword="true"/> if the circles are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Circle left, Circle right) => !(left == right);
    }
}
