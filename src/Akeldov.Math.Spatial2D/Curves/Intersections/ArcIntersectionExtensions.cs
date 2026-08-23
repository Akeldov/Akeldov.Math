using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="Arc"/>.
    /// </summary>
    public static class ArcIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between an arc and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="line">The line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, Line line)
        {
            List<PointXY> intersections = new List<PointXY>();

            if (source.Radius == 0f)
            {
                if (GetSignedDistance(line, source.Center) == 0f)
                    intersections.Add(source.Center);

                return intersections;
            }

            PointXY lineOrigin = line.ClosestPointToOrigin;
            VectorXY lineDirection = line.Direction;
            VectorXY originToCenter = lineOrigin - source.Center;

            float a = lineDirection.SquaredLength;
            float b = 2f * VectorXY.Dot(originToCenter, lineDirection);
            float c = originToCenter.SquaredLength - source.Radius * source.Radius;
            float discriminant = b * b - 4f * a * c;

            if (discriminant < 0f)
                return intersections;

            float denominator = 2f * a;
            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            float firstCoordinate = (-b - sqrtDiscriminant) / denominator;
            float secondCoordinate = (-b + sqrtDiscriminant) / denominator;

            AddIfOnArc(source, lineOrigin + firstCoordinate * lineDirection, intersections);

            if (secondCoordinate != firstCoordinate)
                AddIfOnArc(source, lineOrigin + secondCoordinate * lineDirection, intersections);

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between an arc and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="line">The parameterized line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(source, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between an arc and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="segment">The segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, Segment segment)
        {
            VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;
            if (segmentDirection.SquaredLength == 0f)
            {
                if (!(segment.IncludesEndpointA || segment.IncludesEndpointB) || !IncludesPoint(source, segment.EndpointA))
                    return new List<PointXY>();

                return new List<PointXY> { segment.EndpointA };
            }

            var supportingLine = new Line(segment.EndpointA, segment.EndpointB);
            List<PointXY> intersections = GetPointIntersections(source, supportingLine);
            SegmentIntersectionExtensions.RestrictSupportingLineIntersectionsToSegment(intersections, segment);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between an arc and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="segment">The parameterized segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source, (Segment)segment);
        }

        /// <summary>
        /// Adds a circle intersection when it lies within the arc's closed angular region.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="point">The candidate circle intersection.</param>
        /// <param name="intersections">The caller-owned result list.</param>
        private static void AddIfOnArc(Arc source, PointXY point, List<PointXY> intersections)
        {
            if (IsWithinAngularRegion(source, point))
                intersections.Add(point);
        }

        /// <summary>
        /// Determines whether a point lies within the arc's closed angular region using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="point">The point to classify.</param>
        /// <returns><see langword="true"/> when the point lies within the angular region; otherwise, <see langword="false"/>.</returns>
        private static bool IsWithinAngularRegion(Arc source, PointXY point)
        {
            if (source.IsFullCircle)
                return true;

            VectorXY centerToPoint = point - source.Center;
            float angle = MathF.Atan2(centerToPoint.Y, centerToPoint.X).NormalizeAngleRad();

            if (source.StartAngle == source.EndAngle)
                return angle == source.StartAngle;

            if (source.StartAngle < source.EndAngle)
                return angle >= source.StartAngle && angle <= source.EndAngle;

            return angle >= source.StartAngle || angle <= source.EndAngle;
        }

        /// <summary>
        /// Determines whether an arc contains a point using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="point">The point to classify.</param>
        /// <returns><see langword="true"/> when the point belongs to the arc; otherwise, <see langword="false"/>.</returns>
        private static bool IncludesPoint(Arc source, PointXY point)
        {
            VectorXY centerToPoint = point - source.Center;
            if (centerToPoint.SquaredLength != source.Radius * source.Radius)
                return false;

            return source.Radius == 0f || IsWithinAngularRegion(source, point);
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
