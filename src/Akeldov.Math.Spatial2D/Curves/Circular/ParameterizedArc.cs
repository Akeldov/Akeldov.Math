using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a parameterized circular arc in two-dimensional space.
    /// </summary>
    [Serializable]
    public readonly struct ParameterizedArc : IFinitePath, IEquatable<ParameterizedArc>
    {
        private readonly PointXY _center;
        private readonly float _radius;
        private readonly float _startAngle;
        private readonly float _endAngle;
        private readonly AngularDirection _angularDirection;
        private readonly bool _isFullCircle;

        /// <summary>
        /// Creates a parameterized arc from <paramref name="startAngle"/> to <paramref name="endAngle"/>.
        /// Equal input angles represent a zero-length arc. An end angle one full turn after the start angle
        /// represents a full circle even though both angles normalize to the same value.
        /// </summary>
        /// <param name="center">The center of the source circle.</param>
        /// <param name="radius">The radius of the source circle.</param>
        /// <param name="startAngle">The start angle in radians.</param>
        /// <param name="endAngle">The end angle in radians.</param>
        /// <param name="angularDirection">The direction in which curve coordinates increase.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="radius"/> is negative, NaN, or infinite, when an angle is NaN or infinite,
        /// or when <paramref name="angularDirection"/> is unsupported.
        /// </exception>
        public ParameterizedArc(PointXY center, float radius, float startAngle, float endAngle, AngularDirection angularDirection)
        {
            PointXYValidation.ThrowIfNotFinite(
                center,
                nameof(center),
                "Arc center coordinates must be finite.");

            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Arc radius must be finite and non-negative.");

            if (float.IsNaN(startAngle) || float.IsInfinity(startAngle))
                throw new ArgumentOutOfRangeException(nameof(startAngle), "Arc start angle must be finite.");

            if (float.IsNaN(endAngle) || float.IsInfinity(endAngle))
                throw new ArgumentOutOfRangeException(nameof(endAngle), "Arc end angle must be finite.");

            if (angularDirection != AngularDirection.Counterclockwise &&
                angularDirection != AngularDirection.Clockwise)
            {
                throw new ArgumentOutOfRangeException(nameof(angularDirection), "Angular direction is not supported.");
            }

            _center = center;
            _radius = radius;
            _startAngle = startAngle.NormalizeAngleRad();
            _endAngle = endAngle.NormalizeAngleRad();
            _angularDirection = angularDirection;
            _isFullCircle = IsFullTurn(startAngle, endAngle);
        }

        /// <summary>
        /// Gets the center of the source circle.
        /// </summary>
        public PointXY Center => _center;

        /// <summary>
        /// Gets the radius of the source circle.
        /// </summary>
        public float Radius => _radius;

        /// <summary>
        /// Gets the normalized start angle in radians.
        /// </summary>
        public float StartAngle => _startAngle;

        /// <summary>
        /// Gets the normalized end angle in radians.
        /// </summary>
        public float EndAngle => _endAngle;

        /// <summary>
        /// Gets the direction in which curve coordinates increase.
        /// </summary>
        public AngularDirection AngularDirection => _angularDirection;

        /// <summary>
        /// Gets the normalized start angle in degrees.
        /// </summary>
        public float StartAngleDeg => _startAngle * Constants.Rad2Deg;

        /// <summary>
        /// Gets the normalized end angle in degrees.
        /// </summary>
        public float EndAngleDeg => _endAngle * Constants.Rad2Deg;

        /// <summary>
        /// Gets a value indicating whether this arc represents a full circle.
        /// </summary>
        public bool IsFullCircle => _isFullCircle;

        /// <summary>
        /// Gets the point at the start angle of this arc.
        /// </summary>
        public PointXY StartPoint => GetPointAtAngle(_startAngle);

        /// <summary>
        /// Gets the point at the end angle of this arc.
        /// </summary>
        public PointXY EndPoint => GetPointAtAngle(_endAngle);

        /// <summary>
        /// Gets the arc length.
        /// </summary>
        public float Length => GetArcLength();

        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        public PointXY EndpointA => StartPoint;

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        public PointXY EndpointB => EndPoint;

        /// <summary>
        /// Determines whether the direction from this arc's center to the specified point lies within this arc's angular region.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns><see langword="true"/> if the point lies within this arc's angular region; otherwise, <see langword="false"/>.</returns>
        public bool IsWithinAngularRegion(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = (point - Center).Normalize();
            float angle = MathF.Atan2(toPoint.Y, toPoint.X).NormalizeAngleRad();

            return ContainsAngle(angle);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ParameterizedArc other && Equals(other);

        /// <summary>
        /// Indicates whether this arc has the same center, radius, angles, traversal direction, and full-circle flag as another arc.
        /// </summary>
        /// <param name="other">The arc to compare with this arc.</param>
        /// <returns><see langword="true"/> if both arcs are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedArc other) =>
            Center.Equals(other.Center) &&
            Radius.Equals(other.Radius) &&
            StartAngle.Equals(other.StartAngle) &&
            EndAngle.Equals(other.EndAngle) &&
            AngularDirection == other.AngularDirection &&
            IsFullCircle == other.IsFullCircle;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Center, Radius, StartAngle, EndAngle, AngularDirection, IsFullCircle);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedArc(center: {0}, radius: {1}, rad: {2} - {3}, direction: {4}, fullCircle: {5})",
                Center,
                Radius,
                StartAngle,
                EndAngle,
                AngularDirection,
                IsFullCircle);

        /// <summary>
        /// Returns a string representation of this arc with angles in degrees.
        /// </summary>
        /// <returns>A string representation of this arc with degree angles.</returns>
        public string ToDegreesString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedArc(center: {0}, radius: {1}, deg: {2} - {3}, direction: {4}, fullCircle: {5})",
                Center,
                Radius,
                StartAngleDeg,
                EndAngleDeg,
                AngularDirection,
                IsFullCircle);

        /// <summary>
        /// Returns point intersections between this arc and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this arc.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            var intersections = new List<PointXY>();

            if (_radius <= geometryEpsilon)
            {
                VectorXY toCenter = _center - ray.Origin;
                if (VectorXY.Dot(toCenter, ray.Direction) >= -geometryEpsilon &&
                    VectorXY.Cross(toCenter, ray.Direction).IsAlmostZero(geometryEpsilon))
                {
                    intersections.AddDistinct(_center, geometryEpsilon);
                }

                return intersections;
            }

            VectorXY dir = ray.Direction;
            VectorXY f = ray.Origin - _center;

            float a = 1f;
            float b = 2f * VectorXY.Dot(f, dir);
            float c = f.SquaredLength - _radius * _radius;

            float discriminant = b * b - 4f * a * c;

            if (discriminant < -geometryEpsilon)
                return intersections;

            if (discriminant < 0f)
                discriminant = 0f;

            float sqrtD = MathF.Sqrt(discriminant);

            float t1 = (-b - sqrtD) / (2f * a);
            float t2 = (-b + sqrtD) / (2f * a);

            AddRayCircleIntersectionIfOnArc(ray, t1, intersections, geometryEpsilon);

            if (!t2.AlmostEquals(t1, geometryEpsilon))
                AddRayCircleIntersectionIfOnArc(ray, t2, intersections, geometryEpsilon);

            return intersections;
        }

        /// <summary>
        /// Returns the shortest distance from the specified point to this arc.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this arc.</returns>
        public float Distance(PointXY point)
        {
            return Project(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this arc.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this arc.</returns>
        public CurveProjection Project(PointXY point)
        {
            var projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this arc and reports the arc length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, arc length coordinate, and distance to this arc.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = point - _center;

            if (_radius <= GeometryConstants.GeometryEpsilon || toPoint.SquaredLength <= GeometryConstants.GeometryEpsilonSquared)
            {
                PointXY start = StartPoint;
                return new ParameterizedCurveProjection(start, 0f, point.Distance(start));
            }

            float angleToPoint = MathF.Atan2(toPoint.Y, toPoint.X).NormalizeAngleRad();

            if (ContainsAngle(angleToPoint))
            {
                PointXY projected = _center + toPoint.Normalize() * _radius;
                float curveCoordinate = GetCurveCoordinate(angleToPoint);
                return new ParameterizedCurveProjection(projected, curveCoordinate, point.Distance(projected));
            }

            PointXY arcStart = StartPoint;
            PointXY arcEnd = EndPoint;

            float distStart = point.Distance(arcStart);
            float distEnd = point.Distance(arcEnd);

            if (distStart <= distEnd)
                return new ParameterizedCurveProjection(arcStart, 0f, distStart);

            return new ParameterizedCurveProjection(arcEnd, GetArcLength(), distEnd);
        }

        /// <summary>
        /// Indicates whether two arcs are equal.
        /// </summary>
        /// <param name="left">The first arc.</param>
        /// <param name="right">The second arc.</param>
        /// <returns><see langword="true"/> if the arcs are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedArc left, ParameterizedArc right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two arcs are different.
        /// </summary>
        /// <param name="left">The first arc.</param>
        /// <param name="right">The second arc.</param>
        /// <returns><see langword="true"/> if the arcs are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedArc left, ParameterizedArc right) => !(left == right);

        /// <summary>
        /// Converts a parameterized arc to its geometric arc.
        /// </summary>
        /// <param name="arc">The parameterized arc to convert.</param>
        public static explicit operator Arc(ParameterizedArc arc)
        {
            if (arc.IsFullCircle)
                return new Arc(arc.Center, arc.Radius, arc.StartAngle, arc.StartAngle + 2f * MathF.PI);

            return arc.AngularDirection == AngularDirection.Counterclockwise
                ? new Arc(arc.Center, arc.Radius, arc.StartAngle, arc.EndAngle)
                : new Arc(arc.Center, arc.Radius, arc.EndAngle, arc.StartAngle);
        }

        private void AddRayCircleIntersectionIfOnArc(
            Ray ray,
            float rayCoordinate,
            List<PointXY> intersections,
            float geometryEpsilon)
        {
            if (rayCoordinate < -geometryEpsilon)
                return;

            PointXY point = ray.Origin + ray.Direction * rayCoordinate;
            if (IsWithinAngularRegion(point))
                intersections.AddDistinct(point, geometryEpsilon);
        }

        private float GetCurveCoordinate(float angle)
        {
            if (IsFullCircle)
                return GetAngleDeltaFromStart(angle) * _radius;

            float span = GetAngularSpan();
            if (span <= GeometryConstants.GeometryEpsilon)
                return 0f;

            return GetAngleDeltaFromStart(angle) * _radius;
        }

        private float GetArcLength()
        {
            if (IsFullCircle)
                return 2f * MathF.PI * _radius;

            float span = GetAngularSpan();

            return span * _radius;
        }

        private bool ContainsAngle(float angle)
        {
            return IsFullCircle || (AngularDirection == AngularDirection.Counterclockwise
                ? angle.IsAngleWithinArc(_startAngle, _endAngle)
                : angle.IsAngleWithinArc(_endAngle, _startAngle));
        }

        private float GetAngularSpan()
        {
            return AngularDirection == AngularDirection.Counterclockwise
                ? PositiveAngleDelta(_startAngle, _endAngle)
                : PositiveAngleDelta(_endAngle, _startAngle);
        }

        private float GetAngleDeltaFromStart(float angle)
        {
            return AngularDirection == AngularDirection.Counterclockwise
                ? PositiveAngleDelta(_startAngle, angle)
                : PositiveAngleDelta(angle, _startAngle);
        }

        /// <summary>
        /// Returns the point at the specified arc length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units.</param>
        /// <returns>The point on this arc.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            float length = Length;
            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the arc length.");

            if (_radius <= GeometryConstants.GeometryEpsilon)
                return StartPoint;

            float angleDelta = curveCoordinate / _radius;
            float angle = AngularDirection == AngularDirection.Counterclockwise
                ? (_startAngle + angleDelta).NormalizeAngleRad()
                : (_startAngle - angleDelta).NormalizeAngleRad();

            return GetPointAtAngle(angle);
        }

        private PointXY GetPointAtAngle(float angle)
        {
            return new PointXY(
                _center.X + _radius * MathF.Cos(angle),
                _center.Y + _radius * MathF.Sin(angle));
        }

        private static float PositiveAngleDelta(float from, float to)
        {
            float delta = to - from;
            if (delta < 0f)
                delta += 2f * MathF.PI;

            return delta;
        }

        private static bool IsFullTurn(float startAngle, float endAngle)
        {
            float delta = endAngle - startAngle;
            if (MathF.Abs(delta) <= GeometryConstants.GeometryEpsilon)
                return false;

            float turns = delta / (2f * MathF.PI);
            return turns.AlmostEquals(MathF.Round(turns));
        }

    }
}
