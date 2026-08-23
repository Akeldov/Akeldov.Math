using Akeldov.Math.Spatial2D;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    internal static class BezierPathApproximation
    {
        public const int DefaultSegmentCount = 64;

        public static List<ParameterizedSegment> Flatten(
            Func<float, PointXY> pointAt,
            int segmentCount)
        {
            ValidateSegmentCount(segmentCount, nameof(segmentCount));

            var segments = new List<ParameterizedSegment>(segmentCount);
            PointXY previous = pointAt(0f);

            for (int i = 1; i <= segmentCount; i++)
            {
                PointXY current = pointAt(i / (float)segmentCount);
                if (!current.Equals(previous))
                    segments.Add(new ParameterizedSegment(previous, current));

                previous = current;
            }

            return segments;
        }

        public static float GetLength(Func<float, PointXY> pointAt)
        {
            PointXY previous = pointAt(0f);
            float length = 0f;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                length += previous.Distance(current);
                previous = current;
            }

            return length;
        }

        public static PointXY[] CreatePoints(Func<float, PointXY> pointAt)
        {
            var points = new PointXY[DefaultSegmentCount + 1];
            for (int i = 0; i < points.Length; i++)
                points[i] = pointAt(i / (float)DefaultSegmentCount);

            return points;
        }

        public static float GetLength(PointXY[] points)
        {
            float length = 0f;
            for (int i = 1; i < points.Length; i++)
                length += points[i - 1].Distance(points[i]);

            return length;
        }

        public static PointXY GetPoint(
            Func<float, PointXY> pointAt,
            float length,
            float curveCoordinate)
        {
            if (float.IsNaN(curveCoordinate) || float.IsInfinity(curveCoordinate))
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must be finite.");

            if (curveCoordinate < 0f || curveCoordinate > length)
                throw new ArgumentOutOfRangeException(nameof(curveCoordinate), "Curve coordinate must lie within the Bezier curve length.");

            if (length == 0f)
                return pointAt(0f);

            PointXY previous = pointAt(0f);
            float previousCoordinate = 0f;

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                float currentParameter = i / (float)DefaultSegmentCount;
                PointXY current = pointAt(currentParameter);
                float segmentLength = previous.Distance(current);
                float currentCoordinate = previousCoordinate + segmentLength;

                if (curveCoordinate <= currentCoordinate || i == DefaultSegmentCount)
                {
                    if (segmentLength == 0f)
                        return current;

                    float segmentParameter = (curveCoordinate - previousCoordinate) / segmentLength;
                    float bezierParameter = (i - 1 + segmentParameter) / DefaultSegmentCount;
                    return pointAt(bezierParameter);
                }

                previous = current;
                previousCoordinate = currentCoordinate;
            }

            return pointAt(1f);
        }

        public static ParameterizedCurveProjection ProjectWithParameter(
            Func<float, PointXY> pointAt,
            PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Point coordinates must be finite.");

            PointXY previous = pointAt(0f);
            float previousCoordinate = 0f;
            var closestProjection = new ParameterizedCurveProjection(previous, 0f, point.Distance(previous));

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                var segment = new ParameterizedSegment(previous, current);
                ParameterizedCurveProjection projection = segment.ProjectWithParameter(point);
                var curveProjection = new ParameterizedCurveProjection(
                    projection.ProjectedPoint,
                    previousCoordinate + projection.CurveCoordinate,
                    projection.Distance);

                if (curveProjection.Distance < closestProjection.Distance)
                    closestProjection = curveProjection;

                previousCoordinate += segment.Length;
                previous = current;
            }

            return closestProjection;
        }

        public static ParameterizedCurveProjection ProjectWithParameter(
            PointXY[] approximationPoints,
            PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(point, nameof(point), "Point coordinates must be finite.");

            PointXY closestPoint = approximationPoints[0];
            float closestSquaredDistance = (point - closestPoint).SquaredLength;
            float closestCoordinate = 0f;
            float previousCoordinate = 0f;

            for (int i = 1; i < approximationPoints.Length; i++)
            {
                PointXY start = approximationPoints[i - 1];
                PointXY end = approximationPoints[i];
                VectorXY direction = end - start;
                float squaredLength = direction.SquaredLength;
                float parameter = squaredLength == 0f
                    ? 0f
                    : VectorXY.Dot(point - start, direction) / squaredLength;
                parameter = MathF.Max(0f, MathF.Min(1f, parameter));

                PointXY candidate = start + direction * parameter;
                float squaredDistance = (point - candidate).SquaredLength;
                float segmentLength = MathF.Sqrt(squaredLength);

                if (squaredDistance < closestSquaredDistance)
                {
                    closestPoint = candidate;
                    closestSquaredDistance = squaredDistance;
                    closestCoordinate = previousCoordinate + segmentLength * parameter;
                }

                previousCoordinate += segmentLength;
            }

            return new ParameterizedCurveProjection(
                closestPoint,
                closestCoordinate,
                MathF.Sqrt(closestSquaredDistance));
        }

        public static int CountRightwardCrossings(
            Func<float, PointXY> pointAt,
            PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            int count = 0;
            PointXY previous = pointAt(0f);

            for (int i = 1; i <= DefaultSegmentCount; i++)
            {
                PointXY current = pointAt(i / (float)DefaultSegmentCount);
                count += CountSegmentRightwardCrossings(previous, current, origin);
                previous = current;
            }

            return count;
        }

        public static int CountRightwardCrossings(
            PointXY startPoint,
            PointXY controlPoint,
            PointXY endPoint,
            PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            if (origin.X >= MathF.Max(startPoint.X, MathF.Max(controlPoint.X, endPoint.X)) ||
                origin.Y < MathF.Min(startPoint.Y, MathF.Min(controlPoint.Y, endPoint.Y)) ||
                origin.Y > MathF.Max(startPoint.Y, MathF.Max(controlPoint.Y, endPoint.Y)))
            {
                return 0;
            }

            double ay = startPoint.Y - 2.0 * controlPoint.Y + endPoint.Y;
            double by = 2.0 * (controlPoint.Y - startPoint.Y);
            Span<double> intervals = stackalloc double[3];
            int intervalCount = 2;
            intervals[0] = 0.0;
            intervals[1] = 1.0;

            if (ay != 0.0)
            {
                double extremum = -by / (2.0 * ay);
                if (extremum > 0.0 && extremum < 1.0)
                {
                    intervals[1] = extremum;
                    intervals[2] = 1.0;
                    intervalCount = 3;
                }
            }

            return CountPolynomialCrossings(
                intervals,
                intervalCount,
                0.0,
                ay,
                by,
                startPoint.Y,
                0.0,
                startPoint.X - 2.0 * controlPoint.X + endPoint.X,
                2.0 * (controlPoint.X - startPoint.X),
                startPoint.X,
                startPoint,
                endPoint,
                origin,
                by,
                2.0 * ay + by,
                ay,
                ay,
                0.0,
                0.0);
        }

        public static int CountRightwardCrossings(
            PointXY startPoint,
            PointXY controlPointA,
            PointXY controlPointB,
            PointXY endPoint,
            PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            float maxX = MathF.Max(MathF.Max(startPoint.X, controlPointA.X), MathF.Max(controlPointB.X, endPoint.X));
            float minY = MathF.Min(MathF.Min(startPoint.Y, controlPointA.Y), MathF.Min(controlPointB.Y, endPoint.Y));
            float maxY = MathF.Max(MathF.Max(startPoint.Y, controlPointA.Y), MathF.Max(controlPointB.Y, endPoint.Y));
            if (origin.X >= maxX || origin.Y < minY || origin.Y > maxY)
                return 0;

            double ay = -startPoint.Y + 3.0 * controlPointA.Y - 3.0 * controlPointB.Y + endPoint.Y;
            double by = 3.0 * startPoint.Y - 6.0 * controlPointA.Y + 3.0 * controlPointB.Y;
            double cy = 3.0 * (controlPointA.Y - startPoint.Y);
            Span<double> intervals = stackalloc double[4];
            intervals[0] = 0.0;
            int intervalCount = 1;

            double derivativeDiscriminant = 4.0 * by * by - 12.0 * ay * cy;
            if (ay != 0.0 && derivativeDiscriminant > 0.0)
            {
                double sqrt = System.Math.Sqrt(derivativeDiscriminant);
                AddIntervalBoundary(intervals, ref intervalCount, (-2.0 * by - sqrt) / (6.0 * ay));
                AddIntervalBoundary(intervals, ref intervalCount, (-2.0 * by + sqrt) / (6.0 * ay));
            }
            else if (ay == 0.0 && by != 0.0)
            {
                AddIntervalBoundary(intervals, ref intervalCount, -cy / (2.0 * by));
            }

            SortIntervalBoundaries(intervals, intervalCount);
            intervals[intervalCount++] = 1.0;

            return CountPolynomialCrossings(
                intervals,
                intervalCount,
                ay,
                by,
                cy,
                startPoint.Y,
                -startPoint.X + 3.0 * controlPointA.X - 3.0 * controlPointB.X + endPoint.X,
                3.0 * startPoint.X - 6.0 * controlPointA.X + 3.0 * controlPointB.X,
                3.0 * (controlPointA.X - startPoint.X),
                startPoint.X,
                startPoint,
                endPoint,
                origin,
                cy,
                3.0 * ay + 2.0 * by + cy,
                by,
                3.0 * ay + by,
                ay,
                -ay);
        }

        public static int CountRightwardCrossings(PointXY[] approximationPoints, PointXY origin)
        {
            PointXYValidation.ThrowIfNotFinite(origin, nameof(origin), "Ray origin coordinates must be finite.");

            int count = 0;
            for (int i = 1; i < approximationPoints.Length; i++)
                count += CountSegmentRightwardCrossings(approximationPoints[i - 1], approximationPoints[i], origin);

            return count;
        }

        private static int CountPolynomialCrossings(
            Span<double> intervals,
            int intervalCount,
            double ay,
            double by,
            double cy,
            double dy,
            double ax,
            double bx,
            double cx,
            double dx,
            PointXY startPoint,
            PointXY endPoint,
            PointXY origin,
            double startFirstDerivative,
            double endFirstDerivative,
            double startSecondCoefficient,
            double endSecondCoefficient,
            double startThirdCoefficient,
            double endThirdCoefficient)
        {
            int count = 0;

            if (startPoint.Y == origin.Y && startPoint.X > origin.X &&
                (startFirstDerivative > 0.0 ||
                    (startFirstDerivative == 0.0 &&
                        (startSecondCoefficient > 0.0 ||
                            (startSecondCoefficient == 0.0 && startThirdCoefficient > 0.0)))))
            {
                count++;
            }

            if (endPoint.Y == origin.Y && endPoint.X > origin.X &&
                (endFirstDerivative < 0.0 ||
                    (endFirstDerivative == 0.0 &&
                        (endSecondCoefficient > 0.0 ||
                            (endSecondCoefficient == 0.0 && endThirdCoefficient > 0.0)))))
            {
                count++;
            }

            for (int i = 1; i < intervalCount; i++)
            {
                double from = intervals[i - 1];
                double to = intervals[i];
                double fromY = EvaluatePolynomial(ay, by, cy, dy, from);
                double toY = EvaluatePolynomial(ay, by, cy, dy, to);
                double scanlineY = origin.Y;

                if (scanlineY <= System.Math.Min(fromY, toY) || scanlineY >= System.Math.Max(fromY, toY))
                    continue;

                bool increasing = toY > fromY;
                for (int iteration = 0; iteration < 24; iteration++)
                {
                    double middle = (from + to) * 0.5;
                    double middleY = EvaluatePolynomial(ay, by, cy, dy, middle);
                    if ((middleY < scanlineY) == increasing)
                        from = middle;
                    else
                        to = middle;
                }

                if (EvaluatePolynomial(ax, bx, cx, dx, (from + to) * 0.5) > origin.X)
                    count++;
            }

            return count;
        }

        private static int CountSegmentRightwardCrossings(PointXY start, PointXY end, PointXY origin)
        {
            if ((start.Y <= origin.Y && origin.Y < end.Y) ||
                (end.Y <= origin.Y && origin.Y < start.Y))
            {
                float x = start.X + (origin.Y - start.Y) * (end.X - start.X) / (end.Y - start.Y);
                return x > origin.X ? 1 : 0;
            }

            return 0;
        }

        private static double EvaluatePolynomial(
            double cubic,
            double quadratic,
            double linear,
            double constant,
            double t) => ((cubic * t + quadratic) * t + linear) * t + constant;

        private static void AddIntervalBoundary(Span<double> intervals, ref int count, double boundary)
        {
            if (boundary > 0.0 && boundary < 1.0)
                intervals[count++] = boundary;
        }

        private static void SortIntervalBoundaries(Span<double> intervals, int count)
        {
            if (count == 3 && intervals[1] > intervals[2])
            {
                double temporary = intervals[1];
                intervals[1] = intervals[2];
                intervals[2] = temporary;
            }
        }

        private static void ValidateSegmentCount(int segmentCount, string parameterName)
        {
            if (segmentCount <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Bezier flattening segment count must be positive.");
        }
    }
}
