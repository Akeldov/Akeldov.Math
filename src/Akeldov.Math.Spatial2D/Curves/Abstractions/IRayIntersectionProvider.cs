using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Represents geometry that can report isolated point intersections with a ray.
    /// </summary>
    public interface IRayIntersectionProvider
    {
        /// <summary>
        /// Returns point intersections between this geometry and the specified ray.
        /// </summary>
        /// <param name="ray">The ray to intersect with this geometry.</param>
        /// <returns>A new mutable list of intersection points ordered in the forward direction of the ray, owned by the caller.</returns>
        /// <remarks>
        /// Points that belong to a continuous set of intersections are not returned.
        /// </remarks>
        List<PointXY> GetPointIntersections(Ray ray);
    }
}
