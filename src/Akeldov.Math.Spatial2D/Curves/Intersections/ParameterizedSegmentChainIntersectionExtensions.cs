using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="ParameterizedSegmentChain"/>.
    /// </summary>
    public static class ParameterizedSegmentChainIntersectionExtensions
    {
        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="line">The line to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, Line line)
        {
            return GetPointIntersections(source.Segments, line);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="line">The parameterized line to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, ParameterizedLine line)
        {
            return GetPointIntersections(source.Segments, line);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="segment">The segment to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second segment's first endpoint to its second endpoint. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, Segment segment)
        {
            return GetPointIntersections(source.Segments, segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="segment">The parameterized segment to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source.Segments, segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between two parameterized segment chains using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second chain's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, ParameterizedSegmentChain segmentChain)
        {
            return GetPointIntersections(source.Segments, segmentChain);
        }

        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a line using exact comparisons.
        /// </summary>
        /// <param name="segments">The segments to intersect.</param>
        /// <param name="line">The intersecting line.</param>
        /// <returns>A new mutable list ordered in the canonical direction of the line.</returns>
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, Line line)
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
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(segments, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a ray using exact comparisons.
        /// </summary>
        /// <param name="segments">The segments to intersect.</param>
        /// <param name="ray">The intersecting ray.</param>
        /// <returns>A new mutable list ordered in the forward direction of the ray.</returns>
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, Ray ray)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < segments.Count; i++)
            {
                List<PointXY> segmentIntersections =
                    RayIntersectionExtensions.GetPointIntersections(ray, segments[i]);

                for (int j = 0; j < segmentIntersections.Count; j++)
                {
                    PointXY intersection = segmentIntersections[j];
                    if (!BelongsToContinuousOverlap(segments, intersection, ray) &&
                        !intersections.Contains(intersection))
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - ray.Origin, ray.Direction).CompareTo(
                    VectorXY.Dot(right - ray.Origin, ray.Direction)));

            return intersections;
        }

        /// <summary>
        /// Returns distinct isolated point intersections between a segment collection and a segment using exact comparisons.
        /// </summary>
        /// <param name="segments">The source segments to intersect.</param>
        /// <param name="segment">The segment to intersect with the source collection.</param>
        /// <returns>A new mutable list ordered from the second segment's first endpoint to its second endpoint.</returns>
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, Segment segment)
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
        /// Returns distinct isolated point intersections between a segment collection and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="segments">The source segments to intersect.</param>
        /// <param name="segment">The parameterized segment to intersect with the source collection.</param>
        /// <returns>A new mutable list ordered from the parameterized segment's start point to its end point.</returns>
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> segments, ParameterizedSegment segment)
        {
            return GetPointIntersections(segments, (Segment)segment);
        }

        /// <summary>
        /// Returns distinct isolated intersections with the chain by intersecting its directed segments in traversal order.
        /// </summary>
        /// <param name="segmentChain">The target segment chain.</param>
        /// <param name="getSegmentIntersections">The exact intersection calculation for one directed chain segment.</param>
        /// <returns>A new mutable list ordered from the chain's start point to its end point.</returns>
        internal static List<PointXY> GetPointIntersections(ParameterizedSegmentChain segmentChain, Func<ParameterizedSegment, List<PointXY>> getSegmentIntersections)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < segmentChain.Segments.Count; i++)
            {
                List<PointXY> segmentIntersections = getSegmentIntersections(segmentChain.Segments[i]);
                for (int j = 0; j < segmentIntersections.Count; j++)
                {
                    PointXY intersection = segmentIntersections[j];
                    if (!intersections.Contains(intersection))
                        intersections.Add(intersection);
                }
            }

            return intersections;
        }

        /// <summary>
        /// Orders distinct known intersections along a parameterized segment chain.
        /// </summary>
        /// <param name="segmentChain">The target segment chain.</param>
        /// <param name="unorderedIntersections">The known intersections with the chain.</param>
        /// <returns>A new mutable list ordered from the chain's start point to its end point.</returns>
        internal static List<PointXY> OrderPointIntersections(ParameterizedSegmentChain segmentChain, IReadOnlyList<PointXY> unorderedIntersections)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < unorderedIntersections.Count; i++)
            {
                PointXY intersection = unorderedIntersections[i];
                if (!intersections.Contains(intersection))
                    intersections.Add(intersection);
            }

            intersections.Sort((left, right) =>
                segmentChain.ProjectWithParameter(left).CurveCoordinate.CompareTo(
                    segmentChain.ProjectWithParameter(right).CurveCoordinate));

            return intersections;
        }

        /// <summary>
        /// Returns distinct isolated intersections between a source segment collection and a target chain.
        /// </summary>
        /// <param name="sourceSegments">The source segments.</param>
        /// <param name="segmentChain">The target segment chain.</param>
        /// <returns>A new mutable list ordered from the target chain's start point to its end point.</returns>
        internal static List<PointXY> GetPointIntersections(IReadOnlyList<ParameterizedSegment> sourceSegments, ParameterizedSegmentChain segmentChain)
        {
            List<PointXY> intersections = GetPointIntersections(
                segmentChain,
                segment => GetPointIntersections(sourceSegments, segment));

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (BelongsToContinuousOverlap(sourceSegments, segmentChain.Segments, intersections[i]))
                    intersections.RemoveAt(i);
            }

            return intersections;
        }

        /// <summary>
        /// Determines whether a point belongs to a continuous overlap between two segment collections.
        /// </summary>
        /// <param name="sourceSegments">The source segments.</param>
        /// <param name="targetSegments">The target segments.</param>
        /// <param name="point">The point to classify.</param>
        /// <returns><see langword="true"/> when the point belongs to a continuous overlap; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousOverlap(IReadOnlyList<ParameterizedSegment> sourceSegments, IReadOnlyList<ParameterizedSegment> targetSegments, PointXY point)
        {
            for (int i = 0; i < sourceSegments.Count; i++)
            {
                Segment sourceSegment = (Segment)sourceSegments[i];
                for (int j = 0; j < targetSegments.Count; j++)
                {
                    Segment targetSegment = (Segment)targetSegments[j];
                    if (SegmentIntersectionExtensions.HasContinuousOverlap(sourceSegment, targetSegment) &&
                        SegmentIntersectionExtensions.IncludesPoint(sourceSegment, point) &&
                        SegmentIntersectionExtensions.IncludesPoint(targetSegment, point))
                    {
                        return true;
                    }
                }
            }

            return false;
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
        /// Determines whether a point belongs to a source segment that continuously overlaps a ray.
        /// </summary>
        /// <param name="segments">The source segments to inspect.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="ray">The intersecting ray.</param>
        /// <returns><see langword="true"/> when the point belongs to a continuous overlap; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousOverlap(IReadOnlyList<ParameterizedSegment> segments, PointXY point, Ray ray)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Segment segment = (Segment)segments[i];
                VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;
                VectorXY originToEndpointA = segment.EndpointA - ray.Origin;

                if (VectorXY.Cross(segmentDirection, ray.Direction) != 0f ||
                    VectorXY.Cross(originToEndpointA, ray.Direction) != 0f)
                {
                    continue;
                }

                float endpointACoordinate = VectorXY.Dot(originToEndpointA, ray.Direction);
                float endpointBCoordinate = VectorXY.Dot(segment.EndpointB - ray.Origin, ray.Direction);
                float segmentStart = endpointACoordinate < endpointBCoordinate ? endpointACoordinate : endpointBCoordinate;
                float segmentEnd = endpointACoordinate > endpointBCoordinate ? endpointACoordinate : endpointBCoordinate;
                float overlapStart = segmentStart > 0f ? segmentStart : 0f;

                if (overlapStart < segmentEnd &&
                    SegmentIntersectionExtensions.IncludesPoint(segment, point) &&
                    VectorXY.Dot(point - ray.Origin, ray.Direction) >= 0f)
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
