using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact-curve intersection calculations for <see cref="Nurbs"/>.
    /// </summary>
    public static class NurbsIntersectionExtensions
    {
        /// <summary>Returns isolated point intersections between a NURBS curve and a ray by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="ray">The ray to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, Ray ray) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), ray);

        /// <summary>Returns isolated point intersections between a NURBS curve and a line by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="line">The line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, Line line) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), line);

        /// <summary>Returns isolated point intersections between a NURBS curve and a parameterized line by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="line">The parameterized line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, ParameterizedLine line) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), line);

        /// <summary>Returns isolated point intersections between a NURBS curve and a segment by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="segment">The segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, Segment segment) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), segment);

        /// <summary>Returns isolated point intersections between a NURBS curve and a parameterized segment by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="segment">The parameterized segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, ParameterizedSegment segment) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), segment);

        /// <summary>Returns distinct isolated point intersections between a NURBS curve and a parameterized segment chain by solving the original rational spline spans.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, ParameterizedSegmentChain segmentChain) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), segmentChain);

        /// <summary>Returns isolated point intersections between a NURBS curve and an arc by solving the original rational spline-circle equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="arc">The arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, Arc arc) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), arc);

        /// <summary>Returns isolated point intersections between a NURBS curve and a parameterized arc by solving the original rational spline-circle equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="arc">The parameterized arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, ParameterizedArc arc) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), arc);

        /// <summary>Returns isolated point intersections between a NURBS curve and a quadratic Bezier curve by solving the original rational-polynomial equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the Bezier curve's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, QuadraticBezier curve) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), curve);

        /// <summary>Returns isolated point intersections between a NURBS curve and a cubic Bezier curve by solving the original rational-polynomial equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the Bezier curve's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, CubicBezier curve) =>
            SplineIntersectionOperations.GetPointIntersections(SplineIntersectionOperations.CreateSpans(source), curve);

        /// <summary>Returns isolated point intersections between a NURBS curve and a B-spline by solving their original piecewise rational-polynomial equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="curve">The B-spline to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the B-spline's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, BSpline curve) =>
            SplineIntersectionOperations.GetPointIntersections(
                SplineIntersectionOperations.CreateSpans(source),
                SplineIntersectionOperations.CreateSpans(curve));

        /// <summary>Returns isolated point intersections between two NURBS curves by solving their original piecewise rational-polynomial equations.</summary>
        /// <param name="source">The source NURBS curve.</param>
        /// <param name="curve">The NURBS curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second curve's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this Nurbs source, Nurbs curve) =>
            SplineIntersectionOperations.GetPointIntersections(
                SplineIntersectionOperations.CreateSpans(source),
                SplineIntersectionOperations.CreateSpans(curve));

        internal static void OrderPointIntersections(Nurbs curve, List<PointXY> intersections) =>
            SplineIntersectionOperations.OrderPointIntersections(
                SplineIntersectionOperations.CreateSpans(curve),
                intersections);
    }
}
