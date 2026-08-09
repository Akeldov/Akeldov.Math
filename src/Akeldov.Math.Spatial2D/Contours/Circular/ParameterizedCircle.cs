using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Represents a circular contour with a length-based curve coordinate around its circumference.
    /// </summary>
    [Serializable]
    public readonly struct ParameterizedCircle : IParameterizedContour, IEquatable<ParameterizedCircle>
    {
        private readonly Circle _circle;
        private readonly float _startAngle;
        private readonly ContourDirection _contourDirection;

        /// <summary>
        /// Initializes a new parameterized circular contour with the specified center, radius, start angle, and contour direction.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="startAngle">The angle in radians where curve coordinate zero lies.</param>
        /// <param name="contourDirection">The direction in which curve coordinates increase along the contour.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="radius"/> is negative, NaN, or infinite, when <paramref name="startAngle"/>
        /// is NaN or infinite, or when <paramref name="contourDirection"/> is unsupported.
        /// </exception>
        public ParameterizedCircle(
            PointXY center,
            float radius,
            float startAngle = 0f,
            ContourDirection contourDirection = ContourDirection.Counterclockwise)
        {
            var circle = new Circle(center, radius);

            if (float.IsNaN(startAngle) || float.IsInfinity(startAngle))
                throw new ArgumentOutOfRangeException(nameof(startAngle), "Circle start angle must be finite.");

            if (contourDirection != ContourDirection.Counterclockwise &&
                contourDirection != ContourDirection.Clockwise)
            {
                throw new ArgumentOutOfRangeException(nameof(contourDirection), "Contour direction is not supported.");
            }

            _circle = circle;
            _startAngle = startAngle.NormalizeAngleRad();
            _contourDirection = contourDirection;
        }

        /// <summary>
        /// Gets the source circle.
        /// </summary>
        public Circle Circle => _circle;

        /// <summary>
        /// Gets the center of the circle.
        /// </summary>
        public PointXY Center => _circle.Center;

        /// <summary>
        /// Gets the circle radius.
        /// </summary>
        public float Radius => _circle.Radius;

        /// <summary>
        /// Gets the normalized angle in radians where curve coordinate zero lies.
        /// </summary>
        public float StartAngle => _startAngle;

        /// <summary>
        /// Gets the direction in which curve coordinates increase along the contour.
        /// </summary>
        public ContourDirection ContourDirection => _contourDirection;

        /// <summary>
        /// Gets the normalized start angle in degrees.
        /// </summary>
        public float StartAngleDeg => _startAngle * Constants.Rad2Deg;

        /// <summary>
        /// Gets the circumference length.
        /// </summary>
        public float Length => _circle.Length;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin) => _circle.CountRightwardCrossings(origin);

        /// <inheritdoc/>
        public bool Encloses(PointXY point)
        {
            return _circle.Encloses(point);
        }

        /// <inheritdoc/>
        public List<PointXY> GetPointIntersections(Ray ray) =>
            GetRayIntersections(ray);

        /// <inheritdoc cref="Circle.GetRayIntersections(Ray, float)"/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _circle.GetRayIntersections(ray, geometryEpsilon);
        }

        /// <inheritdoc/>
        public CurveProjection Project(PointXY point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <inheritdoc/>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            PointXY start = GetPointAtAngle(_startAngle);
            VectorXY toPoint = point - Center;

            if (Radius <= GeometryConstants.GeometryEpsilon ||
                toPoint.SquaredLength <= GeometryConstants.GeometryEpsilonSquared)
            {
                return new ParameterizedCurveProjection(start, 0f, point.Distance(start));
            }

            PointXY projected = Center + toPoint.Normalize() * Radius;
            float angleToPoint = MathF.Atan2(toPoint.Y, toPoint.X).NormalizeAngleRad();
            float curveCoordinate = GetCurveCoordinate(angleToPoint);

            return new ParameterizedCurveProjection(projected, curveCoordinate, point.Distance(projected));
        }

        /// <summary>
        /// Returns the point at the specified circumference length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units.</param>
        /// <returns>The point on this circle.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN, infinite, or outside the <c>[0, Length]</c> range.
        /// </exception>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > Length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the circle circumference length.");

            if (Radius <= GeometryConstants.GeometryEpsilon)
                return Center;

            float angleDelta = curveCoordinate / Radius;
            float angle = ContourDirection == ContourDirection.Counterclockwise
                ? (_startAngle + angleDelta).NormalizeAngleRad()
                : (_startAngle - angleDelta).NormalizeAngleRad();

            return GetPointAtAngle(angle);
        }

        /// <inheritdoc/>
        public float Distance(PointXY point)
        {
            return _circle.Distance(point);
        }

        /// <inheritdoc/>
        public float SignedDistance(
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return _circle.SignedDistance(point, geometryEpsilon);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ParameterizedCircle other && Equals(other);

        /// <summary>
        /// Indicates whether this circle has the same geometry, start angle, and traversal direction as another circle.
        /// </summary>
        /// <param name="other">The circle to compare with this circle.</param>
        /// <returns><see langword="true"/> if both parameterized circles are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedCircle other) =>
            Circle.Equals(other.Circle) &&
            StartAngle.Equals(other.StartAngle) &&
            ContourDirection == other.ContourDirection;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Circle, StartAngle, ContourDirection);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedCircle(center: {0}, radius: {1}, rad: {2}, direction: {3})",
                Center,
                Radius,
                StartAngle,
                ContourDirection);

        /// <summary>
        /// Returns a string representation of this circle with the start angle in degrees.
        /// </summary>
        /// <returns>A string representation of this circle with a degree angle.</returns>
        public string ToDegreesString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "ParameterizedCircle(center: {0}, radius: {1}, deg: {2}, direction: {3})",
                Center,
                Radius,
                StartAngleDeg,
                ContourDirection);

        /// <summary>
        /// Converts a parameterized circle to its geometric circle.
        /// </summary>
        /// <param name="circle">The parameterized circle to convert.</param>
        public static explicit operator Circle(ParameterizedCircle circle)
        {
            return circle.Circle;
        }

        /// <summary>
        /// Indicates whether two parameterized circles are equal.
        /// </summary>
        /// <param name="left">The first circle.</param>
        /// <param name="right">The second circle.</param>
        /// <returns><see langword="true"/> if the parameterized circles are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedCircle left, ParameterizedCircle right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two parameterized circles are different.
        /// </summary>
        /// <param name="left">The first circle.</param>
        /// <param name="right">The second circle.</param>
        /// <returns><see langword="true"/> if the parameterized circles are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedCircle left, ParameterizedCircle right) => !(left == right);

        private float GetCurveCoordinate(float angle)
        {
            float angleDelta = ContourDirection == ContourDirection.Counterclockwise
                ? PositiveAngleDelta(_startAngle, angle)
                : PositiveAngleDelta(angle, _startAngle);

            return angleDelta * Radius;
        }

        private PointXY GetPointAtAngle(float angle)
        {
            return new PointXY(
                Center.X + Radius * MathF.Cos(angle),
                Center.Y + Radius * MathF.Sin(angle));
        }

        private static float PositiveAngleDelta(float from, float to)
        {
            float delta = to - from;
            if (delta < 0f)
                delta += 2f * MathF.PI;

            return delta;
        }
    }
}
