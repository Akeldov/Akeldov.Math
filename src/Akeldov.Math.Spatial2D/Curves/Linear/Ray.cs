using Akeldov.Math.Spatial2D;
using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a half-line that starts at an origin and extends in one direction.
    /// </summary>
    /// <remarks>
    /// The default value starts at the coordinate origin and points along the positive X axis.
    /// </remarks>
    public readonly struct Ray : IRayPath, IRightwardCrossingProvider, IEquatable<Ray>
    {
        private readonly PointXY _origin;
        private readonly float _angle;
        // Store X shifted so default(Ray) points along positive X instead of having a zero direction.
        private readonly float _directionXMinusOne;
        private readonly float _directionY;

        /// <summary>
        /// Initializes a new ray that starts at the specified origin and points along the positive X axis.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        public Ray(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(
                origin,
                nameof(origin),
                "Ray origin coordinates must be finite.");

            _origin = origin;
            _angle = 0f;
            _directionXMinusOne = 0f;
            _directionY = 0f;
        }

        /// <summary>
        /// Initializes a new ray that starts at the specified origin and points in the specified angle.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        /// <param name="angle">The ray direction angle in radians.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angle"/> is NaN or infinite.</exception>
        public Ray(PointXY origin, float angle)
        {
            PointXYValidation.ThrowIfNotFinite(
                origin,
                nameof(origin),
                "Ray origin coordinates must be finite.");

            if (float.IsNaN(angle) || float.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), "Ray angle must be finite.");

            _origin = origin;
            _angle = angle;
            _directionXMinusOne = MathF.Cos(angle) - 1f;
            _directionY = MathF.Sin(angle);
        }

        /// <summary>
        /// Gets the ray origin.
        /// </summary>
        public PointXY Origin => _origin;

        /// <summary>
        /// Gets the normalized ray direction.
        /// </summary>
        public VectorXY Direction => new VectorXY(_directionXMinusOne + 1f, _directionY);

        /// <summary>
        /// Gets the ray direction angle in radians.
        /// </summary>
        public float Angle => _angle;

        /// <summary>
        /// Gets the ray endpoint.
        /// </summary>
        public PointXY Endpoint => _origin;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            VectorXY direction = Direction;
            if (direction.Y == 0f)
                return 0;

            float coordinate = (origin.Y - Origin.Y) / direction.Y;
            if (coordinate < 0f || (coordinate == 0f && direction.Y < 0f))
                return 0;

            float x = Origin.X + coordinate * direction.X;
            return x > origin.X ? 1 : 0;
        }

        /// <summary>
        /// Returns the shortest distance from the specified point to this ray.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this ray.</returns>
        public float Distance(PointXY point)
        {
            return Project(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this ray.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this ray.</returns>
        public CurveProjection Project(PointXY point)
        {
            var projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this ray and reports the ray length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, ray length coordinate, and distance to this ray.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY toPoint = point - _origin;
            VectorXY direction = Direction;
            float t = VectorXY.Dot(toPoint, direction);

            if (t < 0f)
                t = 0f;

            PointXY projected = _origin + direction * t;
            return new ParameterizedCurveProjection(projected, t, point.Distance(projected));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Ray other && Equals(other);

        /// <summary>
        /// Indicates whether this ray has the same origin and direction as another ray.
        /// </summary>
        /// <param name="other">The ray to compare with this ray.</param>
        /// <returns><see langword="true"/> if both rays are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Ray other) => Origin.Equals(other.Origin) && Direction.Equals(other.Direction);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Origin, Direction);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} + t*{1}, t >= 0)", Origin, Direction);

        /// <summary>
        /// Returns the point at the specified ray length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite non-negative curve coordinate in world coordinate units.</param>
        /// <returns>The point on this ray.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be non-negative.");

            return Origin + curveCoordinate * Direction;
        }

        /// <summary>
        /// Indicates whether two rays are equal.
        /// </summary>
        /// <param name="left">The first ray.</param>
        /// <param name="right">The second ray.</param>
        /// <returns><see langword="true"/> if the rays are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Ray left, Ray right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two rays are different.
        /// </summary>
        /// <param name="left">The first ray.</param>
        /// <param name="right">The second ray.</param>
        /// <returns><see langword="true"/> if the rays are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Ray left, Ray right) => !(left == right);
    }
}
