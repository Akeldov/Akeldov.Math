using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helper methods for <see cref="ParameterizedSegment"/>.
    /// </summary>
    public static class ParameterizedSegmentExtensions
    {
        /// <summary>
        /// Shortens a parameterized segment by moving the start point and end point toward each other along the segment direction.
        /// </summary>
        /// <param name="segment">The segment to shorten.</param>
        /// <param name="amount">The amount removed from each end, in world coordinate units.</param>
        /// <returns>The shortened parameterized segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative, NaN, or infinite.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the segment has equal endpoints or the shorten amount is too large.</exception>
        public static ParameterizedSegment Shorten(this ParameterizedSegment segment, float amount)
        {
            if (amount < 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be finite and non-negative.");

            VectorXY segmentVector = segment.EndPoint - segment.StartPoint;
            float length = segmentVector.Length;

            if (length <= GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot shorten a segment with equal endpoints.");

            if (2f * amount > length + GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot shorten a segment by more than half its length.");

            VectorXY direction = segmentVector / length;
            PointXY startPoint = segment.StartPoint + amount * direction;
            PointXY endPoint = segment.EndPoint - amount * direction;

            return new ParameterizedSegment(
                startPoint,
                endPoint,
                segment.IncludesStartPoint,
                segment.IncludesEndPoint);
        }

        /// <summary>
        /// Extends a parameterized segment by moving the start point and end point away from each other along the segment direction.
        /// </summary>
        /// <param name="segment">The segment to extend.</param>
        /// <param name="amount">The amount added to each end, in world coordinate units.</param>
        /// <returns>The extended parameterized segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative, NaN, or infinite.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the segment has equal endpoints.</exception>
        public static ParameterizedSegment Extend(this ParameterizedSegment segment, float amount)
        {
            if (amount < 0f || float.IsNaN(amount) || float.IsInfinity(amount))
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be finite and non-negative.");

            VectorXY segmentVector = segment.EndPoint - segment.StartPoint;
            float length = segmentVector.Length;

            if (length <= GeometryConstants.GeometryEpsilon)
                throw new InvalidOperationException("Cannot extend a segment with equal endpoints.");

            VectorXY direction = segmentVector / length;
            PointXY startPoint = segment.StartPoint - amount * direction;
            PointXY endPoint = segment.EndPoint + amount * direction;

            return new ParameterizedSegment(
                startPoint,
                endPoint,
                segment.IncludesStartPoint,
                segment.IncludesEndPoint);
        }

        /// <summary>
        /// Gets the half-plane side of the supporting directed line on which the specified point lies.
        /// </summary>
        /// <param name="segment">The segment whose start-to-end direction defines the supporting directed line.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>
        /// <see cref="HalfPlaneSide.Left"/> when the point is in the left half-plane of the segment start-to-end direction,
        /// <see cref="HalfPlaneSide.Right"/> when it is in the right half-plane of that direction,
        /// or <see cref="HalfPlaneSide.OnTheLine"/> when it lies on the segment supporting line within
        /// <paramref name="geometryEpsilon"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has NaN or infinite coordinates, or when
        /// <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the segment has equal endpoints within <paramref name="geometryEpsilon"/>.</exception>
        public static HalfPlaneSide GetHalfPlaneSide(
            this ParameterizedSegment segment,
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            VectorXY segmentVector = segment.EndPoint - segment.StartPoint;
            float segmentLength = segmentVector.Length;
            if (segmentLength <= geometryEpsilon)
                throw new InvalidOperationException("Cannot determine side for a segment with equal endpoints.");

            VectorXY direction = segmentVector / segmentLength;
            float side = VectorXY.Cross(direction, point - segment.StartPoint);
            if (side.IsAlmostZero(geometryEpsilon))
                return HalfPlaneSide.OnTheLine;

            return side > 0f ? HalfPlaneSide.Left : HalfPlaneSide.Right;
        }
    }
}
