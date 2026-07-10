using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed quadratic Bezier curve segment.
    /// </summary>
    /// <remarks>
    /// Length, projection, distance, and ray-intersection operations use a fixed internal
    /// polyline approximation of the curve.
    /// </remarks>
    public readonly struct QuadraticBezier : IFinitePath, IEquatable<QuadraticBezier>
    {
        private readonly PointXY _startPoint;
        private readonly PointXY _controlPoint;
        private readonly PointXY _endPoint;

        /// <summary>
        /// Initializes a new quadratic Bezier curve.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="controlPoint">The control point.</param>
        /// <param name="endPoint">The end point.</param>
        public QuadraticBezier(PointXY startPoint, PointXY controlPoint, PointXY endPoint)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Bezier control point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                controlPoint,
                nameof(controlPoint),
                "Bezier control point coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
                nameof(endPoint),
                "Bezier control point coordinates must be finite.");

            _startPoint = startPoint;
            _controlPoint = controlPoint;
            _endPoint = endPoint;
        }

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXY StartPoint => _startPoint;

        /// <summary>
        /// Gets the quadratic Bezier control point.
        /// </summary>
        public PointXY ControlPoint => _controlPoint;

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
        /// Returns point intersections between this curve's approximation and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this curve.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return BezierPathApproximation.GetRayIntersections(GetPointAtUnchecked, ray, geometryEpsilon);
        }

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin) =>
            BezierPathApproximation.CountRightwardCrossings(StartPoint, ControlPoint, EndPoint, origin);

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
        public override bool Equals(object? obj) => obj is QuadraticBezier other && Equals(other);

        /// <summary>
        /// Indicates whether this curve has the same control points as another curve.
        /// </summary>
        /// <param name="other">The curve to compare with this curve.</param>
        /// <returns><see langword="true"/> if both curves are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(QuadraticBezier other) =>
            StartPoint.Equals(other.StartPoint) &&
            ControlPoint.Equals(other.ControlPoint) &&
            EndPoint.Equals(other.EndPoint);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(StartPoint, ControlPoint, EndPoint);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "QuadraticBezier({0}, {1}, {2})", StartPoint, ControlPoint, EndPoint);

        /// <summary>
        /// Indicates whether two curves are equal.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(QuadraticBezier left, QuadraticBezier right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two curves are different.
        /// </summary>
        /// <param name="left">The first curve.</param>
        /// <param name="right">The second curve.</param>
        /// <returns><see langword="true"/> if the curves are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(QuadraticBezier left, QuadraticBezier right) => !(left == right);

        /// <summary>
        /// Translates a curve by a vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated curve.</returns>
        public static QuadraticBezier operator +(QuadraticBezier left, VectorXY right) => new QuadraticBezier(
            left.StartPoint + right,
            left.ControlPoint + right,
            left.EndPoint + right);

        /// <summary>
        /// Translates a curve by the negated vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated curve.</returns>
        public static QuadraticBezier operator -(QuadraticBezier left, VectorXY right) => new QuadraticBezier(
            left.StartPoint - right,
            left.ControlPoint - right,
            left.EndPoint - right);

        private PointXY GetPointAtUnchecked(float t)
        {
            float inverseT = 1f - t;
            float startAmount = inverseT * inverseT;
            float controlAmount = 2f * inverseT * t;
            float endAmount = t * t;

            return new PointXY(
                StartPoint.X * startAmount + ControlPoint.X * controlAmount + EndPoint.X * endAmount,
                StartPoint.Y * startAmount + ControlPoint.Y * controlAmount + EndPoint.Y * endAmount);
        }

        private CurveProjection ProjectClosest(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");

            VectorXY quadratic = (EndPoint - ControlPoint) - (ControlPoint - StartPoint);
            VectorXY linear = 2f * (ControlPoint - StartPoint);
            VectorXY offset = StartPoint - point;
            double cubicCoefficient = 2.0 * VectorXY.Dot(quadratic, quadratic);
            double quadraticCoefficient = 3.0 * VectorXY.Dot(quadratic, linear);
            double linearCoefficient =
                VectorXY.Dot(linear, linear) + 2.0 * VectorXY.Dot(offset, quadratic);
            double constantCoefficient = VectorXY.Dot(offset, linear);

            Span<double> boundaries = stackalloc double[4];
            int boundaryCount = 1;
            boundaries[0] = 0.0;
            double derivativeDiscriminant =
                4.0 * quadraticCoefficient * quadraticCoefficient -
                12.0 * cubicCoefficient * linearCoefficient;

            if (cubicCoefficient != 0.0 && derivativeDiscriminant >= 0.0)
            {
                double sqrt = System.Math.Sqrt(derivativeDiscriminant);
                AddBoundary(boundaries, ref boundaryCount,
                    (-2.0 * quadraticCoefficient - sqrt) / (6.0 * cubicCoefficient));
                AddBoundary(boundaries, ref boundaryCount,
                    (-2.0 * quadraticCoefficient + sqrt) / (6.0 * cubicCoefficient));
            }
            else if (cubicCoefficient == 0.0 && quadraticCoefficient != 0.0)
            {
                AddBoundary(boundaries, ref boundaryCount,
                    -linearCoefficient / (2.0 * quadraticCoefficient));
            }

            if (boundaryCount == 3 && boundaries[1] > boundaries[2])
            {
                double temporary = boundaries[1];
                boundaries[1] = boundaries[2];
                boundaries[2] = temporary;
            }

            boundaries[boundaryCount++] = 1.0;
            Span<double> candidates = stackalloc double[8];
            int candidateCount = 0;

            for (int i = 0; i < boundaryCount; i++)
                candidates[candidateCount++] = boundaries[i];

            for (int i = 1; i < boundaryCount; i++)
            {
                double from = boundaries[i - 1];
                double to = boundaries[i];
                double fromValue = EvaluateCubic(
                    cubicCoefficient, quadraticCoefficient, linearCoefficient, constantCoefficient, from);
                double toValue = EvaluateCubic(
                    cubicCoefficient, quadraticCoefficient, linearCoefficient, constantCoefficient, to);

                if ((fromValue < 0.0) == (toValue < 0.0) || fromValue == 0.0 || toValue == 0.0)
                    continue;

                for (int iteration = 0; iteration < 32; iteration++)
                {
                    double middle = (from + to) * 0.5;
                    double middleValue = EvaluateCubic(
                        cubicCoefficient, quadraticCoefficient, linearCoefficient, constantCoefficient, middle);
                    if ((middleValue < 0.0) == (fromValue < 0.0))
                    {
                        from = middle;
                        fromValue = middleValue;
                    }
                    else
                    {
                        to = middle;
                    }
                }

                candidates[candidateCount++] = (from + to) * 0.5;
            }

            PointXY bestPoint = StartPoint;
            float bestSquaredDistance = (point - bestPoint).SquaredLength;
            for (int i = 1; i < candidateCount; i++)
            {
                PointXY candidate = GetPointAtUnchecked((float)candidates[i]);
                float squaredDistance = (point - candidate).SquaredLength;
                if (squaredDistance < bestSquaredDistance)
                {
                    bestPoint = candidate;
                    bestSquaredDistance = squaredDistance;
                }
            }

            return new CurveProjection(bestPoint, MathF.Sqrt(bestSquaredDistance));
        }

        private static double EvaluateCubic(double cubic, double quadratic, double linear, double constant, double t) =>
            ((cubic * t + quadratic) * t + linear) * t + constant;

        private static void AddBoundary(Span<double> boundaries, ref int count, double value)
        {
            if (value > 0.0 && value < 1.0)
                boundaries[count++] = value;
        }
    }
}
