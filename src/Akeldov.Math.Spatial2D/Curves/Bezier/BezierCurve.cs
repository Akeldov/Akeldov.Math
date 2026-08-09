using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed Bezier curve segment of arbitrary degree.
    /// </summary>
    /// <remarks>
    /// Length, projection, distance, and ray-intersection operations use a fixed internal
    /// polyline approximation of the curve.
    /// </remarks>
    public sealed class BezierCurve : IFinitePath, IEquatable<BezierCurve>
    {
        private readonly PointXY[] _controlPoints;
        private readonly PointXY[] _approximationPoints;
        private readonly IReadOnlyList<PointXY> _readOnlyControlPoints;
        private readonly float _length;

        /// <summary>
        /// Initializes a new Bezier curve from the specified control points.
        /// </summary>
        /// <param name="controlPoints">The control points in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controlPoints"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than two control points are provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a control point has a NaN or infinite coordinate.</exception>
        public BezierCurve(params PointXY[] controlPoints)
            : this((IReadOnlyList<PointXY>)(controlPoints ?? throw new ArgumentNullException(nameof(controlPoints))))
        {
        }

        /// <summary>
        /// Initializes a new Bezier curve from the specified control points.
        /// </summary>
        /// <param name="controlPoints">The control points in traversal order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controlPoints"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fewer than two control points are provided.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a control point has a NaN or infinite coordinate.</exception>
        public BezierCurve(IReadOnlyList<PointXY> controlPoints)
        {
            if (controlPoints == null)
                throw new ArgumentNullException(nameof(controlPoints));

            if (controlPoints.Count < 2)
                throw new ArgumentException("BezierCurve must contain at least two control points.", nameof(controlPoints));

            _controlPoints = new PointXY[controlPoints.Count];
            for (int i = 0; i < controlPoints.Count; i++)
            {
                PointXY controlPoint = controlPoints[i];
                PointXYValidation.ThrowIfNotFinite(
                    controlPoint,
                    nameof(controlPoints),
                    "Bezier control point coordinates must be finite.");

                _controlPoints[i] = controlPoint;
            }

            _readOnlyControlPoints = Array.AsReadOnly(_controlPoints);
            _approximationPoints = BezierPathApproximation.CreatePoints(GetPointAtUnchecked);
            _length = BezierPathApproximation.GetLength(_approximationPoints);
        }

        /// <summary>
        /// Gets the read-only structural view of the copied control points.
        /// </summary>
        public IReadOnlyList<PointXY> ControlPoints => _readOnlyControlPoints;

        /// <summary>
        /// Gets the Bezier curve degree.
        /// </summary>
        public int Degree => _controlPoints.Length - 1;

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXY StartPoint => _controlPoints[0];

        /// <summary>
        /// Gets the point at the end of the traversal direction.
        /// </summary>
        public PointXY EndPoint => _controlPoints[_controlPoints.Length - 1];

        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        public PointXY EndpointA => StartPoint;

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        public PointXY EndpointB => EndPoint;

        /// <summary>
        /// Gets the approximate curve length in world coordinate units.
        /// </summary>
        public float Length => _length;

        /// <summary>
        /// Returns the point at the specified normalized Bezier parameter.
        /// </summary>
        /// <param name="t">The normalized Bezier parameter in the [0, 1] range.</param>
        /// <returns>The point on this curve.</returns>
        public PointXY GetPointAt(float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t) || t < 0f || t > 1f)
                throw new ArgumentOutOfRangeException(nameof(t), "Bezier parameter must be finite and lie within the [0, 1] range.");

            return GetPointAtUnchecked(t);
        }

        /// <summary>
        /// Returns a polyline approximation of this curve.
        /// </summary>
        /// <param name="segmentCount">The positive number of line segments to create.</param>
        /// <returns>A new mutable list of directed segments owned by the caller.</returns>
        public List<ParameterizedSegment> Flatten(int segmentCount)
        {
            return BezierPathApproximation.Flatten(GetPointAtUnchecked, segmentCount);
        }

        /// <summary>
        /// Returns the shortest approximate distance from the specified point to this curve.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The approximate distance to this curve.</returns>
        public float Distance(PointXY point)
        {
            return BezierPathApproximation.ProjectWithParameter(_approximationPoints, point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this curve using the curve approximation.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point and distance to this curve.</returns>
        public CurveProjection Project(PointXY point)
        {
            ParameterizedCurveProjection projection =
                BezierPathApproximation.ProjectWithParameter(_approximationPoints, point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Projects the specified point onto this curve using the curve approximation and reports the approximate length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point, length coordinate, and distance to this curve.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            return BezierPathApproximation.ProjectWithParameter(_approximationPoints, point);
        }

        /// <summary>
        /// Returns point intersections between this curve's approximation and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this curve.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        public List<PointXY> GetPointIntersections(Ray ray) =>
            BezierPathApproximation.GetPointIntersections(GetPointAtUnchecked, ray);

        /// <inheritdoc cref="ICurve.GetRayIntersections(Ray, float)"/>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return BezierPathApproximation.GetRayIntersections(GetPointAtUnchecked, ray, geometryEpsilon);
        }

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            float maxX = _controlPoints[0].X;
            float minY = _controlPoints[0].Y;
            float maxY = _controlPoints[0].Y;

            for (int i = 1; i < _controlPoints.Length; i++)
            {
                PointXY point = _controlPoints[i];
                if (point.X > maxX)
                    maxX = point.X;
                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }

            if (origin.X >= maxX || origin.Y < minY || origin.Y > maxY)
                return 0;

            return BezierPathApproximation.CountRightwardCrossings(_approximationPoints, origin);
        }

        /// <summary>
        /// Returns the point at the specified approximate curve length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units.</param>
        /// <returns>The point on this curve.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            return BezierPathApproximation.GetPoint(GetPointAtUnchecked, Length, curveCoordinate);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is BezierCurve other && Equals(other);

        /// <summary>
        /// Indicates whether this curve has the same control points as another curve.
        /// </summary>
        /// <param name="other">The curve to compare with this curve.</param>
        /// <returns><see langword="true"/> if both curves are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(BezierCurve? other)
        {
            if (other == null || other._controlPoints.Length != _controlPoints.Length)
                return false;

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                if (!_controlPoints[i].Equals(other._controlPoints[i]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            for (int i = 0; i < _controlPoints.Length; i++)
            {
                hashCode.Add(_controlPoints[i]);
            }

            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "BezierCurve(degree: {0})", Degree);

        /// <summary>
        /// Indicates whether two curves are equal.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(BezierCurve? left, BezierCurve? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two curves are different.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(BezierCurve? left, BezierCurve? right) => !(left == right);

        /// <summary>
        /// Translates a curve by a vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated curve.</returns>
        public static BezierCurve operator +(BezierCurve left, VectorXY right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            var translated = new PointXY[left._controlPoints.Length];
            for (int i = 0; i < translated.Length; i++)
            {
                translated[i] = left._controlPoints[i] + right;
            }

            return new BezierCurve(translated);
        }

        /// <summary>
        /// Translates a curve by the negated vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated curve.</returns>
        public static BezierCurve operator -(BezierCurve left, VectorXY right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            var translated = new PointXY[left._controlPoints.Length];
            for (int i = 0; i < translated.Length; i++)
            {
                translated[i] = left._controlPoints[i] - right;
            }

            return new BezierCurve(translated);
        }

        private PointXY GetPointAtUnchecked(float t)
        {
            var points = new PointXY[_controlPoints.Length];
            Array.Copy(_controlPoints, points, _controlPoints.Length);

            for (int level = 1; level < points.Length; level++)
            {
                for (int i = 0; i < points.Length - level; i++)
                {
                    points[i] = Lerp(points[i], points[i + 1], t);
                }
            }

            return points[0];
        }

        private static PointXY Lerp(PointXY start, PointXY end, float t)
        {
            return new PointXY(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t);
        }
    }
}
