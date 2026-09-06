using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    // Intersects the original polynomial and rational spline spans. The cached polyline used by
    // length, projection, and rasterization is intentionally not involved in these operations.
    internal static class SplineIntersectionOperations
    {
        public static List<PolynomialCurveSpan> CreateSpans(BSpline curve) =>
            PolynomialCurveSpan.CreateSplineSpans(curve.Degree, curve.ControlPoints, null, curve.Knots);

        public static List<PolynomialCurveSpan> CreateSpans(Nurbs curve) =>
            PolynomialCurveSpan.CreateSplineSpans(curve.Degree, curve.ControlPoints, curve.Weights, curve.Knots);

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, Line line)
        {
            List<PointXY> intersections = GetLinearIntersections(
                source,
                line,
                _ => true,
                point => GetLineDirections(line.Direction));

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - line.ClosestPointToOrigin, line.Direction).CompareTo(
                    VectorXY.Dot(right - line.ClosestPointToOrigin, line.Direction)));
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, ParameterizedLine line)
        {
            List<PointXY> intersections = GetPointIntersections(source, line.Line);
            if (VectorXY.Dot(line.Direction, line.Line.Direction) < 0f)
                intersections.Reverse();

            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, Ray ray)
        {
            var supportingLine = new Line(ray.Origin, ray.Origin + ray.Direction);
            List<PointXY> intersections = GetLinearIntersections(
                source,
                supportingLine,
                point => VectorXY.Dot(point - ray.Origin, ray.Direction) >= 0f,
                point => GetRayDirections(ray, point));

            AddIfIncluded(source, ray.Origin, intersections);
            RemoveContinuousLinearOverlaps(
                source,
                supportingLine,
                point => VectorXY.Dot(point - ray.Origin, ray.Direction) >= 0f,
                point => GetRayDirections(ray, point),
                intersections);
            intersections.Sort((left, right) =>
                VectorXY.Dot(left - ray.Origin, ray.Direction).CompareTo(
                    VectorXY.Dot(right - ray.Origin, ray.Direction)));
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, Segment segment)
        {
            VectorXY direction = segment.EndpointB - segment.EndpointA;
            if (direction.SquaredLength == 0f)
            {
                return (segment.IncludesEndpointA || segment.IncludesEndpointB) && IncludesPoint(source, segment.EndpointA)
                    ? new List<PointXY> { segment.EndpointA }
                    : new List<PointXY>();
            }

            var supportingLine = new Line(segment.EndpointA, segment.EndpointB);
            List<PointXY> intersections = GetLinearIntersections(
                source,
                supportingLine,
                point => IncludesSupportingLineCoordinate(segment, point),
                point => GetSegmentDirections(segment, point));

            if (segment.IncludesEndpointA)
                AddIfIncluded(source, segment.EndpointA, intersections);
            if (segment.IncludesEndpointB)
                AddIfIncluded(source, segment.EndpointB, intersections);

            RemoveContinuousLinearOverlaps(
                source,
                supportingLine,
                point => IncludesSupportingLineCoordinate(segment, point),
                point => GetSegmentDirections(segment, point),
                intersections);
            SegmentIntersectionExtensions.RestrictSupportingLineIntersectionsToSegment(intersections, segment);
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, ParameterizedSegment segment) =>
            GetPointIntersections(source, (Segment)segment);

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, ParameterizedSegmentChain segmentChain)
        {
            List<PointXY> intersections = new List<PointXY>();
            for (int i = 0; i < segmentChain.Segments.Count; i++)
                AddDistinct(intersections, GetPointIntersections(source, segmentChain.Segments[i]));

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                PointXY point = intersections[i];
                for (int segmentIndex = 0; segmentIndex < segmentChain.Segments.Count; segmentIndex++)
                {
                    ParameterizedSegment segment = segmentChain.Segments[segmentIndex];
                    var line = new Line(segment.StartPoint, segment.EndPoint);
                    if (BelongsToContinuousLinearOverlap(
                        source,
                        line,
                        candidate => IncludesSupportingLineCoordinate((Segment)segment, candidate),
                        candidate => GetSegmentDirections((Segment)segment, candidate),
                        point))
                    {
                        intersections.RemoveAt(i);
                        break;
                    }
                }
            }

            return ParameterizedSegmentChainIntersectionExtensions.OrderPointIntersections(segmentChain, intersections);
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, Arc arc)
        {
            List<PointXY> intersections = new List<PointXY>();
            List<PolynomialCurveSpan> coincidentSpans = new List<PolynomialCurveSpan>();

            for (int i = 0; i < source.Count; i++)
            {
                PolynomialCurveSpan span = source[i];
                double[] polynomial = CreateCirclePolynomial(span, arc);
                if (PolynomialRootIsolation.IsZero(polynomial))
                {
                    if (span.IsPoint && IsWithinAngularRegion(arc, span.StartPoint))
                        AddDistinct(intersections, span.StartPoint);
                    else if (!span.IsPoint)
                        coincidentSpans.Add(span);

                    continue;
                }

                List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(polynomial);
                for (int rootIndex = 0; rootIndex < roots.Count; rootIndex++)
                {
                    PointXY point = span.Evaluate(roots[rootIndex]);
                    if (IsWithinAngularRegion(arc, point))
                        AddDistinct(intersections, point);
                }
            }

            if (arc.Radius == 0f)
            {
                AddIfIncluded(source, arc.Center, intersections);
            }
            else
            {
                AddIfIncluded(source, arc.StartPoint, intersections);
                AddIfIncluded(source, arc.EndPoint, intersections);
            }

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (BelongsToContinuousArcOverlap(coincidentSpans, arc, intersections[i]))
                    intersections.RemoveAt(i);
            }

            ArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, ParameterizedArc arc)
        {
            List<PointXY> intersections = GetPointIntersections(source, (Arc)arc);
            ParameterizedArcIntersectionExtensions.OrderPointIntersections(arc, intersections);
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, QuadraticBezier curve)
        {
            List<PointXY> intersections = GetPointIntersections(
                source,
                new[] { PolynomialCurveSpan.Create(curve) });
            QuadraticBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(IReadOnlyList<PolynomialCurveSpan> source, CubicBezier curve)
        {
            List<PointXY> intersections = GetPointIntersections(
                source,
                new[] { PolynomialCurveSpan.Create(curve) });
            CubicBezierIntersectionExtensions.OrderPointIntersections(curve, intersections);
            return intersections;
        }

        public static List<PointXY> GetPointIntersections(
            IReadOnlyList<PolynomialCurveSpan> source,
            IReadOnlyList<PolynomialCurveSpan> target)
        {
            List<PointXY> intersections = new List<PointXY>();
            var continuousOverlaps = new List<(PolynomialCurveSpan Source, PolynomialCurveSpan Target)>();

            for (int sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
            {
                PolynomialCurveSpan sourceSpan = source[sourceIndex];
                for (int targetIndex = 0; targetIndex < target.Count; targetIndex++)
                    AddSpanIntersections(
                        sourceSpan,
                        target[targetIndex],
                        intersections,
                        continuousOverlaps);
            }

            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                PointXY point = intersections[i];
                for (int overlapIndex = 0; overlapIndex < continuousOverlaps.Count; overlapIndex++)
                {
                    (PolynomialCurveSpan sourceSpan, PolynomialCurveSpan targetSpan) = continuousOverlaps[overlapIndex];
                    if (HaveCommonContinuation(sourceSpan, targetSpan, point))
                    {
                        intersections.RemoveAt(i);
                        break;
                    }
                }
            }

            OrderPointIntersections(target, intersections);
            return intersections;
        }

        private static void AddSpanIntersections(
            PolynomialCurveSpan source,
            PolynomialCurveSpan target,
            List<PointXY> intersections,
            List<(PolynomialCurveSpan Source, PolynomialCurveSpan Target)> continuousOverlaps)
        {
            if (source.IsPoint)
            {
                if (target.IncludesPoint(source.StartPoint))
                    AddDistinct(intersections, source.StartPoint);

                return;
            }

            if (target.IsPoint)
            {
                if (source.IncludesPoint(target.StartPoint))
                    AddDistinct(intersections, target.StartPoint);

                return;
            }

            PolynomialCurveSpan resultantSource = source;
            PolynomialCurveSpan resultantTarget = target;
            if (target.Degree > source.Degree)
            {
                resultantSource = target;
                resultantTarget = source;
            }

            double[] resultant = CreateResultant(resultantSource, resultantTarget);
            if (PolynomialRootIsolation.IsZero(resultant))
            {
                continuousOverlaps.Add((source, target));
                AddCommonEndpoints(source, target, intersections);
                return;
            }

            List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(resultant);
            for (int rootIndex = 0; rootIndex < roots.Count; rootIndex++)
            {
                AddIntersectionsAtParameter(
                    resultantSource,
                    roots[rootIndex],
                    resultantTarget,
                    intersections);
            }
        }

        public static void OrderPointIntersections(IReadOnlyList<PolynomialCurveSpan> target, List<PointXY> intersections)
        {
            RemoveDuplicates(intersections);
            intersections.Sort((left, right) => CompareCoordinates(target, left, right));
        }

        private static List<PointXY> GetLinearIntersections(
            IReadOnlyList<PolynomialCurveSpan> source,
            Line line,
            Func<PointXY, bool> includesPoint,
            Func<PointXY, List<PolynomialDirection>> getTargetDirections)
        {
            List<PointXY> intersections = new List<PointXY>();
            for (int i = 0; i < source.Count; i++)
            {
                PolynomialCurveSpan span = source[i];
                double[] polynomial = Add(
                    Add(Scale(span.X, line.EquationA), Scale(span.Y, line.EquationB)),
                    Scale(span.Weight, line.EquationC));
                if (PolynomialRootIsolation.IsZero(polynomial))
                {
                    if (span.IsPoint && includesPoint(span.StartPoint))
                        AddDistinct(intersections, span.StartPoint);

                    continue;
                }

                List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(polynomial);
                for (int rootIndex = 0; rootIndex < roots.Count; rootIndex++)
                {
                    PointXY point = span.Evaluate(roots[rootIndex]);
                    if (includesPoint(point))
                        AddDistinct(intersections, point);
                }
            }

            RemoveContinuousLinearOverlaps(source, line, includesPoint, getTargetDirections, intersections);
            return intersections;
        }

        private static void RemoveContinuousLinearOverlaps(
            IReadOnlyList<PolynomialCurveSpan> source,
            Line line,
            Func<PointXY, bool> includesPoint,
            Func<PointXY, List<PolynomialDirection>> getTargetDirections,
            List<PointXY> intersections)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (BelongsToContinuousLinearOverlap(source, line, includesPoint, getTargetDirections, intersections[i]))
                    intersections.RemoveAt(i);
            }
        }

        private static bool BelongsToContinuousLinearOverlap(
            IReadOnlyList<PolynomialCurveSpan> source,
            Line line,
            Func<PointXY, bool> includesPoint,
            Func<PointXY, List<PolynomialDirection>> getTargetDirections,
            PointXY point)
        {
            if (!includesPoint(point))
                return false;

            List<PolynomialDirection> targetDirections = getTargetDirections(point);
            for (int i = 0; i < source.Count; i++)
            {
                PolynomialCurveSpan span = source[i];
                double[] polynomial = Add(
                    Add(Scale(span.X, line.EquationA), Scale(span.Y, line.EquationB)),
                    Scale(span.Weight, line.EquationC));
                if (!PolynomialRootIsolation.IsZero(polynomial) || span.IsPoint || !span.IncludesPoint(point))
                    continue;

                if (HaveCommonDirection(span.GetContinuationDirections(point), targetDirections))
                    return true;
            }

            return false;
        }

        private static bool BelongsToContinuousArcOverlap(
            List<PolynomialCurveSpan> coincidentSpans,
            Arc arc,
            PointXY point)
        {
            if (!IsWithinAngularRegion(arc, point))
                return false;

            List<PolynomialDirection> targetDirections = GetArcDirections(arc, point);
            for (int i = 0; i < coincidentSpans.Count; i++)
            {
                PolynomialCurveSpan span = coincidentSpans[i];
                if (span.IncludesPoint(point) &&
                    HaveCommonDirection(span.GetContinuationDirections(point), targetDirections))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HaveCommonContinuation(
            PolynomialCurveSpan source,
            PolynomialCurveSpan target,
            PointXY point)
        {
            if (!source.IncludesPoint(point) || !target.IncludesPoint(point))
                return false;

            return HaveCommonDirection(
                source.GetContinuationDirections(point),
                target.GetContinuationDirections(point));
        }

        private static bool HaveCommonDirection(
            List<PolynomialDirection> source,
            List<PolynomialDirection> target)
        {
            for (int i = 0; i < source.Count; i++)
            {
                for (int j = 0; j < target.Count; j++)
                {
                    if (PolynomialDirection.Cross(source[i], target[j]) == 0d &&
                        PolynomialDirection.Dot(source[i], target[j]) > 0d)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static List<PolynomialDirection> GetLineDirections(VectorXY direction) => new List<PolynomialDirection>
        {
            new PolynomialDirection(direction.X, direction.Y),
            new PolynomialDirection(-direction.X, -direction.Y)
        };

        private static List<PolynomialDirection> GetRayDirections(Ray ray, PointXY point)
        {
            var forward = new PolynomialDirection(ray.Direction.X, ray.Direction.Y);
            if (point.Equals(ray.Origin))
                return new List<PolynomialDirection> { forward };

            return new List<PolynomialDirection> { forward, -forward };
        }

        private static List<PolynomialDirection> GetSegmentDirections(Segment segment, PointXY point)
        {
            VectorXY direction = segment.EndpointB - segment.EndpointA;
            var forward = new PolynomialDirection(direction.X, direction.Y);
            if (point.Equals(segment.EndpointA))
                return segment.IncludesEndpointA ? new List<PolynomialDirection> { forward } : new List<PolynomialDirection>();
            if (point.Equals(segment.EndpointB))
                return segment.IncludesEndpointB ? new List<PolynomialDirection> { -forward } : new List<PolynomialDirection>();

            return new List<PolynomialDirection> { forward, -forward };
        }

        private static bool IncludesSupportingLineCoordinate(Segment segment, PointXY point)
        {
            VectorXY direction = segment.EndpointB - segment.EndpointA;
            float coordinate = VectorXY.Dot(point - segment.EndpointA, direction);
            if (coordinate < 0f || coordinate > direction.SquaredLength)
                return false;
            if (coordinate == 0f && !segment.IncludesEndpointA)
                return false;
            if (coordinate == direction.SquaredLength && !segment.IncludesEndpointB)
                return false;

            return true;
        }

        private static List<PolynomialDirection> GetArcDirections(Arc arc, PointXY point)
        {
            if (arc.Radius == 0f)
                return new List<PolynomialDirection>();

            VectorXY radial = point - arc.Center;
            var counterclockwise = new PolynomialDirection(-radial.Y, radial.X);
            if (arc.IsFullCircle)
                return new List<PolynomialDirection> { counterclockwise, -counterclockwise };

            var directions = new List<PolynomialDirection>();
            if (!point.Equals(arc.EndPoint))
                directions.Add(counterclockwise);
            if (!point.Equals(arc.StartPoint))
                directions.Add(-counterclockwise);
            return directions;
        }

        private static bool IsWithinAngularRegion(Arc arc, PointXY point) =>
            arc.Radius == 0f || ArcIntersectionExtensions.IsWithinAngularRegion(arc, point);

        private static void AddIntersectionsAtParameter(
            PolynomialCurveSpan source,
            double sourceParameter,
            PolynomialCurveSpan target,
            List<PointXY> intersections)
        {
            source.EvaluateHomogeneous(sourceParameter, out double sourceX, out double sourceY, out double sourceWeight);
            double[] xEquation = Subtract(Scale(target.Weight, sourceX), Scale(target.X, sourceWeight));
            double[] yEquation = Subtract(Scale(target.Weight, sourceY), Scale(target.Y, sourceWeight));
            double[] equation = PolynomialRootIsolation.IsZero(xEquation) ? yEquation : xEquation;
            if (PolynomialRootIsolation.IsZero(equation))
                return;

            PointXY sourcePoint = source.Evaluate(sourceParameter);
            List<double> targetParameters = PolynomialRootIsolation.FindRootsInUnitInterval(equation);
            for (int i = 0; i < targetParameters.Count; i++)
            {
                PointXY targetPoint = target.Evaluate(targetParameters[i]);
                if (targetPoint.Equals(sourcePoint))
                    AddDistinct(intersections, targetPoint);
            }
        }

        private static void AddCommonEndpoints(
            PolynomialCurveSpan source,
            PolynomialCurveSpan target,
            List<PointXY> intersections)
        {
            AddIfIncluded(target, source.StartPoint, intersections);
            AddIfIncluded(target, source.EndPoint, intersections);
            AddIfIncluded(source, target.StartPoint, intersections);
            AddIfIncluded(source, target.EndPoint, intersections);
        }

        private static void AddIfIncluded(
            IReadOnlyList<PolynomialCurveSpan> spans,
            PointXY point,
            List<PointXY> intersections)
        {
            if (IncludesPoint(spans, point))
                AddDistinct(intersections, point);
        }

        private static void AddIfIncluded(
            PolynomialCurveSpan span,
            PointXY point,
            List<PointXY> intersections)
        {
            if (span.IncludesPoint(point))
                AddDistinct(intersections, point);
        }

        private static bool IncludesPoint(IReadOnlyList<PolynomialCurveSpan> spans, PointXY point)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                if (spans[i].IncludesPoint(point))
                    return true;
            }

            return false;
        }

        private static int CompareCoordinates(
            IReadOnlyList<PolynomialCurveSpan> target,
            PointXY left,
            PointXY right)
        {
            TryGetCoordinate(target, left, out int leftSpan, out double leftParameter);
            TryGetCoordinate(target, right, out int rightSpan, out double rightParameter);
            int spanComparison = leftSpan.CompareTo(rightSpan);
            return spanComparison != 0 ? spanComparison : leftParameter.CompareTo(rightParameter);
        }

        private static bool TryGetCoordinate(
            IReadOnlyList<PolynomialCurveSpan> spans,
            PointXY point,
            out int spanIndex,
            out double parameter)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                List<double> parameters = spans[i].FindParameters(point);
                if (parameters.Count > 0)
                {
                    spanIndex = i;
                    parameter = parameters[0];
                    return true;
                }
            }

            spanIndex = int.MaxValue;
            parameter = double.PositiveInfinity;
            return false;
        }

        private static double[] CreateCirclePolynomial(PolynomialCurveSpan source, Arc arc)
        {
            double[] x = Subtract(source.X, Scale(source.Weight, arc.Center.X));
            double[] y = Subtract(source.Y, Scale(source.Weight, arc.Center.Y));
            return Subtract(
                Add(Multiply(x, x), Multiply(y, y)),
                Scale(Multiply(source.Weight, source.Weight), (double)arc.Radius * arc.Radius));
        }

        private static double[] CreateResultant(PolynomialCurveSpan source, PolynomialCurveSpan target)
        {
            double[][] xEquation = CreateParameterEquation(source.X, source.Weight, target.X, target.Weight);
            double[][] yEquation = CreateParameterEquation(source.Y, source.Weight, target.Y, target.Weight);
            int xDegree = GetCoefficientDegree(xEquation);
            int yDegree = GetCoefficientDegree(yEquation);

            if (xDegree == 0)
                return Power(xEquation[0], yDegree);
            if (yDegree == 0)
                return Power(yEquation[0], xDegree);

            int size = xDegree + yDegree;
            double[][][] matrix = new double[size][][];
            for (int row = 0; row < size; row++)
            {
                matrix[row] = new double[size][];
                for (int column = 0; column < size; column++)
                    matrix[row][column] = new[] { 0d };
            }

            for (int row = 0; row < yDegree; row++)
            {
                for (int coefficient = 0; coefficient <= xDegree; coefficient++)
                    matrix[row][row + coefficient] = xEquation[xDegree - coefficient];
            }

            for (int row = 0; row < xDegree; row++)
            {
                for (int coefficient = 0; coefficient <= yDegree; coefficient++)
                    matrix[yDegree + row][row + coefficient] = yEquation[yDegree - coefficient];
            }

            double[][] characteristic = GetCharacteristicPolynomial(matrix);
            double[] determinant = characteristic[characteristic.Length - 1];
            return size % 2 == 0 ? determinant : Scale(determinant, -1d);
        }

        private static double[][] CreateParameterEquation(
            double[] sourceCoordinate,
            double[] sourceWeight,
            double[] targetCoordinate,
            double[] targetWeight)
        {
            int length = System.Math.Max(targetCoordinate.Length, targetWeight.Length);
            double[][] coefficients = new double[length][];
            for (int i = 0; i < length; i++)
            {
                double targetCoordinateCoefficient = i < targetCoordinate.Length ? targetCoordinate[i] : 0d;
                double targetWeightCoefficient = i < targetWeight.Length ? targetWeight[i] : 0d;
                coefficients[i] = Subtract(
                    Scale(sourceCoordinate, targetWeightCoefficient),
                    Scale(sourceWeight, targetCoordinateCoefficient));
            }

            return coefficients;
        }

        // Samuelson-Berkowitz computes a determinant over the polynomial coefficient ring
        // without divisions, so continuous common components remain detectable as zero resultants.
        private static double[][] GetCharacteristicPolynomial(double[][][] matrix)
        {
            int size = matrix.Length;
            if (size == 0)
                return new[] { new[] { 1d } };

            double[][][] trailing = new double[size - 1][][];
            for (int row = 1; row < size; row++)
            {
                trailing[row - 1] = new double[size - 1][];
                for (int column = 1; column < size; column++)
                    trailing[row - 1][column - 1] = matrix[row][column];
            }

            double[][] trailingCharacteristic = GetCharacteristicPolynomial(trailing);
            double[][] toeplitz = new double[size + 1][];
            toeplitz[0] = new[] { 1d };
            toeplitz[1] = Scale(matrix[0][0], -1d);

            if (size > 1)
            {
                double[][] vector = new double[size - 1][];
                for (int row = 1; row < size; row++)
                    vector[row - 1] = matrix[row][0];

                for (int index = 2; index <= size; index++)
                {
                    double[] product = new[] { 0d };
                    for (int column = 1; column < size; column++)
                        product = Add(product, Multiply(matrix[0][column], vector[column - 1]));

                    toeplitz[index] = Scale(product, -1d);
                    if (index == size)
                        continue;

                    double[][] next = new double[size - 1][];
                    for (int row = 0; row < size - 1; row++)
                    {
                        next[row] = new[] { 0d };
                        for (int column = 0; column < size - 1; column++)
                            next[row] = Add(next[row], Multiply(trailing[row][column], vector[column]));
                    }

                    vector = next;
                }
            }

            double[][] characteristic = new double[size + 1][];
            for (int row = 0; row <= size; row++)
            {
                characteristic[row] = new[] { 0d };
                int lastColumn = System.Math.Min(row, trailingCharacteristic.Length - 1);
                for (int column = 0; column <= lastColumn; column++)
                {
                    characteristic[row] = Add(
                        characteristic[row],
                        Multiply(toeplitz[row - column], trailingCharacteristic[column]));
                }
            }

            return characteristic;
        }

        private static int GetCoefficientDegree(double[][] coefficients)
        {
            int degree = coefficients.Length - 1;
            while (degree > 0 && PolynomialRootIsolation.IsZero(coefficients[degree]))
                degree--;

            return degree;
        }

        internal static double[] Add(double[] left, double[] right)
        {
            double[] result = new double[System.Math.Max(left.Length, right.Length)];
            for (int i = 0; i < result.Length; i++)
                result[i] = (i < left.Length ? left[i] : 0d) + (i < right.Length ? right[i] : 0d);

            return Trim(result);
        }

        internal static double[] Subtract(double[] left, double[] right)
        {
            double[] result = new double[System.Math.Max(left.Length, right.Length)];
            for (int i = 0; i < result.Length; i++)
                result[i] = (i < left.Length ? left[i] : 0d) - (i < right.Length ? right[i] : 0d);

            return Trim(result);
        }

        internal static double[] Scale(double[] polynomial, double scale)
        {
            double[] result = new double[polynomial.Length];
            for (int i = 0; i < polynomial.Length; i++)
                result[i] = polynomial[i] * scale;

            return Trim(result);
        }

        internal static double[] Multiply(double[] left, double[] right)
        {
            double[] result = new double[left.Length + right.Length - 1];
            for (int i = 0; i < left.Length; i++)
            {
                for (int j = 0; j < right.Length; j++)
                    result[i + j] += left[i] * right[j];
            }

            return Trim(result);
        }

        internal static double Evaluate(double[] polynomial, double parameter)
        {
            double value = polynomial[polynomial.Length - 1];
            for (int i = polynomial.Length - 2; i >= 0; i--)
                value = value * parameter + polynomial[i];

            return value;
        }

        internal static double[] Differentiate(double[] polynomial)
        {
            if (polynomial.Length <= 1)
                return new[] { 0d };

            double[] derivative = new double[polynomial.Length - 1];
            for (int i = 1; i < polynomial.Length; i++)
                derivative[i - 1] = i * polynomial[i];

            return Trim(derivative);
        }

        private static double[] Power(double[] polynomial, int exponent)
        {
            double[] result = new[] { 1d };
            for (int i = 0; i < exponent; i++)
                result = Multiply(result, polynomial);

            return result;
        }

        private static double[] Trim(double[] polynomial)
        {
            int length = polynomial.Length;
            while (length > 1 && polynomial[length - 1] == 0d)
                length--;

            if (length == polynomial.Length)
                return polynomial;

            double[] result = new double[length];
            Array.Copy(polynomial, result, length);
            return result;
        }

        private static void AddDistinct(List<PointXY> target, List<PointXY> source)
        {
            for (int i = 0; i < source.Count; i++)
                AddDistinct(target, source[i]);
        }

        private static void AddDistinct(List<PointXY> intersections, PointXY point)
        {
            if (!intersections.Contains(point))
                intersections.Add(point);
        }

        private static void RemoveDuplicates(List<PointXY> intersections)
        {
            for (int i = intersections.Count - 1; i >= 0; i--)
            {
                if (intersections.IndexOf(intersections[i]) != i)
                    intersections.RemoveAt(i);
            }
        }

    internal sealed class PolynomialCurveSpan
    {
        private static readonly double[] UnitWeight = { 1d };

        public PolynomialCurveSpan(double[] x, double[] y, double[] weight)
        {
            X = x;
            Y = y;
            Weight = weight;
            Degree = System.Math.Max(System.Math.Max(x.Length, y.Length), weight.Length) - 1;
            double[] xDerivativeNumerator = SplineIntersectionOperations.Subtract(
                SplineIntersectionOperations.Multiply(SplineIntersectionOperations.Differentiate(X), Weight),
                SplineIntersectionOperations.Multiply(X, SplineIntersectionOperations.Differentiate(Weight)));
            double[] yDerivativeNumerator = SplineIntersectionOperations.Subtract(
                SplineIntersectionOperations.Multiply(SplineIntersectionOperations.Differentiate(Y), Weight),
                SplineIntersectionOperations.Multiply(Y, SplineIntersectionOperations.Differentiate(Weight)));
            IsPoint = PolynomialRootIsolation.IsZero(xDerivativeNumerator) &&
                PolynomialRootIsolation.IsZero(yDerivativeNumerator);
        }

        public double[] X { get; }
        public double[] Y { get; }
        public double[] Weight { get; }
        public int Degree { get; }
        public bool IsPoint { get; }
        public PointXY StartPoint => Evaluate(0d);
        public PointXY EndPoint => Evaluate(1d);

        public PointXY Evaluate(double parameter)
        {
            EvaluateHomogeneous(parameter, out double x, out double y, out double weight);
            return new PointXY((float)(x / weight), (float)(y / weight));
        }

        public void EvaluateHomogeneous(double parameter, out double x, out double y, out double weight)
        {
            x = SplineIntersectionOperations.Evaluate(X, parameter);
            y = SplineIntersectionOperations.Evaluate(Y, parameter);
            weight = SplineIntersectionOperations.Evaluate(Weight, parameter);
        }

        public bool IncludesPoint(PointXY point) => FindParameters(point).Count > 0;

        public List<double> FindParameters(PointXY point)
        {
            double[] xEquation = SplineIntersectionOperations.Subtract(
                X,
                SplineIntersectionOperations.Scale(Weight, point.X));
            double[] yEquation = SplineIntersectionOperations.Subtract(
                Y,
                SplineIntersectionOperations.Scale(Weight, point.Y));
            double[] equation = PolynomialRootIsolation.IsZero(xEquation) ? yEquation : xEquation;

            if (PolynomialRootIsolation.IsZero(equation))
                return Evaluate(0d).Equals(point) ? new List<double> { 0d } : new List<double>();

            List<double> roots = PolynomialRootIsolation.FindRootsInUnitInterval(equation);
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                if (!Evaluate(roots[i]).Equals(point))
                    roots.RemoveAt(i);
            }

            if (Evaluate(0d).Equals(point) && !roots.Contains(0d))
                roots.Insert(0, 0d);
            if (Evaluate(1d).Equals(point) && !roots.Contains(1d))
                roots.Add(1d);
            return roots;
        }

        public List<PolynomialDirection> GetContinuationDirections(PointXY point)
        {
            List<PolynomialDirection> directions = new List<PolynomialDirection>();
            List<double> parameters = FindParameters(point);
            for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
            {
                double parameter = parameters[parameterIndex];
                EvaluateHomogeneous(parameter, out double x, out double y, out double weight);
                double pointX = x / weight;
                double pointY = y / weight;
                double[] xDifference = SplineIntersectionOperations.Subtract(
                    X,
                    SplineIntersectionOperations.Scale(Weight, pointX));
                double[] yDifference = SplineIntersectionOperations.Subtract(
                    Y,
                    SplineIntersectionOperations.Scale(Weight, pointY));

                for (int order = 1; order <= Degree; order++)
                {
                    xDifference = SplineIntersectionOperations.Differentiate(xDifference);
                    yDifference = SplineIntersectionOperations.Differentiate(yDifference);
                    double directionX = SplineIntersectionOperations.Evaluate(xDifference, parameter);
                    double directionY = SplineIntersectionOperations.Evaluate(yDifference, parameter);
                    if (directionX == 0d && directionY == 0d)
                        continue;

                    var direction = new PolynomialDirection(directionX, directionY);
                    if (parameter < 1d)
                        AddDirection(directions, direction);
                    if (parameter > 0d)
                        AddDirection(directions, order % 2 == 0 ? direction : -direction);
                    break;
                }
            }

            return directions;
        }

        public static PolynomialCurveSpan Create(QuadraticBezier curve)
        {
            return new PolynomialCurveSpan(
                new[]
                {
                    (double)curve.StartPoint.X,
                    2d * ((double)curve.ControlPoint.X - curve.StartPoint.X),
                    (double)curve.StartPoint.X - 2d * curve.ControlPoint.X + curve.EndPoint.X
                },
                new[]
                {
                    (double)curve.StartPoint.Y,
                    2d * ((double)curve.ControlPoint.Y - curve.StartPoint.Y),
                    (double)curve.StartPoint.Y - 2d * curve.ControlPoint.Y + curve.EndPoint.Y
                },
                UnitWeight);
        }

        public static PolynomialCurveSpan Create(CubicBezier curve)
        {
            return new PolynomialCurveSpan(
                new[]
                {
                    (double)curve.StartPoint.X,
                    3d * ((double)curve.ControlPointA.X - curve.StartPoint.X),
                    3d * ((double)curve.StartPoint.X - 2d * curve.ControlPointA.X + curve.ControlPointB.X),
                    -(double)curve.StartPoint.X + 3d * curve.ControlPointA.X - 3d * curve.ControlPointB.X + curve.EndPoint.X
                },
                new[]
                {
                    (double)curve.StartPoint.Y,
                    3d * ((double)curve.ControlPointA.Y - curve.StartPoint.Y),
                    3d * ((double)curve.StartPoint.Y - 2d * curve.ControlPointA.Y + curve.ControlPointB.Y),
                    -(double)curve.StartPoint.Y + 3d * curve.ControlPointA.Y - 3d * curve.ControlPointB.Y + curve.EndPoint.Y
                },
                UnitWeight);
        }

        public static List<PolynomialCurveSpan> CreateSplineSpans(
            int degree,
            IReadOnlyList<PointXY> controlPoints,
            IReadOnlyList<float>? weights,
            IReadOnlyList<float> knots)
        {
            var spans = new List<PolynomialCurveSpan>();
            for (int span = degree; span < controlPoints.Count; span++)
            {
                double from = knots[span];
                double to = knots[span + 1];
                if (from == to)
                    continue;

                double[][] x = new double[degree + 1][];
                double[][] y = new double[degree + 1][];
                double[][] weight = new double[degree + 1][];
                for (int j = 0; j <= degree; j++)
                {
                    int controlPointIndex = span - degree + j;
                    PointXY point = controlPoints[controlPointIndex];
                    double homogeneousWeight = weights is null ? 1d : weights[controlPointIndex];
                    x[j] = new[] { point.X * homogeneousWeight };
                    y[j] = new[] { point.Y * homogeneousWeight };
                    weight[j] = new[] { homogeneousWeight };
                }

                for (int level = 1; level <= degree; level++)
                {
                    for (int j = degree; j >= level; j--)
                    {
                        int knotIndex = span - degree + j;
                        double denominator = knots[knotIndex + degree - level + 1] - knots[knotIndex];
                        double alphaConstant = (from - knots[knotIndex]) / denominator;
                        double alphaLinear = (to - from) / denominator;
                        x[j] = Blend(x[j - 1], x[j], alphaConstant, alphaLinear);
                        y[j] = Blend(y[j - 1], y[j], alphaConstant, alphaLinear);
                        weight[j] = Blend(weight[j - 1], weight[j], alphaConstant, alphaLinear);
                    }
                }

                spans.Add(new PolynomialCurveSpan(x[degree], y[degree], weight[degree]));
            }

            return spans;
        }

        private static double[] Blend(double[] left, double[] right, double alphaConstant, double alphaLinear)
        {
            double[] difference = SplineIntersectionOperations.Subtract(right, left);
            double[] alpha = { alphaConstant, alphaLinear };
            return SplineIntersectionOperations.Add(
                left,
                SplineIntersectionOperations.Multiply(alpha, difference));
        }

        private static void AddDirection(List<PolynomialDirection> directions, PolynomialDirection direction)
        {
            for (int i = 0; i < directions.Count; i++)
            {
                if (PolynomialDirection.Cross(directions[i], direction) == 0d &&
                    PolynomialDirection.Dot(directions[i], direction) > 0d)
                {
                    return;
                }
            }

            directions.Add(direction);
        }
    }

    internal sealed class PolynomialDirection
    {
        public PolynomialDirection(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static PolynomialDirection operator -(PolynomialDirection value) =>
            new PolynomialDirection(-value.X, -value.Y);

        public static double Cross(PolynomialDirection left, PolynomialDirection right) =>
            left.X * right.Y - left.Y * right.X;

        public static double Dot(PolynomialDirection left, PolynomialDirection right) =>
            left.X * right.X + left.Y * right.Y;
    }

    }
}
