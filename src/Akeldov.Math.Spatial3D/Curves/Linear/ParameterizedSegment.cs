using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a directed finite line segment parameterized by distance from its start point.
    /// </summary>
    /// <remarks>
    /// The default value is a zero-length path at the global coordinate origin with both endpoints excluded.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ParameterizedSegment : IFinitePath, IEquatable<ParameterizedSegment>
    {
        private readonly PointXYZ _startPoint;
        private readonly PointXYZ _endPoint;
        private readonly bool _includesStartPoint;
        private readonly bool _includesEndPoint;

        /// <summary>
        /// Initializes a new parameterized segment with both endpoints included.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an endpoint coordinate is NaN or infinite.
        /// </exception>
        public ParameterizedSegment(PointXYZ startPoint, PointXYZ endPoint)
            : this(startPoint, endPoint, includesStartPoint: true, includesEndPoint: true)
        {
        }

        /// <summary>
        /// Initializes a new parameterized segment with explicit endpoint inclusion.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="includesStartPoint">Whether the start point belongs to the segment.</param>
        /// <param name="includesEndPoint">Whether the end point belongs to the segment.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an endpoint coordinate is NaN or infinite.
        /// </exception>
        public ParameterizedSegment(
            PointXYZ startPoint,
            PointXYZ endPoint,
            bool includesStartPoint,
            bool includesEndPoint)
        {
            if (float.IsNaN(startPoint.X) || float.IsInfinity(startPoint.X) ||
                float.IsNaN(startPoint.Y) || float.IsInfinity(startPoint.Y) ||
                float.IsNaN(startPoint.Z) || float.IsInfinity(startPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(startPoint),
                    "Segment endpoint coordinates must be finite.");

            if (float.IsNaN(endPoint.X) || float.IsInfinity(endPoint.X) ||
                float.IsNaN(endPoint.Y) || float.IsInfinity(endPoint.Y) ||
                float.IsNaN(endPoint.Z) || float.IsInfinity(endPoint.Z))
                throw new ArgumentOutOfRangeException(
                    nameof(endPoint),
                    "Segment endpoint coordinates must be finite.");

            _startPoint = startPoint;
            _endPoint = endPoint;
            _includesStartPoint = includesStartPoint;
            _includesEndPoint = includesEndPoint;
        }

        /// <summary>
        /// Gets the start point.
        /// </summary>
        public PointXYZ StartPoint => _startPoint;

        /// <summary>
        /// Gets the end point.
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
        /// Gets the segment length.
        /// </summary>
        public float Length => (float)GetLength();

        /// <summary>
        /// Gets a value indicating whether the start point belongs to the segment.
        /// </summary>
        public bool IncludesStartPoint => _includesStartPoint;

        /// <summary>
        /// Gets a value indicating whether the end point belongs to the segment.
        /// </summary>
        public bool IncludesEndPoint => _includesEndPoint;

        /// <summary>
        /// Returns the shortest distance from the specified point to this segment.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this segment.</returns>
        public float Distance(PointXYZ point)
        {
            return ProjectWithParameter(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this segment.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this segment.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            ParameterizedCurveProjection projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Returns the point at the specified segment length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">
        /// The finite curve coordinate in world coordinate units in the closed range from zero to <see cref="Length"/>.
        /// </param>
        /// <returns>The point on this segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN, infinite, negative, or greater than <see cref="Length"/>.
        /// </exception>
        public PointXYZ GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            double exactLength = GetLength();
            float length = (float)exactLength;
            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(
                    nameof(curveCoordinate),
                    "Curve coordinate must lie within the segment length.");

            if (curveCoordinate == 0f || exactLength == 0d)
                return StartPoint;

            if (curveCoordinate == length)
                return EndPoint;

            double normalizedParameter = curveCoordinate / exactLength;
            return new PointXYZ(
                (float)(StartPoint.X + normalizedParameter * ((double)EndPoint.X - StartPoint.X)),
                (float)(StartPoint.Y + normalizedParameter * ((double)EndPoint.Y - StartPoint.Y)),
                (float)(StartPoint.Z + normalizedParameter * ((double)EndPoint.Z - StartPoint.Z)));
        }

        /// <summary>
        /// Projects the specified point onto this segment and reports the segment length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, segment length coordinate, and distance to this segment.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            double segmentX = (double)EndPoint.X - StartPoint.X;
            double segmentY = (double)EndPoint.Y - StartPoint.Y;
            double segmentZ = (double)EndPoint.Z - StartPoint.Z;
            double segmentLengthSquared =
                segmentX * segmentX +
                segmentY * segmentY +
                segmentZ * segmentZ;

            if (segmentLengthSquared == 0d)
                return new ParameterizedCurveProjection(StartPoint, 0f, GetDistance(point, StartPoint));

            double pointX = (double)point.X - StartPoint.X;
            double pointY = (double)point.Y - StartPoint.Y;
            double pointZ = (double)point.Z - StartPoint.Z;
            double normalizedParameter =
                (pointX * segmentX + pointY * segmentY + pointZ * segmentZ) /
                segmentLengthSquared;

            if (normalizedParameter < 0d)
                normalizedParameter = 0d;
            else if (normalizedParameter > 1d)
                normalizedParameter = 1d;

            var projection = new PointXYZ(
                (float)(StartPoint.X + normalizedParameter * segmentX),
                (float)(StartPoint.Y + normalizedParameter * segmentY),
                (float)(StartPoint.Z + normalizedParameter * segmentZ));
            float curveCoordinate = (float)(normalizedParameter * global::System.Math.Sqrt(segmentLengthSquared));

            return new ParameterizedCurveProjection(
                projection,
                curveCoordinate,
                GetDistance(point, projection));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ParameterizedSegment other && Equals(other);

        /// <summary>
        /// Indicates whether this segment has the same directed endpoints and endpoint-inclusion flags as another segment.
        /// </summary>
        /// <param name="other">The segment to compare with this segment.</param>
        /// <returns><see langword="true"/> if both segments are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ParameterizedSegment other) =>
            StartPoint.Equals(other.StartPoint) &&
            EndPoint.Equals(other.EndPoint) &&
            IncludesStartPoint == other.IncludesStartPoint &&
            IncludesEndPoint == other.IncludesEndPoint;

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(StartPoint, EndPoint, IncludesStartPoint, IncludesEndPoint);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} - {1})", StartPoint, EndPoint);

        /// <summary>
        /// Indicates whether two parameterized segments are equal.
        /// </summary>
        /// <param name="left">The first parameterized segment.</param>
        /// <param name="right">The second parameterized segment.</param>
        /// <returns><see langword="true"/> if the segments are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedSegment left, ParameterizedSegment right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two parameterized segments are different.
        /// </summary>
        /// <param name="left">The first parameterized segment.</param>
        /// <param name="right">The second parameterized segment.</param>
        /// <returns><see langword="true"/> if the segments are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ParameterizedSegment left, ParameterizedSegment right) => !(left == right);

        /// <summary>
        /// Converts a parameterized segment to its geometric segment.
        /// </summary>
        /// <param name="segment">The parameterized segment to convert.</param>
        public static explicit operator Segment(ParameterizedSegment segment)
        {
            return new Segment(
                segment.StartPoint,
                segment.EndPoint,
                segment.IncludesStartPoint,
                segment.IncludesEndPoint);
        }

        /// <summary>
        /// Translates a parameterized segment by a vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated segment.</returns>
        public static ParameterizedSegment operator +(ParameterizedSegment left, VectorXYZ right) =>
            new ParameterizedSegment(
                left.StartPoint + right,
                left.EndPoint + right,
                left.IncludesStartPoint,
                left.IncludesEndPoint);

        /// <summary>
        /// Translates a parameterized segment by the negated vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated segment.</returns>
        public static ParameterizedSegment operator -(ParameterizedSegment left, VectorXYZ right) =>
            new ParameterizedSegment(
                left.StartPoint - right,
                left.EndPoint - right,
                left.IncludesStartPoint,
                left.IncludesEndPoint);

        private double GetLength()
        {
            double dx = (double)EndPoint.X - StartPoint.X;
            double dy = (double)EndPoint.Y - StartPoint.Y;
            double dz = (double)EndPoint.Z - StartPoint.Z;

            return global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static float GetDistance(PointXYZ left, PointXYZ right)
        {
            double dx = (double)right.X - left.X;
            double dy = (double)right.Y - left.Y;
            double dz = (double)right.Z - left.Z;

            return (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
