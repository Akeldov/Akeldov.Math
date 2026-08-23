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
        /// Returns the distinct isolated point intersections between an arc and a parameterized segment chain using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, ParameterizedSegmentChain segmentChain)
        {
            return ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain, segment => GetPointIntersections(source, segment));
        }

        /// <summary>
        /// Returns the isolated point intersections between two arcs using exact comparisons.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="arc">The arc to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the second arc's start angle. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Arc source, Arc arc)
        {
            if (source.Center.Equals(arc.Center))
            {
                if (source.Radius != arc.Radius)
                    return new List<PointXY>();

                return GetConcentricPointIntersections(source, arc);
            }

            if (source.Radius == 0f)
            {
                return IncludesPoint(arc, source.Center)
                    ? new List<PointXY> { source.Center }
                    : new List<PointXY>();
            }

            if (arc.Radius == 0f)
            {
                return IncludesPoint(source, arc.Center)
                    ? new List<PointXY> { arc.Center }
                    : new List<PointXY>();
            }

            VectorXY centerDelta = arc.Center - source.Center;
            float squaredCenterDistance = centerDelta.SquaredLength;
            float radiusSum = source.Radius + arc.Radius;
            float radiusDifference = source.Radius - arc.Radius;

            if (squaredCenterDistance > radiusSum * radiusSum ||
                squaredCenterDistance < radiusDifference * radiusDifference)
            {
                return new List<PointXY>();
            }

            float centerDistance = MathF.Sqrt(squaredCenterDistance);
            float sourceCoordinate =
                (source.Radius * source.Radius - arc.Radius * arc.Radius + squaredCenterDistance) /
                (2f * centerDistance);
            float perpendicularSquared = source.Radius * source.Radius - sourceCoordinate * sourceCoordinate;

            if (perpendicularSquared < 0f)
                return new List<PointXY>();

            PointXY midpoint = source.Center + centerDelta * (sourceCoordinate / centerDistance);
            VectorXY perpendicular = new VectorXY(-centerDelta.Y, centerDelta.X) / centerDistance;
            float perpendicularDistance = MathF.Sqrt(perpendicularSquared);
            List<PointXY> intersections = new List<PointXY>();

            AddIfOnBothArcs(source, arc, midpoint + perpendicular * perpendicularDistance, intersections);
            if (perpendicularDistance != 0f)
                AddIfOnBothArcs(source, arc, midpoint - perpendicular * perpendicularDistance, intersections);

            OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Orders distinct known intersections counterclockwise along an arc from its start angle.
        /// </summary>
        /// <param name="arc">The target arc.</param>
        /// <param name="intersections">The caller-owned intersection list to update.</param>
        internal static void OrderPointIntersections(Arc arc, List<PointXY> intersections)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (intersections.IndexOf(intersections[i]) != i)
                    intersections.RemoveAt(i);
            }

            intersections.Sort((left, right) =>
                GetCounterclockwiseCoordinate(arc, left).CompareTo(
                    GetCounterclockwiseCoordinate(arc, right)));
        }

        /// <summary>
        /// Returns isolated intersections between concentric arcs on the same circle.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="arc">The target arc.</param>
        /// <returns>A new mutable list ordered counterclockwise from the target arc's start angle.</returns>
        private static List<PointXY> GetConcentricPointIntersections(Arc source, Arc arc)
        {
            if (source.Radius == 0f)
                return new List<PointXY> { source.Center };

            List<float> candidateAngles = new List<float>
            {
                source.StartAngle,
                source.EndAngle,
                arc.StartAngle,
                arc.EndAngle
            };
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < candidateAngles.Count; i++)
            {
                float angle = candidateAngles[i];
                if (!ContainsAngle(source, angle) ||
                    !ContainsAngle(arc, angle) ||
                    BelongsToContinuousAngularOverlap(source, arc, angle))
                {
                    continue;
                }

                PointXY point = new PointXY(
                    source.Center.X + source.Radius * MathF.Cos(angle),
                    source.Center.Y + source.Radius * MathF.Sin(angle));

                if (!intersections.Contains(point))
                    intersections.Add(point);
            }

            OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Adds a circle intersection when it belongs to both angular regions.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="arc">The target arc.</param>
        /// <param name="point">The candidate circle intersection.</param>
        /// <param name="intersections">The caller-owned result list.</param>
        private static void AddIfOnBothArcs(Arc source, Arc arc, PointXY point, List<PointXY> intersections)
        {
            if (IsWithinAngularRegion(source, point) &&
                IsWithinAngularRegion(arc, point) &&
                !intersections.Contains(point))
            {
                intersections.Add(point);
            }
        }

        /// <summary>
        /// Determines whether an angle belongs to a continuous common portion of two concentric arcs.
        /// </summary>
        /// <param name="source">The source arc.</param>
        /// <param name="arc">The target arc.</param>
        /// <param name="angle">The normalized angle in radians.</param>
        /// <returns><see langword="true"/> when a common angular interval continues from the angle; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousAngularOverlap(Arc source, Arc arc, float angle)
        {
            bool continuesCounterclockwise =
                ContinuesCounterclockwise(source, angle) && ContinuesCounterclockwise(arc, angle);
            bool continuesClockwise = ContinuesClockwise(source, angle) && ContinuesClockwise(arc, angle);
            return continuesCounterclockwise || continuesClockwise;
        }

        /// <summary>
        /// Determines whether an arc continues immediately counterclockwise from an included angle.
        /// </summary>
        /// <param name="arc">The arc to inspect.</param>
        /// <param name="angle">The normalized angle in radians.</param>
        /// <returns><see langword="true"/> when the arc continues counterclockwise; otherwise, <see langword="false"/>.</returns>
        private static bool ContinuesCounterclockwise(Arc arc, float angle) =>
            arc.IsFullCircle || (arc.StartAngle != arc.EndAngle && ContainsAngle(arc, angle) && angle != arc.EndAngle);

        /// <summary>
        /// Determines whether an arc continues immediately clockwise from an included angle.
        /// </summary>
        /// <param name="arc">The arc to inspect.</param>
        /// <param name="angle">The normalized angle in radians.</param>
        /// <returns><see langword="true"/> when the arc continues clockwise; otherwise, <see langword="false"/>.</returns>
        private static bool ContinuesClockwise(Arc arc, float angle) =>
            arc.IsFullCircle || (arc.StartAngle != arc.EndAngle && ContainsAngle(arc, angle) && angle != arc.StartAngle);

        /// <summary>
        /// Determines whether a normalized angle belongs to an arc using exact comparisons.
        /// </summary>
        /// <param name="arc">The arc to inspect.</param>
        /// <param name="angle">The normalized angle in radians.</param>
        /// <returns><see langword="true"/> when the angle belongs to the arc; otherwise, <see langword="false"/>.</returns>
        private static bool ContainsAngle(Arc arc, float angle)
        {
            if (arc.IsFullCircle)
                return true;

            if (arc.StartAngle == arc.EndAngle)
                return angle == arc.StartAngle;

            if (arc.StartAngle < arc.EndAngle)
                return angle >= arc.StartAngle && angle <= arc.EndAngle;

            return angle >= arc.StartAngle || angle <= arc.EndAngle;
        }

        /// <summary>
        /// Returns the counterclockwise angular coordinate of a point from an arc's start angle.
        /// </summary>
        /// <param name="arc">The target arc.</param>
        /// <param name="point">The point on the arc.</param>
        /// <returns>The angular coordinate in radians.</returns>
        private static float GetCounterclockwiseCoordinate(Arc arc, PointXY point)
        {
            float angle = MathF.Atan2(point.Y - arc.Center.Y, point.X - arc.Center.X).NormalizeAngleRad();
            float coordinate = angle - arc.StartAngle;
            return coordinate < 0f ? coordinate + 2f * MathF.PI : coordinate;
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
