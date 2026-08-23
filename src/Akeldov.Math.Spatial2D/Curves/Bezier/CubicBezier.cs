using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed cubic Bezier curve segment.
    /// </summary>
    /// <remarks>
    /// Length and length-coordinate operations use a fixed internal polyline approximation
    /// of the curve. Ray intersections are found by solving the original curve polynomial.
    /// </remarks>
    public readonly struct CubicBezier : IFinitePath, IEquatable<CubicBezier>
    {
        private readonly PointXY _startPoint;
        private readonly PointXY _controlPointA;
        private readonly PointXY _controlPointB;
        private readonly PointXY _endPoint;

        /// <summary>
        /// Initializes a new cubic Bezier curve.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="controlPointA">The first control point.</param>
        /// <param name="controlPointB">The second control point.</param>
        /// <param name="endPoint">The end point.</param>
        public CubicBezier(
            PointXY startPoint,
            PointXY controlPointA,
            PointXY controlPointB,
            PointXY endPoint)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Bezier control point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                controlPointA,
                nameof(controlPointA),
                "Bezier control point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                controlPointB,
                nameof(controlPointB),
                "Bezier control point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
                nameof(endPoint),
                "Bezier control point coordinates must be finite.");

            _startPoint = startPoint;
            _controlPointA = controlPointA;
            _controlPointB = controlPointB;
            _endPoint = endPoint;
        }

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXY StartPoint => _startPoint;

        /// <summary>
        /// Gets the first cubic Bezier control point.
        /// </summary>
        public PointXY ControlPointA => _controlPointA;

        /// <summary>
        /// Gets the second cubic Bezier control point.
        /// </summary>
        public PointXY ControlPointB => _controlPointB;

        /// <summary>
        /// Gets the point at the end of the traversal direction.
        /// </summary>
        public PointXY EndPoint => _endPoint;

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
        public float Length => BezierPathApproximation.GetLength(GetPointAtUnchecked);

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
            return ProjectClosest(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this curve using the curve approximation.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point and distance to this curve.</returns>
        public CurveProjection Project(PointXY point)
        {
            return ProjectClosest(point);
        }

        /// <summary>
        /// Projects the specified point onto this curve using the curve approximation and reports the approximate length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point, length coordinate, and distance to this curve.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            return BezierPathApproximation.ProjectWithParameter(GetPointAtUnchecked, point);
        }

        /// <summary>
        /// Returns point intersections between this curve and the specified ray by solving the original curve polynomial.
        /// </summary>
        /// <param name="ray">The ray to intersect with this curve.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        public List<PointXY> GetPointIntersections(Ray ray)
        {
            List<PointXY> intersections = RayIntersectionExtensions.GetPointIntersections(ray, this);
            intersections.Sort((left, right) =>
                VectorXY.Dot(left - ray.Origin, ray.Direction).CompareTo(
                    VectorXY.Dot(right - ray.Origin, ray.Direction)));
            return intersections;
        }

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin) =>
            BezierPathApproximation.CountRightwardCrossings(
                StartPoint,
                ControlPointA,
                ControlPointB,
                EndPoint,
                origin);

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
        public override bool Equals(object? obj) => obj is CubicBezier other && Equals(other);

        /// <summary>
        /// Indicates whether this curve has the same control points as another curve.
        /// </summary>
        /// <param name="other">The curve to compare with this curve.</param>
        /// <returns><see langword="true"/> if both curves are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(CubicBezier other) =>
            StartPoint.Equals(other.StartPoint) &&
            ControlPointA.Equals(other.ControlPointA) &&
            ControlPointB.Equals(other.ControlPointB) &&
            EndPoint.Equals(other.EndPoint);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(StartPoint, ControlPointA, ControlPointB, EndPoint);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(
                CultureInfo.InvariantCulture,
                "CubicBezier({0}, {1}, {2}, {3})",
                StartPoint,
                ControlPointA,
                ControlPointB,
                EndPoint);

        /// <summary>
        /// Indicates whether two curves are equal.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(CubicBezier left, CubicBezier right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two curves are different.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(CubicBezier left, CubicBezier right) => !(left == right);

        /// <summary>
        /// Translates a curve by a vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated curve.</returns>
        public static CubicBezier operator +(CubicBezier left, VectorXY right) => new CubicBezier(
            left.StartPoint + right,
            left.ControlPointA + right,
            left.ControlPointB + right,
            left.EndPoint + right);

        /// <summary>
        /// Translates a curve by the negated vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated curve.</returns>
        public static CubicBezier operator -(CubicBezier left, VectorXY right) => new CubicBezier(
            left.StartPoint - right,
            left.ControlPointA - right,
            left.ControlPointB - right,
            left.EndPoint - right);

        private PointXY GetPointAtUnchecked(float t)
        {
            float inverseT = 1f - t;
            float inverseTSquared = inverseT * inverseT;
            float tSquared = t * t;
            float startAmount = inverseTSquared * inverseT;
            float controlAAmount = 3f * inverseTSquared * t;
            float controlBAmount = 3f * inverseT * tSquared;
            float endAmount = tSquared * t;

            return new PointXY(
                StartPoint.X * startAmount +
                    ControlPointA.X * controlAAmount +
                    ControlPointB.X * controlBAmount +
                    EndPoint.X * endAmount,
                StartPoint.Y * startAmount +
                    ControlPointA.Y * controlAAmount +
                    ControlPointB.Y * controlBAmount +
                    EndPoint.Y * endAmount);
        }

        private CurveProjection ProjectClosest(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");

            PointXY bestPoint = StartPoint;
            float bestSquaredDistance = (point - bestPoint).SquaredLength;

            for (int seedIndex = 0; seedIndex <= 8; seedIndex++)
            {
                float parameter = seedIndex * 0.125f;

                for (int iteration = 0; iteration < 6; iteration++)
                {
                    float inverse = 1f - parameter;
                    PointXY curvePoint = GetPointAtUnchecked(parameter);
                    VectorXY firstDerivative = 3f * (
                        inverse * inverse * (ControlPointA - StartPoint) +
                        2f * inverse * parameter * (ControlPointB - ControlPointA) +
                        parameter * parameter * (EndPoint - ControlPointB));
                    VectorXY secondDerivative = 6f * (
                        inverse * ((ControlPointB - ControlPointA) - (ControlPointA - StartPoint)) +
                        parameter * ((EndPoint - ControlPointB) - (ControlPointB - ControlPointA)));
                    VectorXY delta = curvePoint - point;
                    float denominator = firstDerivative.SquaredLength + VectorXY.Dot(delta, secondDerivative);
                    if (denominator == 0f)
                        break;

                    float next = parameter - VectorXY.Dot(delta, firstDerivative) / denominator;
                    parameter = MathF.Max(0f, MathF.Min(1f, next));
                }

                PointXY candidate = GetPointAtUnchecked(parameter);
                float squaredDistance = (point - candidate).SquaredLength;
                if (squaredDistance < bestSquaredDistance)
                {
                    bestPoint = candidate;
                    bestSquaredDistance = squaredDistance;
                }
            }

            PointXY end = EndPoint;
            float endSquaredDistance = (point - end).SquaredLength;
            if (endSquaredDistance < bestSquaredDistance)
            {
                bestPoint = end;
                bestSquaredDistance = endSquaredDistance;
            }

            return new CurveProjection(bestPoint, MathF.Sqrt(bestSquaredDistance));
        }
    }
}
