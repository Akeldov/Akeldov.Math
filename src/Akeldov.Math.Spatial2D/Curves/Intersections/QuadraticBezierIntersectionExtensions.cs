using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides intersection calculations for <see cref="QuadraticBezier"/>.
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
        /// Returns the distinct isolated point intersections between a quadratic Bezier curve and a parameterized segment chain by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, ParameterizedSegmentChain segmentChain)
        {
            return ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain, segment => GetPointIntersections(source, segment));
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and an arc by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="arc">The arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, Arc arc)
        {
            double[] polynomial = CreateCirclePolynomial(source, arc);
            List<PointXY> intersections = new List<PointXY>();

            if (PolynomialRootIsolation.IsZero(polynomial))
            {
                if (source.StartPoint.Equals(source.ControlPoint) &&
                    source.StartPoint.Equals(source.EndPoint) &&
                    (arc.Radius == 0f || ArcIntersectionExtensions.IsWithinAngularRegion(arc, source.StartPoint)))
                {
                    intersections.Add(source.StartPoint);
                }

                return intersections;
            }

            List<double> stationaryCoordinates = PolynomialRootIsolation.FindStationaryCoordinatesInUnitInterval(polynomial);
            List<float> tangentCoordinates = new List<float>();
            for (int i = 0; i < stationaryCoordinates.Count; i++)
            {
                PointXY point = Evaluate(source, stationaryCoordinates[i]);
                if (IsOnCircleAfterFloatRounding(point, arc))
                {
                    tangentCoordinates.Add((float)stationaryCoordinates[i]);
                    AddIfOnArc(point, arc, intersections);
                }
            }

            List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(polynomial);
            for (int i = 0; i < roots.Count; i++)
            {
                if (!tangentCoordinates.Contains((float)roots[i]))
                    AddIfOnArc(Evaluate(source, roots[i]), arc, intersections);
            }

            ArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic Bezier curve and a parameterized arc by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="arc">The parameterized arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, ParameterizedArc arc)
        {
            List<PointXY> intersections = GetPointIntersections(source, (Arc)arc);
            ParameterizedArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between two quadratic Bezier curves by numerically isolating the roots of the original quartic resultant above float precision.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, QuadraticBezier curve)
        {
            double[] x =
            {
                source.StartPoint.X,
                2d * ((double)source.ControlPoint.X - source.StartPoint.X),
                (double)source.StartPoint.X - 2d * source.ControlPoint.X + source.EndPoint.X
            };
            double[] y =
            {
                source.StartPoint.Y,
                2d * ((double)source.ControlPoint.Y - source.StartPoint.Y),
                (double)source.StartPoint.Y - 2d * source.ControlPoint.Y + source.EndPoint.Y
            };
            double[] polynomial = CreateImplicitPolynomial(curve, x, y);
            bool sourceIsPoint = source.StartPoint.Equals(source.ControlPoint) && source.StartPoint.Equals(source.EndPoint);
            return GetPointIntersections(curve, polynomial, parameter => Evaluate(source, parameter), sourceIsPoint);
        }

        /// <summary>
        /// Returns isolated point intersections between a quadratic and a cubic Bezier curve by numerically isolating the roots of the original sextic resultant above float precision.
        /// </summary>
        /// <param name="source">The source quadratic Bezier curve.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the cubic curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this QuadraticBezier source, CubicBezier curve)
        {
            List<PointXY> intersections = CubicBezierIntersectionExtensions.GetPointIntersections(curve, source);
            CubicBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>
        /// Creates the polynomial obtained by substituting a parameterized source curve into a quadratic Bezier curve's implicit equation.
        /// </summary>
        /// <param name="curve">The quadratic Bezier curve that defines the implicit equation.</param>
        /// <param name="x">The source curve's X-coordinate polynomial in ascending power order.</param>
        /// <param name="y">The source curve's Y-coordinate polynomial in ascending power order.</param>
        /// <returns>The implicit intersection polynomial in ascending power order.</returns>
        internal static double[] CreateImplicitPolynomial(QuadraticBezier curve, double[] x, double[] y)
        {
            double quadraticX = (double)curve.StartPoint.X - 2d * curve.ControlPoint.X + curve.EndPoint.X;
            double linearX = 2d * ((double)curve.ControlPoint.X - curve.StartPoint.X);
            double constantX = curve.StartPoint.X;
            double quadraticY = (double)curve.StartPoint.Y - 2d * curve.ControlPoint.Y + curve.EndPoint.Y;
            double linearY = 2d * ((double)curve.ControlPoint.Y - curve.StartPoint.Y);
            double constantY = curve.StartPoint.Y;
            double determinant = quadraticX * linearY - linearX * quadraticY;
            double[] quadraticElimination = new double[x.Length];
            double[] linearElimination = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                quadraticElimination[i] = quadraticY * x[i] - quadraticX * y[i];
                linearElimination[i] = linearY * x[i] - linearX * y[i];
            }

            quadraticElimination[0] += quadraticX * constantY - quadraticY * constantX;
            linearElimination[0] += linearX * constantY - linearY * constantX;

            double[] polynomial = new double[2 * x.Length - 1];
            bool isLinearTarget = quadraticX == 0d && quadraticY == 0d;
            double[] squaredElimination = isLinearTarget
                ? linearElimination
                : quadraticElimination;
            for (int i = 0; i < quadraticElimination.Length; i++)
            {
                for (int j = 0; j < squaredElimination.Length; j++)
                    polynomial[i + j] += squaredElimination[i] * squaredElimination[j];

                if (!isLinearTarget)
                    polynomial[i] -= determinant * linearElimination[i];
            }

            return polynomial;
        }

        /// <summary>
        /// Extracts, validates, and orders intersections represented by an implicit intersection polynomial.
        /// </summary>
        /// <param name="curve">The target quadratic Bezier curve.</param>
        /// <param name="polynomial">The implicit intersection polynomial.</param>
        /// <param name="evaluateSource">The source curve evaluator.</param>
        /// <param name="sourceIsPoint">Whether the source curve is a single point.</param>
        /// <returns>A new mutable list owned by the caller and ordered along the target curve.</returns>
        internal static List<PointXY> GetPointIntersections(QuadraticBezier curve, double[] polynomial, System.Func<double, PointXY> evaluateSource, bool sourceIsPoint)
        {
            List<PointXY> intersections = new List<PointXY>();

            if (PolynomialRootIsolation.IsZero(polynomial))
            {
                PointXY point = evaluateSource(0d);
                if (sourceIsPoint &&
                    TryGetCurveCoordinate(curve, point, out double coordinate) &&
                    Evaluate(curve, coordinate).Equals(point))
                {
                    intersections.Add(point);
                }

                return intersections;
            }

            List<double> stationaryCoordinates = PolynomialRootIsolation.FindStationaryCoordinatesInUnitInterval(polynomial);
            List<float> tangentCoordinates = new List<float>();
            for (int i = 0; i < stationaryCoordinates.Count; i++)
            {
                PointXY point = evaluateSource(stationaryCoordinates[i]);
                if (TryGetCurveCoordinate(curve, point, out double coordinate) &&
                    Evaluate(curve, coordinate).Equals(point))
                {
                    tangentCoordinates.Add((float)stationaryCoordinates[i]);
                    AddDistinct(intersections, point);
                }
            }

            List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(polynomial);
            for (int i = 0; i < roots.Count; i++)
            {
                if (tangentCoordinates.Contains((float)roots[i]))
                    continue;

                PointXY point = evaluateSource(roots[i]);
                if (TryGetCurveCoordinate(curve, point, out _))
                    AddDistinct(intersections, point);
            }

            OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>
        /// Orders distinct known intersections from a quadratic Bezier curve's start point to its end point.
        /// </summary>
        /// <param name="curve">The target quadratic Bezier curve.</param>
        /// <param name="intersections">The caller-owned intersection list to update.</param>
        internal static void OrderPointIntersections(QuadraticBezier curve, List<PointXY> intersections)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (intersections.IndexOf(intersections[i]) != i)
                    intersections.RemoveAt(i);
            }

            intersections.Sort((left, right) =>
            {
                bool hasLeftCoordinate = TryGetCurveCoordinate(curve, left, out double leftCoordinate);
                bool hasRightCoordinate = TryGetCurveCoordinate(curve, right, out double rightCoordinate);

                if (!hasLeftCoordinate)
                    leftCoordinate = double.PositiveInfinity;
                if (!hasRightCoordinate)
                    rightCoordinate = double.PositiveInfinity;

                return leftCoordinate.CompareTo(rightCoordinate);
            });
        }

        /// <summary>
        /// Finds the first normalized parameter at which a quadratic Bezier curve produces a point in public float precision.
        /// </summary>
        private static bool TryGetCurveCoordinate(QuadraticBezier curve, PointXY point, out double coordinate)
        {
            double[] x =
            {
                (double)curve.StartPoint.X - point.X,
                2d * ((double)curve.ControlPoint.X - curve.StartPoint.X),
                (double)curve.StartPoint.X - 2d * curve.ControlPoint.X + curve.EndPoint.X
            };
            double[] y =
            {
                (double)curve.StartPoint.Y - point.Y,
                2d * ((double)curve.ControlPoint.Y - curve.StartPoint.Y),
                (double)curve.StartPoint.Y - 2d * curve.ControlPoint.Y + curve.EndPoint.Y
            };

            if (PolynomialRootIsolation.IsZero(x) && PolynomialRootIsolation.IsZero(y))
            {
                coordinate = 0d;
                return true;
            }

            List<double> candidates = PolynomialRootIsolation.IsZero(x)
                ? PolynomialRootIsolation.FindRootsInUnitInterval(y)
                : PolynomialRootIsolation.FindRootsInUnitInterval(x);

            coordinate = double.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                double candidate = candidates[i];
                if (candidate < coordinate)
                    coordinate = candidate;
            }

            return !double.IsPositiveInfinity(coordinate);
        }

        /// <summary>
        /// Adds a point when it is not already present in the caller-owned list.
        /// </summary>
        private static void AddDistinct(List<PointXY> intersections, PointXY point)
        {
            if (!intersections.Contains(point))
                intersections.Add(point);
        }

        /// <summary>
        /// Adds a distinct candidate when it belongs to the arc's angular region.
        /// </summary>
        private static void AddIfOnArc(PointXY point, Arc arc, List<PointXY> intersections)
        {
            if ((arc.Radius == 0f || ArcIntersectionExtensions.IsWithinAngularRegion(arc, point)) &&
                !intersections.Contains(point))
            {
                intersections.Add(point);
            }
        }

        /// <summary>
        /// Determines whether a stationary candidate rounds exactly onto the source circle in the public point precision.
        /// </summary>
        private static bool IsOnCircleAfterFloatRounding(PointXY point, Arc arc)
        {
            double deltaX = (double)point.X - arc.Center.X;
            double deltaY = (double)point.Y - arc.Center.Y;
            return deltaX * deltaX + deltaY * deltaY == (double)arc.Radius * arc.Radius;
        }

        /// <summary>
        /// Creates the quartic squared-distance polynomial between a quadratic Bezier curve and a circle.
        /// </summary>
        private static double[] CreateCirclePolynomial(QuadraticBezier source, Arc arc)
        {
            double[] x =
            {
                (double)source.StartPoint.X - arc.Center.X,
                2d * ((double)source.ControlPoint.X - source.StartPoint.X),
                (double)source.StartPoint.X - 2d * source.ControlPoint.X + source.EndPoint.X
            };
            double[] y =
            {
                (double)source.StartPoint.Y - arc.Center.Y,
                2d * ((double)source.ControlPoint.Y - source.StartPoint.Y),
                (double)source.StartPoint.Y - 2d * source.ControlPoint.Y + source.EndPoint.Y
            };

            double[] polynomial = new double[5];
            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                    polynomial[i + j] += x[i] * x[j] + y[i] * y[j];
            }

            polynomial[0] -= (double)arc.Radius * arc.Radius;
            return polynomial;
        }

        /// <summary>
        /// Evaluates a quadratic Bezier curve in double precision and rounds the result to the public point type.
        /// </summary>
        private static PointXY Evaluate(QuadraticBezier source, double parameter)
        {
            double inverse = 1d - parameter;
            double startWeight = inverse * inverse;
            double controlWeight = 2d * inverse * parameter;
            double endWeight = parameter * parameter;
            return new PointXY(
                (float)(startWeight * source.StartPoint.X + controlWeight * source.ControlPoint.X + endWeight * source.EndPoint.X),
                (float)(startWeight * source.StartPoint.Y + controlWeight * source.ControlPoint.Y + endWeight * source.EndPoint.Y));
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
