using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides intersection calculations for <see cref="CubicBezier"/>.
    /// </summary>
    public static class CubicBezierIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a cubic Bezier curve and a ray by solving the original curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="ray">The ray to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, Ray ray)
        {
            List<PointXY> intersections = RayIntersectionExtensions.GetPointIntersections(ray, source);
            return RayIntersectionExtensions.OrderPointIntersections(ray, intersections);
        }

        private static readonly double[] PolynomialOne = { 1d };

        /// <summary>
        /// Returns isolated point intersections between a cubic Bezier curve and a line by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="line">The line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, Line line)
        {
            double startValue = GetSignedDistance(line, source.StartPoint);
            double controlAValue = GetSignedDistance(line, source.ControlPointA);
            double controlBValue = GetSignedDistance(line, source.ControlPointB);
            double endValue = GetSignedDistance(line, source.EndPoint);
            double cubic = -startValue + 3.0 * controlAValue - 3.0 * controlBValue + endValue;
            double quadratic = 3.0 * startValue - 6.0 * controlAValue + 3.0 * controlBValue;
            double linear = 3.0 * (controlAValue - startValue);

            if (cubic == 0.0 && quadratic == 0.0 && linear == 0.0 && startValue == 0.0)
            {
                return source.StartPoint.Equals(source.ControlPointA) &&
                    source.StartPoint.Equals(source.ControlPointB) &&
                    source.StartPoint.Equals(source.EndPoint)
                    ? new List<PointXY> { source.StartPoint }
                    : new List<PointXY>();
            }

            List<double> parameters = new List<double>();
            AddEndpointParameters(startValue, endValue, parameters);
            AddCubicRoots(cubic, quadratic, linear, startValue, parameters);

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
        /// Returns isolated point intersections between a cubic Bezier curve and a parameterized line by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="line">The parameterized line to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the parameterized direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(source, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a cubic Bezier curve and a segment by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="segment">The segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the segment's first endpoint to its second endpoint. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, Segment segment)
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
        /// Returns isolated point intersections between a cubic Bezier curve and a parameterized segment by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="segment">The parameterized segment to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the parameterized segment's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, ParameterizedSegment segment)
        {
            return GetPointIntersections(source, (Segment)segment);
        }

        /// <summary>
        /// Returns the distinct isolated point intersections between a cubic Bezier curve and a parameterized segment chain by solving the curve polynomial.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="segmentChain">The parameterized segment chain to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the chain's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, ParameterizedSegmentChain segmentChain)
        {
            return ParameterizedSegmentChainIntersectionExtensions.GetPointIntersections(segmentChain, segment => GetPointIntersections(source, segment));
        }

        /// <summary>
        /// Returns isolated point intersections between a cubic Bezier curve and an arc by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="arc">The arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered counterclockwise from the arc's start angle.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, Arc arc)
        {
            double[] polynomial = CreateCirclePolynomial(source, arc);
            List<PointXY> intersections = new List<PointXY>();

            if (PolynomialRootIsolation.IsZero(polynomial))
            {
                if (source.StartPoint.Equals(source.ControlPointA) &&
                    source.StartPoint.Equals(source.ControlPointB) &&
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
        /// Returns isolated point intersections between a cubic Bezier curve and a parameterized arc by numerically isolating the roots of the original curve-circle polynomial above float precision.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="arc">The parameterized arc to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the arc's start point to its end point in its angular direction.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, ParameterizedArc arc)
        {
            List<PointXY> intersections = GetPointIntersections(source, (Arc)arc);
            ParameterizedArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        /// <summary>
        /// Returns isolated point intersections between a cubic and a quadratic Bezier curve by numerically isolating the roots of the original sextic resultant above float precision.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="curve">The quadratic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the quadratic curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, QuadraticBezier curve)
        {
            double[] x =
            {
                source.StartPoint.X,
                3d * ((double)source.ControlPointA.X - source.StartPoint.X),
                3d * ((double)source.StartPoint.X - 2d * source.ControlPointA.X + source.ControlPointB.X),
                -(double)source.StartPoint.X + 3d * source.ControlPointA.X - 3d * source.ControlPointB.X + source.EndPoint.X
            };
            double[] y =
            {
                source.StartPoint.Y,
                3d * ((double)source.ControlPointA.Y - source.StartPoint.Y),
                3d * ((double)source.StartPoint.Y - 2d * source.ControlPointA.Y + source.ControlPointB.Y),
                -(double)source.StartPoint.Y + 3d * source.ControlPointA.Y - 3d * source.ControlPointB.Y + source.EndPoint.Y
            };
            double[] polynomial = QuadraticBezierIntersectionExtensions.CreateImplicitPolynomial(curve, x, y);
            bool sourceIsPoint = source.StartPoint.Equals(source.ControlPointA) &&
                source.StartPoint.Equals(source.ControlPointB) &&
                source.StartPoint.Equals(source.EndPoint);
            return QuadraticBezierIntersectionExtensions.GetPointIntersections(curve, polynomial, parameter => Evaluate(source, parameter), sourceIsPoint);
        }

        /// <summary>
        /// Returns isolated point intersections between two cubic Bezier curves by numerically isolating the roots of the original resultant of degree up to nine above float precision.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="curve">The cubic Bezier curve to intersect with the source curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the second curve's start point to its end point. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, CubicBezier curve)
        {
            CreatePowerCoordinates(source, out double[] sourceX, out double[] sourceY);
            CreatePowerCoordinates(curve, out double[] targetX, out double[] targetY);
            double[] polynomial = CreateResultantPolynomial(sourceX, sourceY, targetX, targetY);
            bool sourceIsPoint = source.StartPoint.Equals(source.ControlPointA) &&
                source.StartPoint.Equals(source.ControlPointB) &&
                source.StartPoint.Equals(source.EndPoint);
            return GetPointIntersections(curve, polynomial, parameter => Evaluate(source, parameter), sourceIsPoint);
        }

        /// <summary>Returns isolated point intersections between a cubic Bezier curve and a B-spline by solving the original polynomial equations.</summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="curve">The B-spline to intersect with the source Bezier curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, BSpline curve)
        {
            List<PointXY> intersections = BSplineIntersectionExtensions.GetPointIntersections(curve, source);
            BSplineIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>Returns isolated point intersections between a cubic Bezier curve and a NURBS curve by solving the original rational-polynomial equations.</summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="curve">The NURBS curve to intersect with the source Bezier curve.</param>
        /// <returns>A new mutable list owned by the caller, ordered from the spline's start point to its end point. Continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this CubicBezier source, Nurbs curve)
        {
            List<PointXY> intersections = NurbsIntersectionExtensions.GetPointIntersections(curve, source);
            NurbsIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        /// <summary>
        /// Extracts, validates, and orders cubic-target intersections represented by a resultant polynomial.
        /// </summary>
        private static List<PointXY> GetPointIntersections(CubicBezier curve, double[] polynomial, System.Func<double, PointXY> evaluateSource, bool sourceIsPoint)
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
        /// Orders distinct known intersections from a cubic Bezier curve's start point to its end point.
        /// </summary>
        /// <param name="curve">The target cubic Bezier curve.</param>
        /// <param name="intersections">The caller-owned intersection list to update.</param>
        internal static void OrderPointIntersections(CubicBezier curve, List<PointXY> intersections)
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
        /// Creates the resultant of the source coordinate polynomials and the target cubic parameter.
        /// </summary>
        private static double[] CreateResultantPolynomial(double[] sourceX, double[] sourceY, double[] targetX, double[] targetY)
        {
            targetX = Trim(targetX);
            targetY = Trim(targetY);
            int xDegree = targetX.Length - 1;
            int yDegree = targetY.Length - 1;

            if (xDegree == 0 && yDegree == 0)
            {
                double[] pointPolynomial = SubtractSourceCoordinate(targetX[0], sourceX);
                return PolynomialRootIsolation.IsZero(pointPolynomial)
                    ? SubtractSourceCoordinate(targetY[0], sourceY)
                    : pointPolynomial;
            }

            if (xDegree == 0)
                return SubtractSourceCoordinate(targetX[0], sourceX);

            if (yDegree == 0)
                return SubtractSourceCoordinate(targetY[0], sourceY);

            double[][] xCoefficients = CreateParameterCoefficients(targetX, sourceX);
            double[][] yCoefficients = CreateParameterCoefficients(targetY, sourceY);
            int size = xDegree + yDegree;
            double[][][] matrix = new double[size][][];
            for (int row = 0; row < size; row++)
                matrix[row] = new double[size][];

            for (int row = 0; row < yDegree; row++)
            {
                for (int coefficient = 0; coefficient <= xDegree; coefficient++)
                    matrix[row][row + coefficient] = xCoefficients[xDegree - coefficient];
            }

            for (int row = 0; row < xDegree; row++)
            {
                for (int coefficient = 0; coefficient <= yDegree; coefficient++)
                    matrix[yDegree + row][row + coefficient] = yCoefficients[yDegree - coefficient];
            }

            double[] determinant = new[] { 0d };
            AddDeterminantTerms(matrix, 0, new bool[size], PolynomialOne, false, ref determinant);
            return Trim(determinant);
        }

        /// <summary>
        /// Recursively adds the nonzero terms of a polynomial matrix determinant.
        /// </summary>
        private static void AddDeterminantTerms(double[][][] matrix, int row, bool[] usedColumns, double[] product, bool isNegative, ref double[] determinant)
        {
            int size = usedColumns.Length;
            if (row == size)
            {
                AddPolynomial(ref determinant, product, isNegative ? -1d : 1d);
                return;
            }

            for (int column = 0; column < size; column++)
            {
                double[]? entry = matrix[row][column];
                if (usedColumns[column] || entry == null || PolynomialRootIsolation.IsZero(entry))
                    continue;

                int largerUsedColumnCount = 0;
                for (int previous = column + 1; previous < size; previous++)
                {
                    if (usedColumns[previous])
                        largerUsedColumnCount++;
                }

                usedColumns[column] = true;
                AddDeterminantTerms(
                    matrix,
                    row + 1,
                    usedColumns,
                    Multiply(product, entry),
                    isNegative ^ (largerUsedColumnCount % 2 != 0),
                    ref determinant);
                usedColumns[column] = false;
            }
        }

        /// <summary>
        /// Creates the target-parameter coefficient polynomials for one coordinate equation.
        /// </summary>
        private static double[][] CreateParameterCoefficients(double[] target, double[] source)
        {
            double[][] coefficients = new double[target.Length][];
            coefficients[0] = SubtractSourceCoordinate(target[0], source);
            for (int i = 1; i < target.Length; i++)
                coefficients[i] = new[] { target[i] };

            return coefficients;
        }

        /// <summary>
        /// Returns a target constant minus a source coordinate polynomial.
        /// </summary>
        private static double[] SubtractSourceCoordinate(double targetConstant, double[] source)
        {
            double[] result = new double[source.Length];
            result[0] = targetConstant - source[0];
            for (int i = 1; i < source.Length; i++)
                result[i] = -source[i];

            return Trim(result);
        }

        /// <summary>
        /// Multiplies two polynomials in ascending power order.
        /// </summary>
        private static double[] Multiply(double[] left, double[] right)
        {
            double[] product = new double[left.Length + right.Length - 1];
            for (int i = 0; i < left.Length; i++)
            {
                for (int j = 0; j < right.Length; j++)
                    product[i + j] += left[i] * right[j];
            }

            return Trim(product);
        }

        /// <summary>
        /// Adds a scaled polynomial to an accumulator.
        /// </summary>
        private static void AddPolynomial(ref double[] accumulator, double[] polynomial, double scale)
        {
            if (accumulator.Length < polynomial.Length)
                System.Array.Resize(ref accumulator, polynomial.Length);

            for (int i = 0; i < polynomial.Length; i++)
                accumulator[i] += scale * polynomial[i];
        }

        /// <summary>
        /// Removes exactly zero leading polynomial coefficients.
        /// </summary>
        private static double[] Trim(double[] polynomial)
        {
            int length = polynomial.Length;
            while (length > 1 && polynomial[length - 1] == 0d)
                length--;

            if (length == polynomial.Length)
                return polynomial;

            double[] result = new double[length];
            System.Array.Copy(polynomial, result, length);
            return result;
        }

        /// <summary>
        /// Creates the X- and Y-coordinate power polynomials of a cubic Bezier curve.
        /// </summary>
        private static void CreatePowerCoordinates(CubicBezier curve, out double[] x, out double[] y)
        {
            x = new[]
            {
                (double)curve.StartPoint.X,
                3d * ((double)curve.ControlPointA.X - curve.StartPoint.X),
                3d * ((double)curve.StartPoint.X - 2d * curve.ControlPointA.X + curve.ControlPointB.X),
                -(double)curve.StartPoint.X + 3d * curve.ControlPointA.X - 3d * curve.ControlPointB.X + curve.EndPoint.X
            };
            y = new[]
            {
                (double)curve.StartPoint.Y,
                3d * ((double)curve.ControlPointA.Y - curve.StartPoint.Y),
                3d * ((double)curve.StartPoint.Y - 2d * curve.ControlPointA.Y + curve.ControlPointB.Y),
                -(double)curve.StartPoint.Y + 3d * curve.ControlPointA.Y - 3d * curve.ControlPointB.Y + curve.EndPoint.Y
            };
        }

        /// <summary>
        /// Finds the first normalized parameter at which a cubic Bezier curve produces a point in public float precision.
        /// </summary>
        private static bool TryGetCurveCoordinate(CubicBezier curve, PointXY point, out double coordinate)
        {
            CreatePowerCoordinates(curve, out double[] x, out double[] y);
            x[0] -= point.X;
            y[0] -= point.Y;

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
                if (candidates[i] < coordinate)
                    coordinate = candidates[i];
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
        /// Creates the sextic squared-distance polynomial between a cubic Bezier curve and a circle.
        /// </summary>
        private static double[] CreateCirclePolynomial(CubicBezier source, Arc arc)
        {
            double[] x =
            {
                (double)source.StartPoint.X - arc.Center.X,
                3d * ((double)source.ControlPointA.X - source.StartPoint.X),
                3d * ((double)source.StartPoint.X - 2d * source.ControlPointA.X + source.ControlPointB.X),
                -(double)source.StartPoint.X + 3d * source.ControlPointA.X - 3d * source.ControlPointB.X + source.EndPoint.X
            };
            double[] y =
            {
                (double)source.StartPoint.Y - arc.Center.Y,
                3d * ((double)source.ControlPointA.Y - source.StartPoint.Y),
                3d * ((double)source.StartPoint.Y - 2d * source.ControlPointA.Y + source.ControlPointB.Y),
                -(double)source.StartPoint.Y + 3d * source.ControlPointA.Y - 3d * source.ControlPointB.Y + source.EndPoint.Y
            };

            double[] polynomial = new double[7];
            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                    polynomial[i + j] += x[i] * x[j] + y[i] * y[j];
            }

            polynomial[0] -= (double)arc.Radius * arc.Radius;
            return polynomial;
        }

        /// <summary>
        /// Evaluates a cubic Bezier curve in double precision and rounds the result to the public point type.
        /// </summary>
        private static PointXY Evaluate(CubicBezier source, double parameter)
        {
            double inverse = 1d - parameter;
            double startWeight = inverse * inverse * inverse;
            double controlAWeight = 3d * inverse * inverse * parameter;
            double controlBWeight = 3d * inverse * parameter * parameter;
            double endWeight = parameter * parameter * parameter;
            return new PointXY(
                (float)(startWeight * source.StartPoint.X + controlAWeight * source.ControlPointA.X + controlBWeight * source.ControlPointB.X + endWeight * source.EndPoint.X),
                (float)(startWeight * source.StartPoint.Y + controlAWeight * source.ControlPointA.Y + controlBWeight * source.ControlPointB.Y + endWeight * source.EndPoint.Y));
        }

        /// <summary>
        /// Returns a degenerate segment point when it belongs to the curve and is included by the segment.
        /// </summary>
        /// <param name="source">The source cubic Bezier curve.</param>
        /// <param name="segment">The degenerate segment to intersect.</param>
        /// <returns>A new mutable list containing the isolated point intersection, or an empty list.</returns>
        private static List<PointXY> GetPointIntersection(CubicBezier source, Segment segment)
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
        /// Adds roots strictly inside the unit parameter interval for a cubic polynomial.
        /// </summary>
        /// <param name="cubic">The cubic coefficient.</param>
        /// <param name="quadratic">The quadratic coefficient.</param>
        /// <param name="linear">The linear coefficient.</param>
        /// <param name="constant">The constant coefficient.</param>
        /// <param name="parameters">The parameter list to update.</param>
        private static void AddCubicRoots(double cubic, double quadratic, double linear, double constant, List<double> parameters)
        {
            if (cubic == 0.0)
            {
                AddQuadraticRoots(quadratic, linear, constant, parameters);
                return;
            }

            double normalizedQuadratic = quadratic / cubic;
            double normalizedLinear = linear / cubic;
            double normalizedConstant = constant / cubic;
            double p = normalizedLinear - normalizedQuadratic * normalizedQuadratic / 3.0;
            double q = 2.0 * normalizedQuadratic * normalizedQuadratic * normalizedQuadratic / 27.0 -
                normalizedQuadratic * normalizedLinear / 3.0 +
                normalizedConstant;
            double discriminant = q * q / 4.0 + p * p * p / 27.0;
            double offset = normalizedQuadratic / 3.0;

            if (discriminant > 0.0)
            {
                double sqrtDiscriminant = System.Math.Sqrt(discriminant);
                double root = CubeRoot(-q / 2.0 + sqrtDiscriminant) +
                    CubeRoot(-q / 2.0 - sqrtDiscriminant) -
                    offset;
                AddInteriorParameter(root, parameters);
                return;
            }

            if (discriminant == 0.0)
            {
                double root = CubeRoot(-q / 2.0);
                AddInteriorParameter(2.0 * root - offset, parameters);
                AddInteriorParameter(-root - offset, parameters);
                return;
            }

            double magnitude = 2.0 * System.Math.Sqrt(-p / 3.0);
            double acosDenominator = 2.0 * System.Math.Sqrt(-(p * p * p) / 27.0);
            double acosArgument = System.Math.Max(-1.0, System.Math.Min(1.0, -q / acosDenominator));
            double angle = System.Math.Acos(acosArgument);

            AddInteriorParameter(magnitude * System.Math.Cos(angle / 3.0) - offset, parameters);
            AddInteriorParameter(magnitude * System.Math.Cos((angle + 2.0 * System.Math.PI) / 3.0) - offset, parameters);
            AddInteriorParameter(magnitude * System.Math.Cos((angle + 4.0 * System.Math.PI) / 3.0) - offset, parameters);
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

        /// <summary>
        /// Returns the real cube root of a value.
        /// </summary>
        /// <param name="value">The value whose cube root to calculate.</param>
        /// <returns>The real cube root.</returns>
        private static double CubeRoot(double value) =>
            value < 0.0 ? -System.Math.Pow(-value, 1.0 / 3.0) : System.Math.Pow(value, 1.0 / 3.0);
    }
}
