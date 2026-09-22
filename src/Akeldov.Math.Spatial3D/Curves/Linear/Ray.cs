using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a half-line that starts at an origin and extends in one direction in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// The default value starts at the global coordinate origin and points along the positive X axis.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Ray : IRayPath, IEquatable<Ray>
    {
        private readonly PointXYZ _origin;
        // Store X shifted so default(Ray) points along the positive X axis.
        private readonly float _directionXMinusOne;
        private readonly float _directionY;
        private readonly float _directionZ;

        /// <summary>
        /// Initializes a new ray that starts at the specified origin and points along the positive X axis.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an origin coordinate is NaN or infinite.
        /// </exception>
        public Ray(PointXYZ origin)
        {
            if (float.IsNaN(origin.X) || float.IsInfinity(origin.X) ||
                float.IsNaN(origin.Y) || float.IsInfinity(origin.Y) ||
                float.IsNaN(origin.Z) || float.IsInfinity(origin.Z))
                throw new ArgumentOutOfRangeException(nameof(origin), "Ray origin coordinates must be finite.");

            _origin = origin;
            _directionXMinusOne = 0f;
            _directionY = 0f;
            _directionZ = 0f;
        }

        /// <summary>
        /// Initializes a new ray that starts at the specified origin and points in the specified direction.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        /// <param name="direction">A non-zero vector in the ray direction.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="direction"/> is the zero vector.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an origin coordinate or direction component is NaN or infinite.
        /// </exception>
        public Ray(PointXYZ origin, VectorXYZ direction)
        {
            if (float.IsNaN(origin.X) || float.IsInfinity(origin.X) ||
                float.IsNaN(origin.Y) || float.IsInfinity(origin.Y) ||
                float.IsNaN(origin.Z) || float.IsInfinity(origin.Z))
                throw new ArgumentOutOfRangeException(nameof(origin), "Ray origin coordinates must be finite.");

            if (!direction.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(direction), "Ray direction components must be finite.");

            double length = global::System.Math.Sqrt(
                (double)direction.X * direction.X +
                (double)direction.Y * direction.Y +
                (double)direction.Z * direction.Z);

            if (length == 0d)
                throw new ArgumentException("Ray direction must have non-zero length.", nameof(direction));

            _origin = origin;
            _directionXMinusOne = (float)(direction.X / length) - 1f;
            _directionY = (float)(direction.Y / length);
            _directionZ = (float)(direction.Z / length);
        }

        /// <summary>
        /// Gets the ray origin.
        /// </summary>
        public PointXYZ Origin => _origin;

        /// <summary>
        /// Gets the normalized ray direction.
        /// </summary>
        public VectorXYZ Direction => new VectorXYZ(_directionXMinusOne + 1f, _directionY, _directionZ);

        /// <summary>
        /// Gets the ray endpoint.
        /// </summary>
        public PointXYZ Endpoint => _origin;

        /// <summary>
        /// Returns the shortest distance from the specified point to this ray.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this ray.</returns>
        public float Distance(PointXYZ point)
        {
            return ProjectWithParameter(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this ray.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this ray.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this ray and reports the ray length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, non-negative ray length coordinate, and distance to this ray.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            VectorXYZ direction = Direction;
            double offsetX = (double)point.X - _origin.X;
            double offsetY = (double)point.Y - _origin.Y;
            double offsetZ = (double)point.Z - _origin.Z;
            double coordinate =
                offsetX * direction.X +
                offsetY * direction.Y +
                offsetZ * direction.Z;

            if (coordinate < 0d)
                coordinate = 0d;

            var projected = new PointXYZ(
                (float)(_origin.X + coordinate * direction.X),
                (float)(_origin.Y + coordinate * direction.Y),
                (float)(_origin.Z + coordinate * direction.Z));

            double dx = (double)point.X - projected.X;
            double dy = (double)point.Y - projected.Y;
            double dz = (double)point.Z - projected.Z;
            float distance = (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);

            return new ParameterizedCurveProjection(projected, (float)coordinate, distance);
        }

        /// <summary>
        /// Returns the point at the specified ray length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite non-negative curve coordinate in world coordinate units.</param>
        /// <returns>The point on this ray.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is negative, NaN, or infinite.
        /// </exception>
        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be non-negative.");

            return Origin + curveCoordinate * Direction;
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
