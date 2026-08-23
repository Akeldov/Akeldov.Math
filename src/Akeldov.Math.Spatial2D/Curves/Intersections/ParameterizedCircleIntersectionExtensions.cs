using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedCircle"/>.
    /// </summary>
    public static class ParameterizedCircleIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized circle and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized circle.</param>
        /// <param name="ray">The ray to intersect with the source circle.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedCircle source, Ray ray) =>
            CircleIntersectionExtensions.GetPointIntersections(source.Circle, ray);
    }
}
