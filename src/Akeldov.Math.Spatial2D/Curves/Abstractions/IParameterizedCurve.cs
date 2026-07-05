namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a curve parameterized by a curve coordinate.
    /// </summary>
    /// <remarks>
    /// Curve coordinates are measured in world coordinate units along the curve and are not normalized
    /// to the <c>[0, 1]</c> range. Each implementation defines and documents its own valid coordinate
    /// domain. For example, an infinite line can accept any finite coordinate, a ray can accept
    /// non-negative coordinates, and a finite curve can limit coordinates to its length or define
    /// wrapping behavior for closed curves.
    /// </remarks>
    public interface IParameterizedCurve : ICurve
    {
        /// <summary>
        /// Returns the point at the specified curve coordinate.
        /// </summary>
        /// <param name="curveCoordinate">The finite curve coordinate in world coordinate units.</param>
        /// <returns>The point on this curve.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="curveCoordinate"/> is NaN, infinite, or outside the valid
        /// coordinate domain defined by the implementation.
        /// </exception>
        PointXY GetPoint(float curveCoordinate);

        /// <summary>
        /// Projects the specified point onto this curve and reports the curve coordinate of the projected point.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>
        /// The projection point, its curve coordinate in the same coordinate system as <see cref="IParameterizedCurve.GetPoint"/>,
        /// and the distance to this curve.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="point"/> has a non-finite coordinate.</exception>
        ParameterizedCurveProjection ProjectWithParameter(PointXY point);
    }
}
