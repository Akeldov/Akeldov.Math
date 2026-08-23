using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides intersection calculations for <see cref="ParameterizedArc"/>.
    /// </summary>
    public static class ParameterizedArcIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="line">The line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, Line line)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, line);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="line">The parameterized line to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedLine line)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, line);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="segment">The segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, Segment segment)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, segment);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a parameterized segment using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="segment">The parameterized segment to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedSegment segment)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a parameterized arc and a parameterized segment chain using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedSegmentChain segmentChain)
        {
            return ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain, segment => GetPointIntersections(source, segment));
        }

        /// <summary>
        /// Returns the isolated point intersections between a parameterized arc and an arc using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="arc">The arc to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the second arc's start angle. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, Arc arc)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, arc);
        }

        /// <summary>
        /// Returns the isolated point intersections between two parameterized arcs using exact comparisons.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="arc">The parameterized arc to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second arc's start point to its end point in its angular direction. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, ParameterizedArc arc)
        {
            List<PointXY> intersections = ArcIntersectionExtensions.GetPointIntersections((Arc)source, (Arc)arc);
            OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a quadratic Bezier curve by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, QuadraticBezier curve)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, curve);
        }

        /// <summary>
        /// Returns isolated point intersections between a parameterized arc and a cubic Bezier curve by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source parameterized arc.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source arc.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the curve's start point to its end point.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedArc source, CubicBezier curve)
        {
            return ArcIntersectionExtensions.GetPointIntersections((Arc)source, curve);
        }

        /// <summary>
        /// Orders distinct known intersections from a parameterized arc's start point in its angular direction.
        /// </summary>
        /// <param name="arc">The target parameterized arc.</param>
        /// <param name="intersections">The caller-owned intersection list to update.</param>
        internal static void OrderPointIntersections(ParameterizedArc arc, List<PointXY> intersections)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (intersections.IndexOf(intersections[i]) != i)
                    intersections.RemoveAt(i);
            }

            intersections.Sort((left, right) =>
                GetCurveCoordinate(arc, left).CompareTo(GetCurveCoordinate(arc, right)));
        }

        /// <summary>
        /// Returns the angular coordinate of a point from a parameterized arc's start angle.
        /// </summary>
        /// <param name="arc">The target parameterized arc.</param>
        /// <param name="point">The point on the arc.</param>
        /// <returns>The angular coordinate in radians.</returns>
        private static float GetCurveCoordinate(ParameterizedArc arc, PointXY point)
        {
            float angle = System.MathF.Atan2(point.Y - arc.Center.Y, point.X - arc.Center.X).NormalizeAngleRad();
            float coordinate = arc.AngularDirection == AngularDirection.Counterclockwise
                ? angle - arc.StartAngle
                : arc.StartAngle - angle;
            return coordinate < 0f ? coordinate + 2f * System.MathF.PI : coordinate;
        }
    }
}
