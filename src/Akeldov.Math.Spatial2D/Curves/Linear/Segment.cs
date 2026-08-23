using Akeldov.Math.Spatial2D;
using System;
using System.Globalization;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite line segment in two-dimensional space.
    /// </summary>
    public readonly struct Segment : IFiniteTwoEndpointCurve, IRightwardCrossingProvider, IEquatable<Segment>
    {
        private readonly PointXY _endpointA;
        private readonly PointXY _endpointB;

        private readonly bool _includesEndpointA;
        private readonly bool _includesEndpointB;

        /// <summary>
        /// Initializes a new segment with both endpoints included.
        /// </summary>
        /// <param name="startPoint">The first endpoint.</param>
        /// <param name="endPoint">The second endpoint.</param>
        public Segment(PointXY startPoint, PointXY endPoint)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Segment endpoint coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
                nameof(endPoint),
                "Segment endpoint coordinates must be finite.");

            _endpointA = startPoint;
            _endpointB = endPoint;
            _includesEndpointA = true;
            _includesEndpointB = true;
        }

        /// <summary>
        /// Initializes a new segment with explicit endpoint inclusion.
        /// </summary>
        /// <param name="startPoint">The first endpoint.</param>
        /// <param name="endPoint">The second endpoint.</param>
        /// <param name="includesEndpointA">Whether the first endpoint belongs to the segment.</param>
        /// <param name="includesEndpointB">Whether the second endpoint belongs to the segment.</param>
        public Segment(PointXY startPoint, PointXY endPoint, bool includesEndpointA, bool includesEndpointB)
        {
            PointXYValidation.ThrowIfNotFinite(
                startPoint,
                nameof(startPoint),
                "Segment endpoint coordinates must be finite.");

            PointXYValidation.ThrowIfNotFinite(
                endPoint,
                nameof(endPoint),
                "Segment endpoint coordinates must be finite.");

            _endpointA = startPoint;
            _endpointB = endPoint;
            _includesEndpointA = includesEndpointA;
            _includesEndpointB = includesEndpointB;
        }

        /// <summary>
        /// Gets the segment length.
        /// </summary>
        public float Length => _endpointA.Distance(_endpointB);

        /// <summary>
        /// Gets a value indicating whether the first endpoint belongs to the segment.
        /// </summary>
        public bool IncludesEndpointA => _includesEndpointA;

        /// <summary>
        /// Gets a value indicating whether the second endpoint belongs to the segment.
        /// </summary>
        public bool IncludesEndpointB => _includesEndpointB;

        /// <summary>
        /// Gets the first endpoint.
        /// </summary>
        public PointXY EndpointA => _endpointA;

        /// <summary>
        /// Gets the second endpoint.
        /// </summary>
        public PointXY EndpointB => _endpointB;

        /// <inheritdoc/>
        public int CountRightwardCrossings(PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            PointXY lower;
            PointXY upper;
            bool includesLower;

            if (EndpointA.Y < EndpointB.Y)
            {
                lower = EndpointA;
                upper = EndpointB;
                includesLower = IncludesEndpointA;
            }
            else if (EndpointB.Y < EndpointA.Y)
            {
                lower = EndpointB;
                upper = EndpointA;
                includesLower = IncludesEndpointB;
            }
            else
            {
                return 0;
            }

            if (origin.Y < lower.Y || origin.Y >= upper.Y ||
                (origin.Y == lower.Y && !includesLower))
            {
                return 0;
            }

            float x = lower.X + (origin.Y - lower.Y) * (upper.X - lower.X) / (upper.Y - lower.Y);
            return x > origin.X ? 1 : 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Segment other && Equals(other);

        /// <summary>
        /// Indicates whether this segment has the same endpoints and endpoint-inclusion flags as another segment.
        /// </summary>
        /// <param name="other">The segment to compare with this segment.</param>
        /// <returns><see langword="true"/> if both segments are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Segment other) =>
            (EndpointA.Equals(other.EndpointA) &&
                EndpointB.Equals(other.EndpointB) &&
                IncludesEndpointA == other.IncludesEndpointA &&
                IncludesEndpointB == other.IncludesEndpointB) ||
            (EndpointA.Equals(other.EndpointB) &&
                EndpointB.Equals(other.EndpointA) &&
                IncludesEndpointA == other.IncludesEndpointB &&
                IncludesEndpointB == other.IncludesEndpointA);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(EndpointA, IncludesEndpointA) ^
            HashCode.Combine(EndpointB, IncludesEndpointB);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0} - {1})", EndpointA, EndpointB);

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
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            VectorXY segmentVector = EndpointB - EndpointA;
            VectorXY startToPoint = point - EndpointA;

            float segmentLengthSquared = segmentVector.SquaredLength;
            if (segmentLengthSquared <= GeometryConstants.GeometryEpsilonSquared)
                return new CurveProjection(EndpointA, point.Distance(EndpointA));

            float normalizedParameter = VectorXY.Dot(startToPoint, segmentVector) / segmentLengthSquared;

            if (normalizedParameter < 0f)
                normalizedParameter = 0f;
            else if (normalizedParameter > 1f)
                normalizedParameter = 1f;

            PointXY projection = EndpointA + normalizedParameter * segmentVector;
            return new CurveProjection(projection, point.Distance(projection));
        }

        /// <summary>
        /// Indicates whether two segments are equal.
        /// </summary>
        /// <param name="left">The first segment.</param>
        /// <param name="right">The second segment.</param>
        /// <returns><see langword="true"/> if the segments are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Segment left, Segment right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two segments are different.
        /// </summary>
        /// <param name="left">The first segment.</param>
        /// <param name="right">The second segment.</param>
        /// <returns><see langword="true"/> if the segments are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Segment left, Segment right) => !(left == right);

        /// <summary>
        /// Translates a segment by a vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector.</param>
        /// <returns>The translated segment.</returns>
        public static Segment operator +(Segment left, VectorXY right) => new Segment(
            left.EndpointA + right,
            left.EndpointB + right,
            left.IncludesEndpointA,
            left.IncludesEndpointB);

        /// <summary>
        /// Translates a segment by the negated vector.
        /// </summary>
        /// <param name="left">The segment to translate.</param>
        /// <param name="right">The translation vector to subtract.</param>
        /// <returns>The translated segment.</returns>
        public static Segment operator -(Segment left, VectorXY right) => new Segment(
            left.EndpointA - right,
            left.EndpointB - right,
            left.IncludesEndpointA,
            left.IncludesEndpointB);
    }
}
