using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a circular arc in two-dimensional space.
    /// </summary>
    [Serializable]
    public readonly struct Arc : IFiniteTwoEndpointCurve, IRightwardCrossingProvider, IEquatable<Arc>
    {
        private readonly PointXY _center;
        private readonly float _radius;
        private readonly float _startAngle;
        private readonly float _endAngle;
        private readonly bool _isFullCircle;

        /// <summary>
        /// Creates an arc from <paramref name="startAngle"/> to <paramref name="endAngle"/>.
        /// Equal input angles represent a zero-length arc. An end angle one full turn after the start angle
        /// represents a full circle even though both angles normalize to the same value.
        /// </summary>
        /// <param name="center">The center of the source circle.</param>
        /// <param name="radius">The radius of the source circle.</param>
        /// <param name="startAngle">The start angle in radians.</param>
        /// <param name="endAngle">The end angle in radians.</param>
        public Arc(PointXY center, float radius, float startAngle, float endAngle)
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

            _center = center;
            _radius = radius;
            _startAngle = startAngle.NormalizeAngleRad();
            _endAngle = endAngle.NormalizeAngleRad();
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
        /// Gets the first endpoint.
        /// </summary>
        public PointXY EndpointA => StartPoint;

        /// <summary>
        /// Gets the second endpoint.
        /// </summary>
        public PointXY EndpointB => EndPoint;

        /// <summary>
        /// Gets the arc length.
        /// </summary>
        public float Length => GetArcLength();

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            List<PointXY> intersections = ArcIntersectionExtensions.GetPointIntersections(this, new Ray(origin));
            int count = 0;

            for (int i = 0; i < intersections.Count; i++)
            {
                PointXY intersection = intersections[i];
                if (intersection.X <= origin.X)
                    continue;

                if (intersection.Y == Center.Y - Radius || intersection.Y == Center.Y + Radius)
                    continue;

                if (!IsFullCircle && intersection.Equals(StartPoint) && MathF.Cos(StartAngle) <= 0f)
                    continue;

                if (!IsFullCircle && intersection.Equals(EndPoint) && MathF.Cos(EndAngle) >= 0f)
                    continue;

                count++;
            }

            return count;
        }

        /// <summary>
        /// Determines whether the specified point lies within this arc's angular region.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>
        /// <see langword="true"/> if the point lies within this arc's angular region or at the center of the source circle;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsWithinAngularRegion(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = point - Center;
            if (toPoint.SquaredLength <= GeometryConstants.GeometryEpsilonSquared)
                return true;

            float angle = MathF.Atan2(toPoint.Y, toPoint.X).NormalizeAngleRad();
            return ContainsAngle(angle);
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
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = point - _center;

            if (_radius <= GeometryConstants.GeometryEpsilon || toPoint.SquaredLength <= GeometryConstants.GeometryEpsilonSquared)
            {
                PointXY start = StartPoint;
                return new CurveProjection(start, point.Distance(start));
            }

            float angleToPoint = MathF.Atan2(toPoint.Y, toPoint.X).NormalizeAngleRad();

            if (ContainsAngle(angleToPoint))
            {
                PointXY projected = _center + toPoint.Normalize() * _radius;
                return new CurveProjection(projected, point.Distance(projected));
            }

            PointXY arcStart = StartPoint;
            PointXY arcEnd = EndPoint;

            float distStart = point.Distance(arcStart);
            float distEnd = point.Distance(arcEnd);

            if (distStart <= distEnd)
                return new CurveProjection(arcStart, distStart);

            return new CurveProjection(arcEnd, distEnd);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Arc other && Equals(other);

        /// <summary>
        /// Indicates whether this arc has the same center, radius, angles, and full-circle flag as another arc.
        /// </summary>
        /// <param name="other">The arc to compare with this arc.</param>
        /// <returns><see langword="true"/> if both arcs are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Arc other)
        {
            return Center.Equals(other.Center) &&
                Radius.Equals(other.Radius) &&
                StartAngle.Equals(other.StartAngle) &&
                EndAngle.Equals(other.EndAngle) &&
                IsFullCircle == other.IsFullCircle;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Radius, StartAngle, EndAngle, IsFullCircle);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Arc(center: {0}, radius: {1}, rad: {2} - {3}, fullCircle: {4})",
                Center,
                Radius,
                StartAngle,
                EndAngle,
                IsFullCircle);
        }

        /// <summary>
        /// Returns a string representation of this arc with angles in degrees.
        /// </summary>
        /// <returns>A string representation of this arc with degree angles.</returns>
        public string ToDegreesString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "Arc(center: {0}, radius: {1}, deg: {2} - {3}, fullCircle: {4})",
                Center,
                Radius,
                StartAngleDeg,
                EndAngleDeg,
                IsFullCircle);

        /// <summary>
        /// Indicates whether two arcs are equal.
        /// </summary>
        /// <param name="left">The first arc.</param>
        /// <param name="right">The second arc.</param>
        /// <returns><see langword="true"/> if the arcs are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Arc left, Arc right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two arcs are different.
        /// </summary>
        /// <param name="left">The first arc.</param>
        /// <param name="right">The second arc.</param>
        /// <returns><see langword="true"/> if the arcs are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Arc left, Arc right)
        {
            return !(left == right);
        }

        private float GetArcLength()
        {
            if (IsFullCircle)
                return 2f * MathF.PI * _radius;

            return PositiveAngleDelta(_startAngle, _endAngle) * _radius;
        }

        private bool ContainsAngle(float angle)
        {
            return IsFullCircle || angle.IsAngleWithinArc(_startAngle, _endAngle);
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
