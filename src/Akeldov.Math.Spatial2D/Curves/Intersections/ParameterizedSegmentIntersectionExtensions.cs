using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="ParameterizedSegment"/>.
    /// </summary>
    public static class ParameterizedSegmentIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized segment and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="line">The line to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, Line line)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, line);
        }
    }
}
