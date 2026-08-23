using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedOrientedRectangleContour"/>.
    /// </summary>
    public static class ParameterizedOrientedRectangleContourIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized oriented rectangle contour and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized oriented rectangle contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedOrientedRectangleContour source, Ray ray) =>
            OrientedRectangleContourIntersectionExtensions.GetPointIntersections((OrientedRectangleContour)source, ray);
    }
}
