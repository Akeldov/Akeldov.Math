using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides intersection calculations for <see cref="ParameterizedCompositeContour"/>.
    /// </summary>
    public static class ParameterizedCompositeContourIntersectionExtensions
    {
        /// <summary>
        /// Returns distinct isolated point intersections between a parameterized composite contour and a ray.
        /// </summary>
        /// <param name="source">The source parameterized composite contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedCompositeContour source, Ray ray) =>
            CompositeContourIntersectionExtensions.GetPointIntersections(source.Curves, ray);
    }
}
