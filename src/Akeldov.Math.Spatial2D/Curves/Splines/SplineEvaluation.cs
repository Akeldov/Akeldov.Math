using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    // The public curves validate scalar arguments and retain copied inputs.
    internal static class SplineEvaluation
    {
        public static float[] CopyAndValidateKnots(IReadOnlyList<float> knots, int degree, int controlPointCount)
        {
            var copy = new float[knots.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                float knot = knots[i];
                if (float.IsNaN(knot) || float.IsInfinity(knot))
                    throw new ArgumentOutOfRangeException(nameof(knots), "Knots must be finite.");
                if (i > 0 && knot < copy[i - 1])
                    throw new ArgumentException("Knots must be nondecreasing.", nameof(knots));

                copy[i] = knot;
            }

            float start = copy[degree];
            float end = copy[controlPointCount];
            if (start >= end)
                throw new ArgumentException("The active knot domain must have positive width.", nameof(knots));

            int multiplicity = 1;
            for (int i = 1; i < copy.Length; i++)
            {
                multiplicity = copy[i] == copy[i - 1] ? multiplicity + 1 : 1;
                bool interior = copy[i] > start && copy[i] < end;
                if (multiplicity > (interior ? degree : degree + 1))
                    throw new ArgumentException("Knot multiplicity exceeds the continuous path limit.", nameof(knots));
            }

            return copy;
        }

        public static PointXY[] CreateApproximation(
            int degree, PointXY[] controlPoints, float[]? weights, float[] knots, int segmentsPerKnotSpan)
        {
            int spanCount = 0;
            for (int span = degree; span < controlPoints.Length; span++)
            {
                if (knots[span] < knots[span + 1])
                    spanCount++;
            }

            long pointCount = (long)spanCount * segmentsPerKnotSpan + 1;
            if (pointCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(segmentsPerKnotSpan), "The approximation contains too many points.");

            var points = new PointXY[(int)pointCount];
            int workLength = (degree + 1) * (weights is null ? 2 : 3);
            Span<double> work = degree < 32 ? stackalloc double[workLength] : new double[workLength];
            int index = 0;
            for (int span = degree; span < controlPoints.Length; span++)
            {
                double from = knots[span];
                double to = knots[span + 1];
                if (from == to)
                    continue;

                if (index == 0)
                    points[index++] = EvaluateSpan(degree, controlPoints, weights, knots, from, span, work);
                for (int step = 1; step <= segmentsPerKnotSpan; step++)
                {
                    double amount = step / (double)segmentsPerKnotSpan;
                    points[index++] = EvaluateSpan(degree, controlPoints, weights, knots, (1.0 - amount) * from + amount * to, span, work);
                }
            }

            return points;
        }

        public static PointXY Evaluate(int degree, PointXY[] controlPoints, float[]? weights, float[] knots, double knot)
        {
            int span = FindSpan(degree, controlPoints.Length, knots, knot);
            int workLength = (degree + 1) * (weights is null ? 2 : 3);
            Span<double> work = degree < 32 ? stackalloc double[workLength] : new double[workLength];
            return EvaluateSpan(degree, controlPoints, weights, knots, knot, span, work);
        }

        private static int FindSpan(int degree, int controlPointCount, float[] knots, double knot)
        {
            int low = degree;
            int high = controlPointCount;
            if (knot == knots[controlPointCount])
            {
                low = high - 1;
                while (knots[low] == knot)
                    low--;
            }
            else
            {
                while (high - low > 1)
                {
                    int middle = low + (high - low) / 2;
                    if (knots[middle] <= knot)
                        low = middle;
                    else
                        high = middle;
                }
            }

            return low;
        }

        private static PointXY EvaluateSpan(
            int degree, PointXY[] controlPoints, float[]? weights, float[] knots,
            double knot, int span, Span<double> work)
        {
            // Polynomial splines use XY; rational splines use homogeneous (w*x, w*y, w).
            // Both use the same de Boor recurrence, with double intermediates.
            int dimension = weights is null ? 2 : 3;
            for (int j = 0; j <= degree; j++)
            {
                int index = span - degree + j;
                int offset = j * dimension;
                PointXY point = controlPoints[index];
                if (weights is null)
                {
                    work[offset] = point.X;
                    work[offset + 1] = point.Y;
                }
                else
                {
                    double weight = weights[index];
                    work[offset] = point.X * weight;
                    work[offset + 1] = point.Y * weight;
                    work[offset + 2] = weight;
                }
            }

            for (int level = 1; level <= degree; level++)
            {
                for (int j = degree; j >= level; j--)
                {
                    int index = span - degree + j;
                    double from = knots[index];
                    double to = knots[index + degree - level + 1];
                    double amount = (knot - from) / (to - from);
                    int offset = j * dimension;
                    for (int coordinate = 0; coordinate < dimension; coordinate++)
                    {
                        int current = offset + coordinate;
                        work[current] = (1.0 - amount) * work[current - dimension] + amount * work[current];
                    }
                }
            }

            int resultOffset = degree * dimension;
            if (weights is null)
                return new PointXY((float)work[resultOffset], (float)work[resultOffset + 1]);

            double resultWeight = work[resultOffset + 2];
            return new PointXY((float)(work[resultOffset] / resultWeight), (float)(work[resultOffset + 1] / resultWeight));
        }
    }
}
