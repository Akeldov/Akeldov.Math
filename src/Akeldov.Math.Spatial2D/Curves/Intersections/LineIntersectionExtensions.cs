using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="Line"/>.
    /// </summary>
    public static class LineIntersectionExtensions
    {
        /// <summary>
        /// Returns the isolated point intersection between two lines using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="line">The line to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. Parallel or coincident lines return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Line line)
        {
            VectorXY sourceDirection = source.Direction;
            VectorXY lineDirection = line.Direction;
            float cross = VectorXY.Cross(sourceDirection, lineDirection);

            if (cross == 0f)
                return new List<PointXY>();

            VectorXY originDelta = line.ClosestPointToOrigin - source.ClosestPointToOrigin;
            float sourceCoordinate = VectorXY.Cross(originDelta, lineDirection) / cross;
            PointXY intersection = source.ClosestPointToOrigin + sourceCoordinate * sourceDirection;

            return new List<PointXY> { intersection };
        }

        /// <summary>
        /// Returns the isolated point intersection between a line and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="line">The parameterized line to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. Parallel or coincident lines return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, ParameterizedLine line)
        {
            return GetPointIntersections(source, line.Line);
        }

        /// <summary>
        /// Returns isolated point intersections between a line and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="segment">The segment to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Segment segment)
        {
            return SegmentIntersectionExtensions.GetPointIntersections(segment, source);
        }

        /// <summary>
        /// Returns isolated point intersections between a line and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="segment">The parameterized segment to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source, (Segment)segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a line and a parameterized segment chain using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, ParameterizedSegmentChain segmentChain)
        {
            List<PointXY> intersections = ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain.Segments, source);
            return ParameterizedSegmentChainIntersectionExtensions.OrderPointIntersections(segmentChain, intersections);
        }
    }
}
