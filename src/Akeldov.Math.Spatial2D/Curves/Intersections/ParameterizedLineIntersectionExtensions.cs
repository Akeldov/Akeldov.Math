using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedLine"/>.
    /// </summary>
    public static class ParameterizedLineIntersectionExtensions
    {
        /// <summary>
        /// Returns the isolated point intersection between a parameterized line and a geometric line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized line.</param>
        /// <param name="line">The line to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. Parallel or coincident lines return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedLine source, Line line)
        {
            return LineIntersectionExtensions.GetPointIntersections(source.Line, line);
        }

        /// <summary>
        /// Returns the isolated point intersection between two parameterized lines using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized line.</param>
        /// <param name="line">The parameterized line to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. Parallel or coincident lines return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedLine source, ParameterizedLine line)
        {
            return LineIntersectionExtensions.GetPointIntersections(source.Line, line);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized line and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized line.</param>
        /// <param name="segment">The segment to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedLine source, Segment segment)
        {
            return LineIntersectionExtensions.GetPointIntersections(source.Line, segment);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized line and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized line.</param>
        /// <param name="segment">The parameterized segment to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedLine source, ParameterizedSegment segment)
        {
            return LineIntersectionExtensions.GetPointIntersections(source.Line, segment);
        }
    }
}
