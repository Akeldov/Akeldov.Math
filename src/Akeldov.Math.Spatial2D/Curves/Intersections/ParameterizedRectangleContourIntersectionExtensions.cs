using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedRectangleContour"/>.
    /// </summary>
    public static class ParameterizedRectangleContourIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized rectangle contour and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized rectangle contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedRectangleContour source, Ray ray) =>
            RectangleContourIntersectionExtensions.GetPointIntersections((RectangleContour)source, ray);
    }
}
