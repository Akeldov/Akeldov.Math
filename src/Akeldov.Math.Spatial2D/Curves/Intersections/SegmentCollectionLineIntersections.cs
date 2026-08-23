using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line and segment intersection calculations shared by curves represented as segment collections.
    /// </summary>
    internal static class SegmentCollectionLineIntersections
    {
        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a line using exact comparisons.
        /// </summary>
        /// <param name="segments">The segments to intersect.</param>
        /// <param name="line">The intersecting line.</param>
        /// <returns>A new mutable list ordered in the canonical direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, Line line)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < segments.Count; i++)
            {
                List<PointXY> segmentIntersections =
                    ParameterizedSegmentIntersectionExtensions.GetPointIntersections(segments[i], line);

                for (int j = 0; j < segmentIntersections.Count; j++)
                {
                    PointXY intersection = segmentIntersections[j];
                    if (!BelongsToContinuousOverlap(segments, intersection, line) &&
                        !intersections.Contains(intersection))
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - line.ClosestPointToOrigin, line.Direction).CompareTo(
                    VectorXY.Dot(right - line.ClosestPointToOrigin, line.Direction)));

            return intersections;
        }

        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="segments">The segments to intersect.</param>
        /// <param name="line">The intersecting parameterized line.</param>
        /// <returns>A new mutable list ordered in the parameterized direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(segments, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a segment using exact comparisons.
        /// </summary>
        /// <param name="segments">The source segments to intersect.</param>
        /// <param name="segment">The segment to intersect with the source collection.</param>
        /// <returns>A new mutable list ordered from the second segment's first endpoint to its second endpoint.</returns>
        public static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, Segment segment)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < segments.Count; i++)
            {
                List<PointXY> segmentIntersections =
                    ParameterizedSegmentIntersectionExtensions.GetPointIntersections(segments[i], segment);

                for (int j = 0; j < segmentIntersections.Count; j++)
                {
                    PointXY intersection = segmentIntersections[j];
                    if (!BelongsToContinuousOverlap(segments, intersection, segment) &&
                        !intersections.Contains(intersection))
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            SegmentIntersectionExtensions.RestrictToSegment(intersections, segment);
            return intersections;
        }

        /// <summary>
        /// Determines whether a point belongs to a segment that continuously overlaps the line.
        /// </summary>
        /// <param name="segments">The segments to inspect.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="line">The intersecting line.</param>
        /// <returns><see langword="true"/> when the point belongs to a continuously overlapping segment; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousOverlap(IReadOnlyList<ParameterizedSegment> segments, PointXY point, Line line)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                ParameterizedSegment segment = segments[i];
                if (GetSignedDistance(line, segment.StartPoint) != 0f ||
                    GetSignedDistance(line, segment.EndPoint) != 0f)
                {
                    continue;
                }

                VectorXY segmentDirection = segment.EndPoint - segment.StartPoint;
                VectorXY startToPoint = point - segment.StartPoint;
                float pointCoordinate = VectorXY.Dot(startToPoint, segmentDirection);

                if (VectorXY.Cross(segmentDirection, startToPoint) == 0f &&
                    pointCoordinate >= 0f &&
                    pointCoordinate <= segmentDirection.SquaredLength)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a point belongs to a source segment that continuously overlaps the intersecting segment.
        /// </summary>
        /// <param name="segments">The source segments to inspect.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="segment">The intersecting segment.</param>
        /// <returns><see langword="true"/> when the point belongs to a continuous overlap; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousOverlap(IReadOnlyList<ParameterizedSegment> segments, PointXY point, Segment segment)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Segment sourceSegment = (Segment)segments[i];
                if (SegmentIntersectionExtensions.HasContinuousOverlap(sourceSegment, segment) &&
                    SegmentIntersectionExtensions.IncludesPoint(sourceSegment, point) &&
                    SegmentIntersectionExtensions.IncludesPoint(segment, point))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the signed distance from a point to a normalized line equation.
        /// </summary>
        /// <param name="line">The line that defines the signed-distance function.</param>
        /// <param name="point">The point to evaluate.</param>
        /// <returns>The signed distance in world coordinate units.</returns>
        private static float GetSignedDistance(Line line, PointXY point) =>
            line.EquationA * point.X + line.EquationB * point.Y + line.EquationC;
    }
}
