using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Isolates distinct real polynomial roots with computations performed above <see cref="float"/> precision.
    /// </summary>
    internal static class PolynomialRootIsolation
    {
        private const int RefinementIterationCount = 128;

        /// <summary>
        /// Returns the distinct real roots of a polynomial within the closed unit interval.
        /// </summary>
        /// <param name="coefficients">The polynomial coefficients in ascending power order.</param>
        /// <returns>A new mutable list of roots ordered from zero to one.</returns>
        public static List<double> FindRootsInUnitInterval(double[] coefficients)
        {
            double[] polynomial = Trim(coefficients);
            List<double> roots = new List<double>();

            if (polynomial.Length <= 1)
                return roots;

            if (Evaluate(polynomial, 0d) == 0d)
                roots.Add(0d);

            List<double[]> sturmSequence = BuildSturmSequence(polynomial);
            double interiorStart = double.Epsilon;
            double interiorEnd = BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(1d) - 1L);
            int interiorRootCount = CountRoots(sturmSequence, interiorStart, interiorEnd);

            IsolateRoots(polynomial, sturmSequence, interiorStart, interiorEnd, interiorRootCount, roots);

            if (Evaluate(polynomial, 1d) == 0d)
                roots.Add(1d);

            roots.Sort();
            for (int i = roots.Count - 1; i > 0; i--)
            {
                if (roots[i] == roots[i - 1])
                    roots.RemoveAt(i);
            }

            return roots;
        }

        /// <summary>
        /// Determines whether every coefficient is exactly zero.
        /// </summary>
        /// <param name="coefficients">The coefficients to inspect.</param>
        /// <returns><see langword="true"/> when the polynomial is identically zero; otherwise, <see langword="false"/>.</returns>
        public static bool IsZero(double[] coefficients)
        {
            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i] != 0d)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the distinct stationary coordinates of a polynomial within the closed unit interval.
        /// </summary>
        /// <param name="coefficients">The polynomial coefficients in ascending power order.</param>
        /// <returns>A new mutable list of stationary coordinates ordered from zero to one.</returns>
        public static List<double> FindStationaryCoordinatesInUnitInterval(double[] coefficients)
        {
            double[] polynomial = Trim(coefficients);
            return polynomial.Length <= 1
                ? new List<double>()
                : FindRootsInUnitInterval(Differentiate(polynomial));
        }

        /// <summary>
        /// Recursively isolates the requested number of roots inside an interval.
        /// </summary>
        private static void IsolateRoots(double[] polynomial, List<double[]> sturmSequence, double left, double right, int rootCount, List<double> roots)
        {
            if (rootCount <= 0 || left >= right)
                return;

            if (rootCount == 1)
            {
                roots.Add(RefineRoot(polynomial, sturmSequence, left, right));
                return;
            }

            double split = FindNonRootSplit(polynomial, left, right);
            if (split <= left || split >= right)
            {
                roots.Add((left + right) * 0.5d);
                return;
            }

            int leftRootCount = CountRoots(sturmSequence, left, split);
            int rightRootCount = CountRoots(sturmSequence, split, right);
            IsolateRoots(polynomial, sturmSequence, left, split, leftRootCount, roots);
            IsolateRoots(polynomial, sturmSequence, split, right, rightRootCount, roots);
        }

        /// <summary>
        /// Refines an interval known to contain one distinct root.
        /// </summary>
        private static double RefineRoot(double[] polynomial, List<double[]> sturmSequence, double left, double right)
        {
            for (int i = 0; i < RefinementIterationCount; i++)
            {
                double middle = left + (right - left) * 0.5d;
                if (middle <= left || middle >= right)
                    break;

                if (Evaluate(polynomial, middle) == 0d)
                    return middle;

                if (CountRoots(sturmSequence, left, middle) > 0)
                    right = middle;
                else
                    left = middle;
            }

            return left + (right - left) * 0.5d;
        }

        /// <summary>
        /// Finds an interior split that is not itself a polynomial root.
        /// </summary>
        private static double FindNonRootSplit(double[] polynomial, double left, double right)
        {
            double split = left + (right - left) * 0.5d;
            for (int i = 0; i < 16 && Evaluate(polynomial, split) == 0d; i++)
                split = left + (split - left) * 0.5d;

            return split;
        }

        /// <summary>
        /// Counts distinct real roots inside an interval using sign variations of a Sturm sequence.
        /// </summary>
        private static int CountRoots(List<double[]> sturmSequence, double left, double right) =>
            CountSignVariations(sturmSequence, left) - CountSignVariations(sturmSequence, right);

        /// <summary>
        /// Counts nonzero sign variations of a Sturm sequence at one coordinate.
        /// </summary>
        private static int CountSignVariations(List<double[]> sturmSequence, double coordinate)
        {
            int variations = 0;
            int previousSign = 0;

            for (int i = 0; i < sturmSequence.Count; i++)
            {
                double value = Evaluate(sturmSequence[i], coordinate);
                int sign = value > 0d ? 1 : value < 0d ? -1 : 0;
                if (sign == 0)
                    continue;

                if (previousSign != 0 && sign != previousSign)
                    variations++;

                previousSign = sign;
            }

            return variations;
        }

        /// <summary>
        /// Builds the normalized Sturm sequence of a polynomial.
        /// </summary>
        private static List<double[]> BuildSturmSequence(double[] polynomial)
        {
            List<double[]> sequence = new List<double[]> { Normalize(polynomial), Normalize(Differentiate(polynomial)) };

            while (sequence[sequence.Count - 1].Length > 1)
            {
                double[] remainder = GetRemainder(sequence[sequence.Count - 2], sequence[sequence.Count - 1]);
                for (int i = 0; i < remainder.Length; i++)
                    remainder[i] = -remainder[i];

                remainder = Normalize(remainder);
                if (remainder.Length == 1 && remainder[0] == 0d)
                    break;

                sequence.Add(remainder);
            }

            return sequence;
        }

        /// <summary>
        /// Returns the polynomial remainder after division.
        /// </summary>
        private static double[] GetRemainder(double[] dividend, double[] divisor)
        {
            double[] remainder = (double[])dividend.Clone();
            int divisorDegree = divisor.Length - 1;
            double divisorLeading = divisor[divisorDegree];

            for (int degree = remainder.Length - 1; degree >= divisorDegree; degree--)
            {
                double factor = remainder[degree] / divisorLeading;
                for (int i = 0; i <= divisorDegree; i++)
                    remainder[degree - divisorDegree + i] -= factor * divisor[i];
            }

            double[] result = new double[divisorDegree];
            Array.Copy(remainder, result, result.Length);
            return Trim(result);
        }

        /// <summary>
        /// Returns the derivative of a polynomial.
        /// </summary>
        private static double[] Differentiate(double[] polynomial)
        {
            double[] derivative = new double[polynomial.Length - 1];
            for (int i = 1; i < polynomial.Length; i++)
                derivative[i - 1] = i * polynomial[i];

            return Trim(derivative);
        }

        /// <summary>
        /// Scales a polynomial by a positive magnitude without changing its signs or roots.
        /// </summary>
        private static double[] Normalize(double[] polynomial)
        {
            polynomial = Trim(polynomial);
            double magnitude = 0d;

            for (int i = 0; i < polynomial.Length; i++)
                magnitude = System.Math.Max(magnitude, System.Math.Abs(polynomial[i]));

            if (magnitude == 0d)
                return new[] { 0d };

            double[] normalized = new double[polynomial.Length];
            for (int i = 0; i < polynomial.Length; i++)
                normalized[i] = polynomial[i] / magnitude;

            return normalized;
        }

        /// <summary>
        /// Removes exactly zero leading coefficients.
        /// </summary>
        private static double[] Trim(double[] polynomial)
        {
            int length = polynomial.Length;
            while (length > 1 && polynomial[length - 1] == 0d)
                length--;

            if (length == polynomial.Length)
                return polynomial;

            double[] trimmed = new double[length];
            Array.Copy(polynomial, trimmed, length);
            return trimmed;
        }

        /// <summary>
        /// Evaluates a polynomial using Horner's method.
        /// </summary>
        private static double Evaluate(double[] polynomial, double coordinate)
        {
            double value = polynomial[polynomial.Length - 1];
            for (int i = polynomial.Length - 2; i >= 0; i--)
                value = value * coordinate + polynomial[i];

            return value;
        }
    }
}
