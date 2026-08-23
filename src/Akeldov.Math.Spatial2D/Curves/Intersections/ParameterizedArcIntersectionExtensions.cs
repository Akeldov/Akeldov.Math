using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="ParameterizedArc"/>.
    /// </summary>
    public static class ParameterizedArcIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="line">The line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, Line line)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, line);
        }
    }
}
