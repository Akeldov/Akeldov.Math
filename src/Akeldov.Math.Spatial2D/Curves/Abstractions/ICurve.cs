using Akeldov.Math.Spatial2D;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a two-dimensional curve that can measure distances to points and be intersected with rays.
    /// </summary>
    public interface ICurve : IPointDistanceProvider
    {
        /// <summary>
        /// Returns point intersections between this curve and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this curve.</param>
        /// <param name="geometryEpsilon">The geometry comparison tolerance in world coordinate units.</param>
        /// <returns>A new mutable list of intersection points in the forward direction of the ray, owned by the caller.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="geometryEpsilon"/> is negative, NaN, or infinite.</exception>
        List<PointXY> GetRayIntersections(Ray ray, float geometryEpsilon = GeometryConstants.GeometryEpsilon);

        /// <summary>
        /// Projects the specified point onto this curve.
        /// </summary>
        /// <param name="point">The finite point to project.</param>
        /// <returns>The projection point and distance to this curve.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="point"/> has a non-finite coordinate.</exception>
        CurveProjection Project(PointXY point);
    }
}
