using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedSegment"/>.
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

        /// <summary>
        /// Returns isolated point intersections between a parameterized segment and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="line">The parameterized line to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, ParameterizedLine line)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, line);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized segment and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="segment">The segment to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second segment's first endpoint to its second endpoint. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, Segment segment)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, segment);
        }

        /// <summary>
        /// Returns isolated point intersections between two parameterized segments using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="segment">The parameterized segment to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second segment's start point to its end point. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, ParameterizedSegment segment)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a parameterized segment and a parameterized segment chain using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, ParameterizedSegmentChain segmentChain)
        {
            List<PointXY> intersections = ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain.Segments, source);
            return ParameterizedSegmentChainIntersectionExtensions.OrderPointIntersections(segmentChain, intersections);
        }

        /// <summary>
        /// Returns the isolated point intersections between a parameterized segment and an arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="arc">The arc to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, Arc arc)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, arc);
        }

        /// <summary>
        /// Returns the isolated point intersections between a parameterized segment and a parameterized arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized segment.</param>
        /// <param name="arc">The parameterized arc to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegment source, ParameterizedArc arc)
        {
            return SegmentIntersectionExtensions.GetPointIntersections((Segment)source, arc);
        }
    }
}
