namespace Akeldov.Math.Spatial2D.Curves
{
    using System;

    /// <summary>
    /// Represents the result of projecting a point onto a parameterized curve.
    /// </summary>
    public readonly struct ParameterizedCurveProjection
    {
        /// <summary>
        /// Initializes a new parameterized projection result.
        /// </summary>
        /// <param name="projectedPoint">The projected point on the curve.</param>
        /// <param name="curveCoordinate">The curve coordinate of the projected point, measured in length units.</param>
        /// <param name="distance">The distance from the original point to the projected point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN, or when <paramref name="distance"/> is negative, NaN, or infinite.
        /// </exception>
        public ParameterizedCurveProjection(PointXY projectedPoint, float curveCoordinate, float distance)
        {
            PointXYValidation.ThrowIfNotFinite(
                projectedPoint,
                nameof(projectedPoint),
                "Projected point coordinates must be finite.");

            if (float.IsNaN(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must not be NaN.");

            if (distance < 0f || float.IsNaN(distance) || float.IsInfinity(distance))
                throw new ArgumentOutOfRangeException(nameof(distance), "Projection distance must be finite and non-negative.");

            ProjectedPoint = projectedPoint;
            CurveCoordinate = curveCoordinate;
            Distance = distance;
        }

        /// <summary>
        /// Gets the projected point on the curve.
        /// </summary>
        public PointXY ProjectedPoint { get; }

        /// <summary>
        /// Gets the curve coordinate of the projected point, measured in length units.
        /// </summary>
        /// <remarks>
        /// <para>For parameterized lines this is signed distance from <see cref="ParameterizedLine.Origin"/> along <see cref="ParameterizedLine.Direction"/>.</para>
        /// <para>For rays this is distance from <see cref="Ray.Origin"/> along <see cref="Ray.Direction"/>.</para>
        /// <para>For segments this is distance from <see cref="ParameterizedSegment.StartPoint"/> along the segment.</para>
        /// <para>For arcs this is arc length from the start point along the arc.</para>
        /// </remarks>
        public float CurveCoordinate { get; }

        /// <summary>
        /// Gets the distance from the original point to the projected point.
        /// </summary>
        public float Distance { get; }
    }
}
