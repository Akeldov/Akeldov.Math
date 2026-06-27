using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helper methods for <see cref="ParameterizedLine"/>.
    /// </summary>
    public static class ParameterizedLineExtensions
    {
        /// <summary>
        /// Gets the half-plane side of this parameterized line on which the specified point lies.
        /// </summary>
        /// <param name="line">The directed parameterized line.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>
        /// <see cref="HalfPlaneSide.Left"/> when the point is in the left half-plane of increasing curve coordinates,
        /// <see cref="HalfPlaneSide.Right"/> when it is in the right half-plane of increasing curve coordinates,
        /// or <see cref="HalfPlaneSide.OnTheLine"/> when it lies on the line within <paramref name="geometryEpsilon"/>.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="point"/> has NaN or infinite coordinates, or when
        /// <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.
        /// </exception>
        public static HalfPlaneSide GetHalfPlaneSide(
            this ParameterizedLine line,
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            GeometryConstants.ValidateGeometryEpsilon(geometryEpsilon, nameof(geometryEpsilon));

            float side = VectorXY.Cross(line.Direction, point - line.Origin);
            if (side.IsAlmostZero(geometryEpsilon))
                return HalfPlaneSide.OnTheLine;

            return side > 0f ? HalfPlaneSide.Left : HalfPlaneSide.Right;
        }
    }
}
