using Akeldov.Math.Spatial2D;
using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite line segment in two-dimensional space.
    /// </summary>
    public readonly struct ParameterizedSegment : IContourPath, IEquatable<ParameterizedSegment>
    {
        private readonly PointXY _startPoint;
        private readonly PointXY _endPoint;

        private readonly bool _includesStartPoint;
        private readonly bool _includesEndPoint;

        /// <summary>
        /// Initializes a new segment with both endpoints included.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        public ParameterizedSegment(PointXY startPoint, PointXY endPoint)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Segment endpoint coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
                nameof(endPoint),
                "Segment endpoint coordinates must be finite.");

            _startPoint = startPoint;
            _endPoint = endPoint;
            _includesStartPoint = true;
            _includesEndPoint = true;
        }

        /// <summary>
        /// Initializes a new segment with explicit endpoint inclusion.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="includesStartPoint">Whether the start point belongs to the segment.</param>
        /// <param name="includesEndPoint">Whether the end point belongs to the segment.</param>
        public ParameterizedSegment(PointXY startPoint, PointXY endPoint, bool includesStartPoint, bool includesEndPoint)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Segment endpoint coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
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
        public PointXY StartPoint => _startPoint;

        /// <summary>
        /// Gets the end point.
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
        /// Gets the segment length.
        /// </summary>
        public float Length => StartPoint.Distance(EndPoint);

        /// <summary>
        /// Gets a value indicating whether the start point belongs to the segment.
        /// </summary>
        public bool IncludesStartPoint => _includesStartPoint;

        /// <summary>
        /// Gets a value indicating whether the end point belongs to the segment.
        /// </summary>
        public bool IncludesEndPoint => _includesEndPoint;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin) =>
            ((Segment)this).CountRightwardCrossings(origin);

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
        public override int GetHashCode() => HashCode.Combine(StartPoint, EndPoint, IncludesStartPoint, IncludesEndPoint);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} - {1})", StartPoint, EndPoint);

        /// <summary>
        /// Returns the shortest distance from the specified point to this segment.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this segment.</returns>
        public float Distance(PointXY point)
        {
            return Project(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this segment.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this segment.</returns>
        public CurveProjection Project(PointXY point)
        {
            var projection = ProjectWithParameter(point);
            return new CurveProjection(projection.ProjectedPoint, projection.Distance);
        }

        /// <summary>
        /// Returns the point at the specified segment length coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units.</param>
        /// <returns>The point on this segment.</returns>
        public PointXY GetPoint(float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            float length = Length;
            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the segment length.");

            VectorXY segmentVector = EndPoint - StartPoint;
            if (length == 0f)
                return StartPoint;

            return StartPoint + (curveCoordinate / length) * segmentVector;
        }

        /// <summary>
        /// Projects the specified point onto this segment and reports the segment length coordinate.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point, segment length coordinate, and distance to this segment.</returns>
        public ParameterizedCurveProjection ProjectWithParameter(PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY segmentVector = EndPoint - StartPoint;
            VectorXY startToPoint = point - StartPoint;

            float segmentLengthSquared = segmentVector.SquaredLength;
            if (segmentLengthSquared == 0f)
                return new ParameterizedCurveProjection(StartPoint, 0f, point.Distance(StartPoint));

            float normalizedParameter = VectorXY.Dot(startToPoint, segmentVector) / segmentLengthSquared;

            if (normalizedParameter < 0f)
                normalizedParameter = 0f;
            else if (normalizedParameter > 1f)
                normalizedParameter = 1f;

            PointXY projection = StartPoint + normalizedParameter * segmentVector;
            float curveCoordinate = normalizedParameter * MathF.Sqrt(segmentLengthSquared);
            return new ParameterizedCurveProjection(projection, curveCoordinate, point.Distance(projection));
        }

        /// <summary>
        /// Indicates whether two segments are equal.
        /// </summary>
        /// <param name="left">The first segment.</param>
        /// <param name="right">The second segment.</param>
        /// <returns><see langword="true"/> if the segments are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ParameterizedSegment left, ParameterizedSegment right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two segments are different.
        /// </summary>
        /// <param name="left">The first segment.</param>
        /// <param name="right">The second segment.</param>
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
        /// Translates a segment by a vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated segment.</returns>
        public static ParameterizedSegment operator +(ParameterizedSegment left, VectorXY right) => new ParameterizedSegment(
            left.StartPoint + right,
            left.EndPoint + right,
            left.IncludesStartPoint,
            left.IncludesEndPoint);

        /// <summary>
        /// Translates a segment by the negated vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated segment.</returns>
        public static ParameterizedSegment operator -(ParameterizedSegment left, VectorXY right) => new ParameterizedSegment(
            left.StartPoint - right,
            left.EndPoint - right,
            left.IncludesStartPoint,
            left.IncludesEndPoint);
    }
}
