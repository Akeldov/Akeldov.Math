using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helper methods for <see cref="Segment"/>.
    /// </summary>
    public static class SegmentExtensions
    {
        /// <summary>
        /// Shortens a segment by moving each endpoint toward the other endpoint.
        /// </summary>
        /// <param name="segment">The segment to shorten.</param>
        /// <param name="amount">The amount removed from each end.</param>
        /// <returns>The shortened segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative, NaN, or infinite.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the segment has equal endpoints or the shorten amount is too large.</exception>
        public static Segment Shorten(this Segment segment, float amount)
        {
            if (amount < 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be finite and non-negative.");

            VectorXY segmentVector = segment.EndpointB - segment.EndpointA;
            float length = segmentVector.Length;

            if (length <= GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot shorten a segment with equal endpoints.");

            if (2f * amount > length + GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot shorten a segment by more than half its length.");

            VectorXY direction = segmentVector / length;
            PointXY endpointA = segment.EndpointA + amount * direction;
            PointXY endpointB = segment.EndpointB - amount * direction;

            return new Segment(
                endpointA,
                endpointB,
                segment.IncludesEndpointA,
                segment.IncludesEndpointB);
        }

        /// <summary>
        /// Extends a segment by moving each endpoint away from the other endpoint.
        /// </summary>
        /// <param name="segment">The segment to extend.</param>
        /// <param name="amount">The amount added to each end.</param>
        /// <returns>The extended segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative, NaN, or infinite.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the segment has equal endpoints.</exception>
        public static Segment Extend(this Segment segment, float amount)
        {
            if (amount < 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be finite and non-negative.");

            VectorXY segmentVector = segment.EndpointB - segment.EndpointA;
            float length = segmentVector.Length;

            if (length <= GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot extend a segment with equal endpoints.");

            VectorXY direction = segmentVector / length;
            PointXY endpointA = segment.EndpointA - amount * direction;
            PointXY endpointB = segment.EndpointB + amount * direction;

            return new Segment(
                endpointA,
                endpointB,
                segment.IncludesEndpointA,
                segment.IncludesEndpointB);
        }
    }
}
