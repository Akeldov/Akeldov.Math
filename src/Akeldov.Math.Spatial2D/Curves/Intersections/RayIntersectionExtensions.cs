using System;
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
        /// Returns the isolated point intersection between two rays using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="ray">The ray to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. Continuous overlaps return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Ray ray)
        {
            VectorXY sourceDirection = source.Direction;
            VectorXY rayDirection = ray.Direction;
            VectorXY originDelta = ray.Origin - source.Origin;
            float cross = VectorXY.Cross(sourceDirection, rayDirection);

            if (cross == 0f)
            {
                if (VectorXY.Cross(originDelta, sourceDirection) != 0f)
                    return new List<PointXY>();

                float directionDot = VectorXY.Dot(sourceDirection, rayDirection);
                float rayOriginCoordinate = VectorXY.Dot(originDelta, sourceDirection);
                if (directionDot < 0f && rayOriginCoordinate == 0f)
                    return new List<PointXY> { source.Origin };

                return new List<PointXY>();
            }

            float sourceCoordinate = VectorXY.Cross(originDelta, rayDirection) / cross;
            float rayCoordinate = VectorXY.Cross(originDelta, sourceDirection) / cross;
            if (sourceCoordinate < 0f || rayCoordinate < 0f)
                return new List<PointXY>();

            return new List<PointXY> { source.Origin + sourceCoordinate * sourceDirection };
        }

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
            if (arc.Radius == 0f)
            {
                VectorXY originToCenter = arc.Center - source.Origin;
                if (originToCenter.SquaredLength == 0f)
                    return new List<PointXY> { arc.Center };

                float centerAngle = MathF.Atan2(originToCenter.Y, originToCenter.X).NormalizeAngleRad();
                float rayAngle = source.Angle.NormalizeAngleRad();
                if (centerAngle == rayAngle)
                    return new List<PointXY> { arc.Center };

                return new List<PointXY>();
            }

            VectorXY direction = source.Direction;
            double directionX = direction.X;
            double directionY = direction.Y;
            double originToCenterX = source.Origin.X - arc.Center.X;
            double originToCenterY = source.Origin.Y - arc.Center.Y;
            double radius = arc.Radius;
            double a = directionX * directionX + directionY * directionY;
            double b = 2d * (originToCenterX * directionX + originToCenterY * directionY);
            double c = originToCenterX * originToCenterX + originToCenterY * originToCenterY - radius * radius;
            double discriminant = b * b - 4d * a * c;

            if (discriminant < 0d)
                return new List<PointXY>();

            double denominator = 2d * a;
            double sqrtDiscriminant = System.Math.Sqrt(discriminant);
            double firstCoordinate = (-b - sqrtDiscriminant) / denominator;
            double secondCoordinate = (-b + sqrtDiscriminant) / denominator;
            var intersections = new List<PointXY>();

            AddRayArcIntersection(source, arc, firstCoordinate, intersections);
            if (secondCoordinate != firstCoordinate)
                AddRayArcIntersection(source, arc, secondCoordinate, intersections);

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

        /// <summary>
        /// Returns isolated point intersections between a ray and a cubic Bezier curve by solving the original curve polynomial.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point. Points belonging to continuous overlaps and intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, CubicBezier curve)
        {
            var supportingLine = new Line(source.Origin, source.Origin + source.Direction);
            List<PointXY> intersections = CubicBezierIntersectionExtensions.GetPointIntersections(curve, supportingLine);

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (VectorXY.Dot(intersections[i] - source.Origin, source.Direction) < 0f)
                    intersections.RemoveAt(i);
            }

            CubicBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>Returns isolated point intersections between a ray and a B-spline by solving the original polynomial spline spans.</summary>
        /// <param name="source">The source ray.</param>
        /// <param name="curve">The B-spline to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps and intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, BSpline curve)
        {
            List<PointXY> intersections = BSplineIntersectionExtensions.GetPointIntersections(curve, source);
            BSplineIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>Returns isolated point intersections between a ray and a NURBS curve by solving the original rational spline spans.</summary>
        /// <param name="source">The source ray.</param>
        /// <param name="curve">The NURBS curve to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps and intersections behind the ray are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Nurbs curve)
        {
            List<PointXY> intersections = NurbsIntersectionExtensions.GetPointIntersections(curve, source);
            NurbsIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        internal static List<PointXY> OrderPointIntersections(Ray ray, List<PointXY> intersections)
        {
            intersections.Sort((left, right) =>
                VectorXY.Dot(left - ray.Origin, ray.Direction).CompareTo(
                    VectorXY.Dot(right - ray.Origin, ray.Direction)));
            return intersections;
        }

        private static void AddRayArcIntersection(
            Ray ray,
            Arc arc,
            double rayCoordinate,
            List<PointXY> intersections)
        {
            if (rayCoordinate < 0d)
                return;

            VectorXY direction = ray.Direction;
            PointXY point = new PointXY(
                (float)(ray.Origin.X + rayCoordinate * direction.X),
                (float)(ray.Origin.Y + rayCoordinate * direction.Y));

            if (ArcIntersectionExtensions.IsWithinAngularRegion(arc, point) && !intersections.Contains(point))
                intersections.Add(point);
        }

        internal static bool HasContinuousIntersection(Ray ray, Segment segment)
        {
            VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;
            if (segmentDirection.SquaredLength == 0f ||
                VectorXY.Cross(segment.EndpointA - ray.Origin, ray.Direction) != 0f ||
                VectorXY.Cross(segmentDirection, ray.Direction) != 0f)
            {
                return false;
            }

            float endpointACoordinate = VectorXY.Dot(segment.EndpointA - ray.Origin, ray.Direction);
            float endpointBCoordinate = VectorXY.Dot(segment.EndpointB - ray.Origin, ray.Direction);
            float overlapStart = MathF.Max(0f, MathF.Min(endpointACoordinate, endpointBCoordinate));
            float overlapEnd = MathF.Max(endpointACoordinate, endpointBCoordinate);
            return overlapEnd > overlapStart;
        }
    }
}
