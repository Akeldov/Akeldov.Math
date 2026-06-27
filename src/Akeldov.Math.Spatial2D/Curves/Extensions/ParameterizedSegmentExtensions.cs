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
