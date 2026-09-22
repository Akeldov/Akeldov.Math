using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite directed quadratic Bezier curve segment in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// Length and length-coordinate operations, including <see cref="ProjectWithParameter"/>, use a fixed
    /// internal polyline approximation. <see cref="Project"/> and <see cref="Distance"/> solve the original
    /// quadratic curve.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct QuadraticBezier : IFinitePath, IEquatable<QuadraticBezier>
    {
        private readonly PointXYZ _startPoint;
        private readonly PointXYZ _controlPoint;
        private readonly PointXYZ _endPoint;

        /// <summary>
        /// Initializes a new quadratic Bezier curve.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="controlPoint">The control point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a control point coordinate is NaN or infinite.
        /// </exception>
        public QuadraticBezier(PointXYZ startPoint, PointXYZ controlPoint, PointXYZ endPoint)
        {
            if (float.IsNaN(startPoint.X) || float.IsInfinity(startPoint.X) ||
                float.IsNaN(startPoint.Y) || float.IsInfinity(startPoint.Y) ||
                float.IsNaN(startPoint.Z) || float.IsInfinity(startPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(startPoint),
                    "Bezier control point coordinates must be finite.");

            if (float.IsNaN(controlPoint.X) || float.IsInfinity(controlPoint.X) ||
                float.IsNaN(controlPoint.Y) || float.IsInfinity(controlPoint.Y) ||
                float.IsNaN(controlPoint.Z) || float.IsInfinity(controlPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(controlPoint),
                    "Bezier control point coordinates must be finite.");

            if (float.IsNaN(endPoint.X) || float.IsInfinity(endPoint.X) ||
                float.IsNaN(endPoint.Y) || float.IsInfinity(endPoint.Y) ||
                float.IsNaN(endPoint.Z) || float.IsInfinity(endPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(endPoint),
                    "Bezier control point coordinates must be finite.");

            _startPoint = startPoint;
            _controlPoint = controlPoint;
            _endPoint = endPoint;
        }

        /// <summary>
        /// Gets the point at the start of the traversal direction.
        /// </summary>
        public PointXYZ StartPoint => _startPoint;

        /// <summary>
        /// Gets the quadratic Bezier control point.
        /// </summary>
        public PointXYZ ControlPoint => _controlPoint;

        /// <summary>
        /// Gets the point at the end of the traversal direction.
        /// </summary>
        public PointXYZ EndPoint => _endPoint;

        /// <summary>
        /// Gets the endpoint at the start of the traversal direction.
        /// </summary>
        public PointXYZ EndpointA => StartPoint;

        /// <summary>
        /// Gets the endpoint at the end of the traversal direction.
        /// </summary>
        public PointXYZ EndpointB => EndPoint;

        /// <summary>
        /// Gets the approximate curve length in world coordinate units.
        /// </summary>
        public float Length => BezierPathApproximation.GetLength(GetPointAtUnchecked);

        /// <summary>
        /// Returns the point at the specified normalized Bezier parameter.
        /// </summary>
        /// <param name="t">The normalized Bezier parameter in the [0, 1] range.</param>
        /// <returns>The point on this curve.</returns>
        public PointXYZ GetPointAt(float t)
        {
            if (float.IsNaN(t) || float.IsInfinity(t) || t < 0f || t > 1f)
                throw new ArgumentOutOfRangeException(
                    nameof(t),
                    "Bezier parameter must be finite and lie within the [0, 1] range.");

            return GetPointAtUnchecked(t);
        }

        /// <summary>
        /// Returns a polyline approximation of this curve.
        /// </summary>
        /// <param name="segmentCount">The positive number of line segments to create.</param>
        /// <returns>A new mutable list of directed segments owned by the caller.</returns>
#pragma warning disable MA0016 // The concrete mutable collection communicates caller ownership.
        public List<ParameterizedSegment> Flatten(int segmentCount)
        {
            return BezierPathApproximation.Flatten(GetPointAtUnchecked, segmentCount);
        }
#pragma warning restore MA0016

        /// <summary>
        /// Returns the shortest distance from the specified point to this curve.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to the original quadratic curve.</returns>
        public float Distance(PointXYZ point)
        {
            return ProjectClosest(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto the original quadratic curve.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this curve.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            return ProjectClosest(point);
        }

        /// <summary>
        /// Projects the specified point onto the curve approximation and reports the approximate length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point, length coordinate, and distance to this curve.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            return BezierPathApproximation.ProjectWithParameter(GetPointAtUnchecked, point);
        }

        /// <summary>
        /// Returns the point at the specified approximate curve length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">
        /// The finite curve coordinate in world coordinate units in the closed range from zero to <see cref="Length"/>.
        /// </param>
        /// <returns>The point on this curve.</returns>
        public PointXYZ GetPoint(float curveCoordinate)
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
            string.Format(
                CultureInfo.InvariantCulture,
                "QuadraticBezier({0}, {1}, {2})",
                StartPoint,
                ControlPoint,
                EndPoint);

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
        public static QuadraticBezier operator +(QuadraticBezier left, VectorXYZ right) => new QuadraticBezier(
            left.StartPoint + right,
            left.ControlPoint + right,
            left.EndPoint + right);

        /// <summary>
        /// Translates a curve by the negated vector.
        /// </summary>
        /// <param name="left">The curve to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated curve.</returns>
        public static QuadraticBezier operator -(QuadraticBezier left, VectorXYZ right) => new QuadraticBezier(
            left.StartPoint - right,
            left.ControlPoint - right,
            left.EndPoint - right);

        private PointXYZ GetPointAtUnchecked(float t)
        {
            if (t == 0f)
                return StartPoint;

            if (t == 1f)
                return EndPoint;

            double inverseT = 1.0 - t;
            double startAmount = inverseT * inverseT;
            double controlAmount = 2.0 * inverseT * t;
            double endAmount = (double)t * t;

            return new PointXYZ(
                (float)(StartPoint.X * startAmount + ControlPoint.X * controlAmount + EndPoint.X * endAmount),
                (float)(StartPoint.Y * startAmount + ControlPoint.Y * controlAmount + EndPoint.Y * endAmount),
                (float)(StartPoint.Z * startAmount + ControlPoint.Z * controlAmount + EndPoint.Z * endAmount));
        }

#pragma warning disable MA0051 // Keeping the cubic root isolation together makes its interval logic auditable.
        private CurveProjection ProjectClosest(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            double quadraticX = (double)StartPoint.X - 2.0 * ControlPoint.X + EndPoint.X;
            double quadraticY = (double)StartPoint.Y - 2.0 * ControlPoint.Y + EndPoint.Y;
            double quadraticZ = (double)StartPoint.Z - 2.0 * ControlPoint.Z + EndPoint.Z;
            double linearX = 2.0 * ((double)ControlPoint.X - StartPoint.X);
            double linearY = 2.0 * ((double)ControlPoint.Y - StartPoint.Y);
            double linearZ = 2.0 * ((double)ControlPoint.Z - StartPoint.Z);
            double offsetX = (double)StartPoint.X - point.X;
            double offsetY = (double)StartPoint.Y - point.Y;
            double offsetZ = (double)StartPoint.Z - point.Z;
            double cubicCoefficient = 2.0 * (
                quadraticX * quadraticX + quadraticY * quadraticY + quadraticZ * quadraticZ);
            double quadraticCoefficient = 3.0 * (
                quadraticX * linearX + quadraticY * linearY + quadraticZ * linearZ);
            double linearCoefficient =
                linearX * linearX + linearY * linearY + linearZ * linearZ +
                2.0 * (offsetX * quadraticX + offsetY * quadraticY + offsetZ * quadraticZ);
            double constantCoefficient =
                offsetX * linearX + offsetY * linearY + offsetZ * linearZ;

            Span<double> boundaries = stackalloc double[4];
            int boundaryCount = 1;
            boundaries[0] = 0.0;
            double derivativeDiscriminant =
                4.0 * quadraticCoefficient * quadraticCoefficient -
                12.0 * cubicCoefficient * linearCoefficient;

            if (cubicCoefficient != 0.0 && derivativeDiscriminant >= 0.0)
            {
                double sqrt = global::System.Math.Sqrt(derivativeDiscriminant);
                AddBoundary(
                    boundaries,
                    ref boundaryCount,
                    (-2.0 * quadraticCoefficient - sqrt) / (6.0 * cubicCoefficient));
                AddBoundary(
                    boundaries,
                    ref boundaryCount,
                    (-2.0 * quadraticCoefficient + sqrt) / (6.0 * cubicCoefficient));
            }
            else if (cubicCoefficient == 0.0 && quadraticCoefficient != 0.0)
            {
                AddBoundary(
                    boundaries,
                    ref boundaryCount,
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
                    cubicCoefficient,
                    quadraticCoefficient,
                    linearCoefficient,
                    constantCoefficient,
                    from);
                double toValue = EvaluateCubic(
                    cubicCoefficient,
                    quadraticCoefficient,
                    linearCoefficient,
                    constantCoefficient,
                    to);

                if ((fromValue < 0.0) == (toValue < 0.0) || fromValue == 0.0 || toValue == 0.0)
                    continue;

                for (int iteration = 0; iteration < 32; iteration++)
                {
                    double middle = (from + to) * 0.5;
                    double middleValue = EvaluateCubic(
                        cubicCoefficient,
                        quadraticCoefficient,
                        linearCoefficient,
                        constantCoefficient,
                        middle);
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

            PointXYZ bestPoint = StartPoint;
            double bestSquaredDistance = GetSquaredDistance(point, bestPoint);
            for (int i = 1; i < candidateCount; i++)
            {
                PointXYZ candidate = GetPointAtUnchecked((float)candidates[i]);
                double squaredDistance = GetSquaredDistance(point, candidate);
                if (squaredDistance < bestSquaredDistance)
                {
                    bestPoint = candidate;
                    bestSquaredDistance = squaredDistance;
                }
            }

            return new CurveProjection(
                bestPoint,
                (float)global::System.Math.Sqrt(bestSquaredDistance));
        }
#pragma warning restore MA0051

        private static double GetSquaredDistance(PointXYZ left, PointXYZ right)
        {
            double dx = (double)right.X - left.X;
            double dy = (double)right.Y - left.Y;
            double dz = (double)right.Z - left.Z;

            return dx * dx + dy * dy + dz * dz;
        }

        private static double EvaluateCubic(
            double cubic,
            double quadratic,
            double linear,
            double constant,
            double t) => ((cubic * t + quadratic) * t + linear) * t + constant;

        private static void AddBoundary(Span<double> boundaries, ref int count, double value)
        {
            if (value > 0.0 && value < 1.0)
                boundaries[count++] = value;
        }
    }
}
