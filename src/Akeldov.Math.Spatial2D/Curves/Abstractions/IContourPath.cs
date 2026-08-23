using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents a finite directed path that provides the spatial queries required to form a contour.
    /// </summary>
    public interface IContourPath : IFinitePath, IRightwardCrossingProvider
    {
        /// <summary>
        /// Returns point intersections between this path and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this path.</param>
        /// <returns>A new mutable list of intersection points ordered in the forward direction of the ray, owned by the caller.</returns>
        /// <remarks>Points that belong to a continuous set of intersections are not returned.</remarks>
        List<PointXY> GetPointIntersections(Ray ray);
    }
}
