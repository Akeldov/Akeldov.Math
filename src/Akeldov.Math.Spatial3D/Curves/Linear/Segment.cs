using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Curves namespace.
namespace Akeldov.Math.Spatial3D.Curves
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents a finite line segment in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// The default value is a zero-length segment at the global coordinate origin with both endpoints excluded.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Segment : IFiniteTwoEndpointCurve, IEquatable<Segment>
    {
        private readonly PointXYZ _endpointA;
        private readonly PointXYZ _endpointB;
        private readonly bool _includesEndpointA;
        private readonly bool _includesEndpointB;

        /// <summary>
        /// Initializes a new segment with both endpoints included.
        /// </summary>
        /// <param name="startPoint">The first endpoint.</param>
        /// <param name="endPoint">The second endpoint.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an endpoint coordinate is NaN or infinite.
        /// </exception>
        public Segment(PointXYZ startPoint, PointXYZ endPoint)
            : this(startPoint, endPoint, includesEndpointA: true, includesEndpointB: true)
        {
        }

        /// <summary>
        /// Initializes a new segment with explicit endpoint inclusion.
        /// </summary>
        /// <param name="startPoint">The first endpoint.</param>
        /// <param name="endPoint">The second endpoint.</param>
        /// <param name="includesEndpointA">Whether the first endpoint belongs to the segment.</param>
        /// <param name="includesEndpointB">Whether the second endpoint belongs to the segment.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an endpoint coordinate is NaN or infinite.
        /// </exception>
        public Segment(PointXYZ startPoint, PointXYZ endPoint, bool includesEndpointA, bool includesEndpointB)
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

            _endpointA = startPoint;
            _endpointB = endPoint;
            _includesEndpointA = includesEndpointA;
            _includesEndpointB = includesEndpointB;
        }

        /// <summary>
        /// Gets the segment length.
        /// </summary>
        public float Length => GetDistance(EndpointA, EndpointB);

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
        public PointXYZ EndpointA => _endpointA;

        /// <summary>
        /// Gets the second endpoint.
        /// </summary>
        public PointXYZ EndpointB => _endpointB;

        /// <summary>
        /// Returns the shortest distance from the specified point to this segment.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this segment.</returns>
        public float Distance(PointXYZ point)
        {
            return Project(point).Distance;
        }

        /// <summary>
        /// Projects the specified point onto this segment.
        /// </summary>
        /// <param name="point">The point to project.</param>
        /// <returns>The projection point and distance to this segment.</returns>
        public CurveProjection Project(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            double segmentX = (double)EndpointB.X - EndpointA.X;
            double segmentY = (double)EndpointB.Y - EndpointA.Y;
            double segmentZ = (double)EndpointB.Z - EndpointA.Z;
            double segmentLengthSquared =
                segmentX * segmentX +
                segmentY * segmentY +
                segmentZ * segmentZ;

            if (segmentLengthSquared == 0d)
                return new CurveProjection(EndpointA, GetDistance(point, EndpointA));

            double pointX = (double)point.X - EndpointA.X;
            double pointY = (double)point.Y - EndpointA.Y;
            double pointZ = (double)point.Z - EndpointA.Z;
            double normalizedParameter =
                (pointX * segmentX + pointY * segmentY + pointZ * segmentZ) /
                segmentLengthSquared;

            if (normalizedParameter < 0d)
                normalizedParameter = 0d;
            else if (normalizedParameter > 1d)
                normalizedParameter = 1d;

            var projection = new PointXYZ(
                (float)(EndpointA.X + normalizedParameter * segmentX),
                (float)(EndpointA.Y + normalizedParameter * segmentY),
                (float)(EndpointA.Z + normalizedParameter * segmentZ));

            return new CurveProjection(projection, GetDistance(point, projection));
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
        public static Segment operator +(Segment left, VectorXYZ right) => new Segment(
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
        public static Segment operator -(Segment left, VectorXYZ right) => new Segment(
            left.EndpointA - right,
            left.EndpointB - right,
            left.IncludesEndpointA,
            left.IncludesEndpointB);

        private static float GetDistance(PointXYZ left, PointXYZ right)
        {
            double dx = (double)right.X - left.X;
            double dy = (double)right.Y - left.Y;
            double dz = (double)right.Z - left.Z;

            return (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
