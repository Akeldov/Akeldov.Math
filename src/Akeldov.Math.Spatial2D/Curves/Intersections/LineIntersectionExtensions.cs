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
        /// Returns the isolated point intersection between a line and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="ray">The ray to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Ray ray) =>
            RayIntersectionExtensions.GetPointIntersections(ray, source);

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

        /// <summary>
        /// Returns the isolated point intersections between a line and an arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="arc">The arc to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Arc arc)
        {
            List<PointXY> intersections = ArcIntersectionExtensions.GetPointIntersections(arc, source);
            ArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns the isolated point intersections between a line and a parameterized arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="arc">The parameterized arc to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, ParameterizedArc arc)
        {
            List<PointXY> intersections = GetPointIntersections(source, (Arc)arc);
            ParameterizedArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a line and a quadratic Bezier curve by solving the original curve polynomial.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, QuadraticBezier curve)
        {
            List<PointXY> intersections = QuadraticBezierIntersectionExtensions.GetPointIntersections(curve, source);
            QuadraticBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a line and a cubic Bezier curve by solving the original curve polynomial.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, CubicBezier curve)
        {
            List<PointXY> intersections = CubicBezierIntersectionExtensions.GetPointIntersections(curve, source);
            CubicBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>Returns isolated point intersections between a line and a B-spline by solving the original polynomial spline spans.</summary>
        /// <param name="source">The source line.</param>
        /// <param name="curve">The B-spline to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, BSpline curve)
        {
            List<PointXY> intersections = BSplineIntersectionExtensions.GetPointIntersections(curve, source);
            BSplineIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>Returns isolated point intersections between a line and a NURBS curve by solving the original rational spline spans.</summary>
        /// <param name="source">The source line.</param>
        /// <param name="curve">The NURBS curve to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Nurbs curve)
        {
            List<PointXY> intersections = NurbsIntersectionExtensions.GetPointIntersections(curve, source);
            NurbsIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }
    }
}
