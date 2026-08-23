using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="Segment"/>.
    /// </summary>
    public static class SegmentIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a segment and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment.</param>
        /// <param name="line">The line to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Segment source, Line line)
        {
            List<PointXY> intersections = new List<PointXY>();
            VectorXY segmentDirection = source.EndpointB - source.EndpointA;

            if (segmentDirection.SquaredLength == 0f)
            {
                if ((source.IncludesEndpointA || source.IncludesEndpointB) &&
                    GetSignedDistance(line, source.EndpointA) == 0f)
                {
                    intersections.Add(source.EndpointA);
                }

                return intersections;
            }

            float endpointADistance = GetSignedDistance(line, source.EndpointA);
            float endpointBDistance = GetSignedDistance(line, source.EndpointB);
            float distanceDelta = endpointADistance - endpointBDistance;

            if (distanceDelta == 0f)
                return intersections;

            float segmentCoordinate = endpointADistance / distanceDelta;
            if (segmentCoordinate < 0f || segmentCoordinate > 1f)
                return intersections;

            PointXY intersection = source.EndpointA + segmentCoordinate * segmentDirection;

            if (segmentCoordinate == 0f)
            {
                if (source.IncludesEndpointA)
                    intersections.Add(intersection);
            }
            else if (segmentCoordinate == 1f)
            {
                if (source.IncludesEndpointB)
                    intersections.Add(intersection);
            }
            else
            {
                intersections.Add(intersection);
            }

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a segment and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment.</param>
        /// <param name="line">The parameterized line to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Segment source, ParameterizedLine line)
        {
            return GetPointIntersections(source, line.Line);
        }

        /// <summary>
        /// Returns isolated point intersections between two segments using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment.</param>
        /// <param name="segment">The segment to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second segment's first endpoint to its second endpoint. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Segment source, Segment segment)
        {
            List<PointXY> intersections = new List<PointXY>();
            VectorXY sourceDirection = source.EndpointB - source.EndpointA;
            VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;

            if (sourceDirection.SquaredLength == 0f)
            {
                if ((source.IncludesEndpointA || source.IncludesEndpointB) && IncludesPoint(segment, source.EndpointA))
                    intersections.Add(source.EndpointA);

                return intersections;
            }

            if (segmentDirection.SquaredLength == 0f)
            {
                if ((segment.IncludesEndpointA || segment.IncludesEndpointB) && IncludesPoint(source, segment.EndpointA))
                    intersections.Add(segment.EndpointA);

                return intersections;
            }

            VectorXY originDelta = segment.EndpointA - source.EndpointA;
            float cross = VectorXY.Cross(sourceDirection, segmentDirection);

            if (cross != 0f)
            {
                float sourceCoordinate = VectorXY.Cross(originDelta, segmentDirection) / cross;
                float segmentCoordinate = VectorXY.Cross(originDelta, sourceDirection) / cross;

                if (IncludesCoordinate(source, sourceCoordinate) && IncludesCoordinate(segment, segmentCoordinate))
                    intersections.Add(source.EndpointA + sourceCoordinate * sourceDirection);

                return intersections;
            }

            if (VectorXY.Cross(originDelta, sourceDirection) != 0f || HasContinuousOverlap(source, segment))
                return intersections;

            AddIfIncludedByBoth(source, segment, segment.EndpointA, intersections);
            AddIfIncludedByBoth(source, segment, segment.EndpointB, intersections);
            AddIfIncludedByBoth(source, segment, source.EndpointA, intersections);
            AddIfIncludedByBoth(source, segment, source.EndpointB, intersections);
            RestrictToSegment(intersections, segment);

            return intersections;
        }

        /// <summary>
        /// Removes points outside a segment or at its excluded endpoints and orders the remaining points from its first endpoint to its second endpoint.
        /// </summary>
        /// <param name="intersections">The caller-owned intersection list to update.</param>
        /// <param name="segment">The segment that restricts and orders the points.</param>
        internal static void RestrictToSegment(List<PointXY> intersections, Segment segment)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (!IncludesPoint(segment, intersections[i]))
                    intersections.RemoveAt(i);
            }

            VectorXY direction = segment.EndpointB - segment.EndpointA;
            intersections.Sort((left, right) =>
                VectorXY.Dot(left - segment.EndpointA, direction).CompareTo(
                    VectorXY.Dot(right - segment.EndpointA, direction)));
        }

        /// <summary>
        /// Restricts known supporting-line intersections to a segment using their exact longitudinal coordinates.
        /// </summary>
        /// <param name="intersections">The caller-owned supporting-line intersection list to update.</param>
        /// <param name="segment">The segment that restricts and orders the points.</param>
        internal static void RestrictSupportingLineIntersectionsToSegment(List<PointXY> intersections, Segment segment)
        {
            VectorXY direction = segment.EndpointB - segment.EndpointA;
            float squaredLength = direction.SquaredLength;

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                float coordinate = VectorXY.Dot(intersections[i] - segment.EndpointA, direction);
                if (coordinate < 0f || coordinate > squaredLength ||
                    (coordinate == 0f && !segment.IncludesEndpointA) ||
                    (coordinate == squaredLength && !segment.IncludesEndpointB))
                {
                    intersections.RemoveAt(i);
                }
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - segment.EndpointA, direction).CompareTo(
                    VectorXY.Dot(right - segment.EndpointA, direction)));
        }

        /// <summary>
        /// Determines whether two segments have a nonzero-length collinear overlap.
        /// </summary>
        /// <param name="first">The first segment.</param>
        /// <param name="second">The second segment.</param>
        /// <returns><see langword="true"/> when the overlap contains a continuous set of points; otherwise, <see langword="false"/>.</returns>
        internal static bool HasContinuousOverlap(Segment first, Segment second)
        {
            VectorXY firstDirection = first.EndpointB - first.EndpointA;
            VectorXY secondDirection = second.EndpointB - second.EndpointA;

            if (firstDirection.SquaredLength == 0f || secondDirection.SquaredLength == 0f ||
                VectorXY.Cross(firstDirection, secondDirection) != 0f ||
                VectorXY.Cross(second.EndpointA - first.EndpointA, firstDirection) != 0f)
            {
                return false;
            }

            float secondACoordinate = VectorXY.Dot(second.EndpointA - first.EndpointA, firstDirection) /
                firstDirection.SquaredLength;
            float secondBCoordinate = VectorXY.Dot(second.EndpointB - first.EndpointA, firstDirection) /
                firstDirection.SquaredLength;
            float secondStart = secondACoordinate < secondBCoordinate ? secondACoordinate : secondBCoordinate;
            float secondEnd = secondACoordinate > secondBCoordinate ? secondACoordinate : secondBCoordinate;
            float overlapStart = secondStart > 0f ? secondStart : 0f;
            float overlapEnd = secondEnd < 1f ? secondEnd : 1f;

            return overlapStart < overlapEnd;
        }

        /// <summary>
        /// Determines whether a segment contains a point using exact comparisons and endpoint inclusion.
        /// </summary>
        /// <param name="segment">The segment to inspect.</param>
        /// <param name="point">The point to classify.</param>
        /// <returns><see langword="true"/> when the point belongs to the segment; otherwise, <see langword="false"/>.</returns>
        internal static bool IncludesPoint(Segment segment, PointXY point)
        {
            VectorXY direction = segment.EndpointB - segment.EndpointA;
            if (direction.SquaredLength == 0f)
            {
                return (segment.IncludesEndpointA || segment.IncludesEndpointB) &&
                    segment.EndpointA.Equals(point);
            }

            VectorXY startToPoint = point - segment.EndpointA;
            if (VectorXY.Cross(direction, startToPoint) != 0f)
                return false;

            float coordinate = VectorXY.Dot(startToPoint, direction);
            if (coordinate < 0f || coordinate > direction.SquaredLength)
                return false;

            if (coordinate == 0f && !segment.IncludesEndpointA)
                return false;

            if (coordinate == direction.SquaredLength && !segment.IncludesEndpointB)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether a normalized segment coordinate belongs to a segment.
        /// </summary>
        /// <param name="segment">The segment to inspect.</param>
        /// <param name="coordinate">The normalized coordinate from zero to one.</param>
        /// <returns><see langword="true"/> when the coordinate belongs to the segment; otherwise, <see langword="false"/>.</returns>
        private static bool IncludesCoordinate(Segment segment, float coordinate)
        {
            if (coordinate < 0f || coordinate > 1f)
                return false;

            if (coordinate == 0f && !segment.IncludesEndpointA)
                return false;

            if (coordinate == 1f && !segment.IncludesEndpointB)
                return false;

            return true;
        }

        /// <summary>
        /// Adds a point when both segments include it.
        /// </summary>
        /// <param name="first">The first segment.</param>
        /// <param name="second">The second segment.</param>
        /// <param name="point">The candidate point.</param>
        /// <param name="intersections">The caller-owned result list.</param>
        private static void AddIfIncludedByBoth(Segment first, Segment second, PointXY point, List<PointXY> intersections)
        {
            if (IncludesPoint(first, point) && IncludesPoint(second, point) && !intersections.Contains(point))
                intersections.Add(point);
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
