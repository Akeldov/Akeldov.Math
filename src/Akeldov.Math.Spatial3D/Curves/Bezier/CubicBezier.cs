using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite directed cubic Bezier curve segment in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// Length and length-coordinate operations, including <see cref="ProjectWithParameter"/>, use a fixed
    /// internal polyline approximation. <see cref="Project"/> and <see cref="Distance"/> use an iterative
    /// closest-point search on the original cubic curve.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CubicBezier : IFinitePath, IEquatable<CubicBezier>
    {
        private readonly PointXYZ _startPoint;
        private readonly PointXYZ _controlPointA;
        private readonly PointXYZ _controlPointB;
        private readonly PointXYZ _endPoint;

        /// <summary>
        /// Initializes a new cubic Bezier curve.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="controlPointA">The first control point.</param>
        /// <param name="controlPointB">The second control point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a control point coordinate is NaN or infinite.
        /// </exception>
        public CubicBezier(
            PointXYZ startPoint,
            PointXYZ controlPointA,
            PointXYZ controlPointB,
            PointXYZ endPoint)
        {
            if (float.IsNaN(startPoint.X) || float.IsInfinity(startPoint.X) ||
                float.IsNaN(startPoint.Y) || float.IsInfinity(startPoint.Y) ||
                float.IsNaN(startPoint.Z) || float.IsInfinity(startPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(startPoint),
                    "Bezier control point coordinates must be finite.");

            if (float.IsNaN(controlPointA.X) || float.IsInfinity(controlPointA.X) ||
                float.IsNaN(controlPointA.Y) || float.IsInfinity(controlPointA.Y) ||
                float.IsNaN(controlPointA.Z) || float.IsInfinity(controlPointA.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(controlPointA),
                    "Bezier control point coordinates must be finite.");

            if (float.IsNaN(controlPointB.X) || float.IsInfinity(controlPointB.X) ||
                float.IsNaN(controlPointB.Y) || float.IsInfinity(controlPointB.Y) ||
                float.IsNaN(controlPointB.Z) || float.IsInfinity(controlPointB.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(controlPointB),
                    "Bezier control point coordinates must be finite.");

            if (float.IsNaN(endPoint.X) || float.IsInfinity(endPoint.X) ||
                float.IsNaN(endPoint.Y) || float.IsInfinity(endPoint.Y) ||
                float.IsNaN(endPoint.Z) || float.IsInfinity(endPoint.Z))
                throw new ArgumentOutOfRangeException(
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
        public PointXYZ StartPoint => _startPoint;

        /// <summary>
        /// Gets the first cubic Bezier control point.
        /// </summary>
        public PointXYZ ControlPointA => _controlPointA;

        /// <summary>
        /// Gets the second cubic Bezier control point.
        /// </summary>
        public PointXYZ ControlPointB => _controlPointB;

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
        /// Returns the shortest approximate distance from the specified point to this curve.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The approximate distance to the original cubic curve.</returns>
        public float Distance(PointXYZ point)
        {
            return ProjectClosest(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto the original cubic curve using an iterative search.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The approximate projection point and distance to this curve.</returns>
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
        public override int GetHashCode() =>
            HashCode.Combine(StartPoint, ControlPointA, ControlPointB, EndPoint);

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
        public static CubicBezier operator +(CubicBezier left, VectorXYZ right) => new CubicBezier(
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
        public static CubicBezier operator -(CubicBezier left, VectorXYZ right) => new CubicBezier(
            left.StartPoint - right,
            left.ControlPointA - right,
            left.ControlPointB - right,
            left.EndPoint - right);

        private PointXYZ GetPointAtUnchecked(float t)
        {
            if (t == 0f)
                return StartPoint;

            if (t == 1f)
                return EndPoint;

            double inverseT = 1.0 - t;
            double inverseTSquared = inverseT * inverseT;
            double tSquared = (double)t * t;
            double startAmount = inverseTSquared * inverseT;
            double controlAAmount = 3.0 * inverseTSquared * t;
            double controlBAmount = 3.0 * inverseT * tSquared;
            double endAmount = tSquared * t;

            return new PointXYZ(
                (float)(
                    StartPoint.X * startAmount +
                    ControlPointA.X * controlAAmount +
                    ControlPointB.X * controlBAmount +
                    EndPoint.X * endAmount),
                (float)(
                    StartPoint.Y * startAmount +
                    ControlPointA.Y * controlAAmount +
                    ControlPointB.Y * controlBAmount +
                    EndPoint.Y * endAmount),
                (float)(
                    StartPoint.Z * startAmount +
                    ControlPointA.Z * controlAAmount +
                    ControlPointB.Z * controlBAmount +
                    EndPoint.Z * endAmount));
        }

#pragma warning disable MA0051 // Keeping the Newton iteration and candidate comparison together aids review.
        private CurveProjection ProjectClosest(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            PointXYZ bestPoint = StartPoint;
            double bestSquaredDistance = GetSquaredDistance(point, bestPoint);

            double startToControlAX = (double)ControlPointA.X - StartPoint.X;
            double startToControlAY = (double)ControlPointA.Y - StartPoint.Y;
            double startToControlAZ = (double)ControlPointA.Z - StartPoint.Z;
            double controlAToControlBX = (double)ControlPointB.X - ControlPointA.X;
            double controlAToControlBY = (double)ControlPointB.Y - ControlPointA.Y;
            double controlAToControlBZ = (double)ControlPointB.Z - ControlPointA.Z;
            double controlBToEndX = (double)EndPoint.X - ControlPointB.X;
            double controlBToEndY = (double)EndPoint.Y - ControlPointB.Y;
            double controlBToEndZ = (double)EndPoint.Z - ControlPointB.Z;

            for (int seedIndex = 0; seedIndex <= 8; seedIndex++)
            {
                double parameter = seedIndex * 0.125;

                for (int iteration = 0; iteration < 6; iteration++)
                {
                    double inverse = 1.0 - parameter;
                    PointXYZ curvePoint = GetPointAtUnchecked((float)parameter);
                    double firstDerivativeX = 3.0 * (
                        inverse * inverse * startToControlAX +
                        2.0 * inverse * parameter * controlAToControlBX +
                        parameter * parameter * controlBToEndX);
                    double firstDerivativeY = 3.0 * (
                        inverse * inverse * startToControlAY +
                        2.0 * inverse * parameter * controlAToControlBY +
                        parameter * parameter * controlBToEndY);
                    double firstDerivativeZ = 3.0 * (
                        inverse * inverse * startToControlAZ +
                        2.0 * inverse * parameter * controlAToControlBZ +
                        parameter * parameter * controlBToEndZ);
                    double secondDerivativeX = 6.0 * (
                        inverse * (controlAToControlBX - startToControlAX) +
                        parameter * (controlBToEndX - controlAToControlBX));
                    double secondDerivativeY = 6.0 * (
                        inverse * (controlAToControlBY - startToControlAY) +
                        parameter * (controlBToEndY - controlAToControlBY));
                    double secondDerivativeZ = 6.0 * (
                        inverse * (controlAToControlBZ - startToControlAZ) +
                        parameter * (controlBToEndZ - controlAToControlBZ));
                    double deltaX = (double)curvePoint.X - point.X;
                    double deltaY = (double)curvePoint.Y - point.Y;
                    double deltaZ = (double)curvePoint.Z - point.Z;
                    double denominator =
                        firstDerivativeX * firstDerivativeX +
                        firstDerivativeY * firstDerivativeY +
                        firstDerivativeZ * firstDerivativeZ +
                        deltaX * secondDerivativeX +
                        deltaY * secondDerivativeY +
                        deltaZ * secondDerivativeZ;
                    if (denominator == 0d)
                        break;

                    double numerator =
                        deltaX * firstDerivativeX +
                        deltaY * firstDerivativeY +
                        deltaZ * firstDerivativeZ;
                    double next = parameter - numerator / denominator;
                    parameter = global::System.Math.Max(0d, global::System.Math.Min(1d, next));
                }

                PointXYZ candidate = GetPointAtUnchecked((float)parameter);
                double squaredDistance = GetSquaredDistance(point, candidate);
                if (squaredDistance < bestSquaredDistance)
                {
                    bestPoint = candidate;
                    bestSquaredDistance = squaredDistance;
                }
            }

            PointXYZ end = EndPoint;
            double endSquaredDistance = GetSquaredDistance(point, end);
            if (endSquaredDistance < bestSquaredDistance)
            {
                bestPoint = end;
                bestSquaredDistance = endSquaredDistance;
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
    }
}
