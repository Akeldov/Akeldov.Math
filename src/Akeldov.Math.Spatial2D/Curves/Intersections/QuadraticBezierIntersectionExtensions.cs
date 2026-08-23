using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="QuadraticBezier"/>.
    /// </summary>
    public static class QuadraticBezierIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and a line by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="line">The line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, Line line)
        {
            double startValue = GetSignedDistance(line, source.StartPoint);
            double controlValue = GetSignedDistance(line, source.ControlPoint);
            double endValue = GetSignedDistance(line, source.EndPoint);
            double quadratic = startValue - 2.0 * controlValue + endValue;
            double linear = 2.0 * (controlValue - startValue);

            if (quadratic == 0.0 && linear == 0.0 && startValue == 0.0)
            {
                return source.StartPoint.Equals(source.ControlPoint) && source.StartPoint.Equals(source.EndPoint)
                    ? new List<PointXY> { source.StartPoint }
                    : new List<PointXY>();
            }

            List<double> parameters = new List<double>();
            AddEndpointParameters(startValue, endValue, parameters);
            AddQuadraticRoots(quadratic, linear, startValue, parameters);

            List<PointXY> intersections = new List<PointXY>();
            for (int i = 0; i < parameters.Count; i++)
            {
                PointXY point = source.GetPointAt((float)parameters[i]);
                if (!intersections.Contains(point))
                    intersections.Add(point);
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - line.ClosestPointToOrigin, line.Direction).CompareTo(
                    VectorXY.Dot(right - line.ClosestPointToOrigin, line.Direction)));

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and a parameterized line by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="line">The parameterized line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(source, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and a segment by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="segment">The segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, Segment segment)
        {
            VectorXY segmentDirection = segment.EndpointB - segment.EndpointA;
            if (segmentDirection.SquaredLength == 0f)
                return GetPointIntersection(source, segment);

            var supportingLine = new Line(segment.EndpointA, segment.EndpointB);
            List<PointXY> intersections = GetPointIntersections(source, supportingLine);
            SegmentIntersectionExtensions.RestrictSupportingLineIntersectionsToSegment(intersections, segment);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and a parameterized segment by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="segment">The parameterized segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source, (Segment)segment);
        }

        /// <summary>
        /// Returns a degenerate segment point when it belongs to the curve and is included by the segment.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="segment">The degenerate segment to intersect.</param>
        /// <returns>A new mutable list containing the isolated point intersection, or an empty list.</returns>
        private static List<PointXY> GetPointIntersection(QuadraticBezier source, Segment segment)
        {
            if (!(segment.IncludesEndpointA || segment.IncludesEndpointB))
                return new List<PointXY>();

            PointXY point = segment.EndpointA;
            var horizontalProbe = new Line(point, point + VectorXY.BasisX);
            List<PointXY> intersections = GetPointIntersections(source, horizontalProbe);

            if (!intersections.Contains(point))
            {
                var verticalProbe = new Line(point, point + VectorXY.BasisY);
                intersections = GetPointIntersections(source, verticalProbe);
            }

            return intersections.Contains(point)
                ? new List<PointXY> { point }
                : new List<PointXY>();
        }

        /// <summary>
        /// Adds exact endpoint roots to the parameter list.
        /// </summary>
        /// <param name="startValue">The polynomial value at parameter zero.</param>
        /// <param name="endValue">The polynomial value at parameter one.</param>
        /// <param name="parameters">The parameter list to update.</param>
        private static void AddEndpointParameters(double startValue, double endValue, List<double> parameters)
        {
            if (startValue == 0.0)
                parameters.Add(0.0);

            if (endValue == 0.0)
                parameters.Add(1.0);
        }

        /// <summary>
        /// Adds roots strictly inside the unit parameter interval for a quadratic polynomial.
        /// </summary>
        /// <param name="quadratic">The quadratic coefficient.</param>
        /// <param name="linear">The linear coefficient.</param>
        /// <param name="constant">The constant coefficient.</param>
        /// <param name="parameters">The parameter list to update.</param>
        private static void AddQuadraticRoots(double quadratic, double linear, double constant, List<double> parameters)
        {
            if (quadratic == 0.0)
            {
                if (linear != 0.0)
                    AddInteriorParameter(-constant / linear, parameters);

                return;
            }

            double discriminant = linear * linear - 4.0 * quadratic * constant;
            if (discriminant < 0.0)
                return;

            if (discriminant == 0.0)
            {
                AddInteriorParameter(-linear / (2.0 * quadratic), parameters);
                return;
            }

            double sqrtDiscriminant = System.Math.Sqrt(discriminant);
            double denominator = 2.0 * quadratic;
            AddInteriorParameter((-linear - sqrtDiscriminant) / denominator, parameters);
            AddInteriorParameter((-linear + sqrtDiscriminant) / denominator, parameters);
        }

        /// <summary>
        /// Adds a distinct parameter when it lies strictly inside the unit interval.
        /// </summary>
        /// <param name="parameter">The candidate curve parameter.</param>
        /// <param name="parameters">The parameter list to update.</param>
        private static void AddInteriorParameter(double parameter, List<double> parameters)
        {
            if (parameter > 0.0 && parameter < 1.0 && !parameters.Contains(parameter))
                parameters.Add(parameter);
        }

        /// <summary>
        /// Returns the signed distance from a point to a normalized line equation.
        /// </summary>
        /// <param name="line">The line that defines the signed-distance function.</param>
        /// <param name="point">The point to evaluate.</param>
        /// <returns>The signed distance in world coordinate units.</returns>
        private static double GetSignedDistance(Line line, PointXY point) =>
            (double)line.EquationA * point.X + (double)line.EquationB * point.Y + line.EquationC;
    }
}
