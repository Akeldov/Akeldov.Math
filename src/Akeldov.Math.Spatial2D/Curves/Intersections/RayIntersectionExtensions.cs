using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="Ray"/>.
    /// </summary>
    public static class RayIntersectionExtensions
    {
        /// <summary>
        /// Returns the isolated point intersection between a ray and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="line">The line to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. Parallel lines, continuous overlaps, and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Line line)
        {
            VectorXY rayDirection = source.Direction;
            VectorXY lineDirection = line.Direction;
            float cross = VectorXY.Cross(rayDirection, lineDirection);

            if (cross == 0f)
                return new List<PointXY>();

            VectorXY originDelta = line.ClosestPointToOrigin - source.Origin;
            float rayCoordinate = VectorXY.Cross(originDelta, lineDirection) / cross;

            if (rayCoordinate < 0f)
                return new List<PointXY>();

            PointXY intersection = source.Origin + rayCoordinate * rayDirection;
            return new List<PointXY> { intersection };
        }

        /// <summary>
        /// Returns the isolated point intersection between a ray and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="line">The parameterized line to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. Parallel lines, continuous overlaps, and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, ParameterizedLine line)
        {
            return GetPointIntersections(source, line.Line);
        }

        /// <summary>
        /// Returns isolated point intersections between a ray and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="segment">The segment to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Segment segment)
        {
            List<PointXY> intersections = new List<PointXY>();
            VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;

            if (segmentDirection.SquaredLength == 0f)
            {
                VectorXY originToPoint = segment.EndpointA - source.Origin;
                if ((segment.IncludesEndpointA || segment.IncludesEndpointB) &&
                    VectorXY.Cross(originToPoint, source.Direction) == 0f &&
                    VectorXY.Dot(originToPoint, source.Direction) >= 0f)
                {
                    intersections.Add(segment.EndpointA);
                }

                return intersections;
            }

            VectorXY originDelta = segment.EndpointA - source.Origin;
            float cross = VectorXY.Cross(source.Direction, segmentDirection);

            if (cross != 0f)
            {
                float rayCoordinate = VectorXY.Cross(originDelta, segmentDirection) / cross;
                float segmentCoordinate = VectorXY.Cross(originDelta, source.Direction) / cross;

                if (rayCoordinate < 0f || segmentCoordinate < 0f || segmentCoordinate > 1f ||
                    (segmentCoordinate == 0f && !segment.IncludesEndpointA) ||
                    (segmentCoordinate == 1f && !segment.IncludesEndpointB))
                {
                    return intersections;
                }

                intersections.Add(source.Origin + rayCoordinate * source.Direction);
                return intersections;
            }

            if (VectorXY.Cross(originDelta, source.Direction) != 0f)
                return intersections;

            float endpointACoordinate = VectorXY.Dot(segment.EndpointA - source.Origin, source.Direction);
            float endpointBCoordinate = VectorXY.Dot(segment.EndpointB - source.Origin, source.Direction);
            float overlapEnd = endpointACoordinate > endpointBCoordinate ? endpointACoordinate : endpointBCoordinate;

            if (overlapEnd < 0f)
                return intersections;

            if (overlapEnd == 0f && SegmentIntersectionExtensions.IncludesPoint(segment, source.Origin))
                intersections.Add(source.Origin);

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a ray and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="segment">The parameterized segment to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source, (Segment)segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a ray and a parameterized segment chain using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Points belonging to continuous overlaps and intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, ParameterizedSegmentChain segmentChain)
        {
            List<PointXY> intersections = ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain.Segments, source);
            return ParameterizedSegmentChainIntersectionExtensions.OrderPointIntersections(segmentChain, intersections);
        }

        /// <summary>
        /// Returns the isolated point intersections between a ray and an arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="arc">The arc to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle. Intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Arc arc)
        {
            var supportingLine = new Line(source.Origin, source.Origin + source.Direction);
            List<PointXY> intersections = ArcIntersectionExtensions.GetPointIntersections(arc, supportingLine);

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (VectorXY.Dot(intersections[i] - source.Origin, source.Direction) < 0f)
                    intersections.RemoveAt(i);
            }

            ArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns the isolated point intersections between a ray and a parameterized arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="arc">The parameterized arc to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction. Intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, ParameterizedArc arc)
        {
            List<PointXY> intersections = GetPointIntersections(source, (Arc)arc);
            ParameterizedArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a ray and a quadratic Bezier curve by solving the original curve polynomial.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point. Points belonging to continuous overlaps and intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, QuadraticBezier curve)
        {
            var supportingLine = new Line(source.Origin, source.Origin + source.Direction);
            List<PointXY> intersections = QuadraticBezierIntersectionExtensions.GetPointIntersections(curve, supportingLine);

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (VectorXY.Dot(intersections[i] - source.Origin, source.Direction) < 0f)
                    intersections.RemoveAt(i);
            }

            QuadraticBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }
    }
}
