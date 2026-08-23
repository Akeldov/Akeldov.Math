using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedArc"/>.
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

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="line">The parameterized line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedLine line)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, line);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="segment">The segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, Segment segment)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, segment);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="segment">The parameterized segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedSegment segment)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, segment);
        }
    }
}
