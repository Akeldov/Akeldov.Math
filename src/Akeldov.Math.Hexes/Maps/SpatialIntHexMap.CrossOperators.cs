using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialIntHexMap
    {
        /// <summary>
        /// Creates a spatial integer map whose cells contain the sums of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left addends.</param>
        /// <param name="right">The topology-only integer source map containing the right addends.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator +(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the differences between the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the minuends.</param>
        /// <param name="right">The topology-only integer source map containing the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator -(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the products of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left factors.</param>
        /// <param name="right">The topology-only integer source map containing the right factors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator *(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] * right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the quotients of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the dividends.</param>
        /// <param name="right">The topology-only integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell divides <see cref="int.MinValue"/> by <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator /(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] / right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the remainders after division of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the dividends.</param>
        /// <param name="right">The topology-only integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell computes <see cref="int.MinValue"/> % <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator %(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] % right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(SpatialIntHexMap left, IntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the sums of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left addends.</param>
        /// <param name="right">The spatial integer source map containing the right addends.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator +(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the differences between the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the minuends.</param>
        /// <param name="right">The spatial integer source map containing the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator -(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the products of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left factors.</param>
        /// <param name="right">The spatial integer source map containing the right factors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator *(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] * right[index]);

            return new SpatialIntHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the quotients of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the dividends.</param>
        /// <param name="right">The spatial integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell divides <see cref="int.MinValue"/> by <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator /(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] / right[index]);

            return new SpatialIntHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer map whose cells contain the remainders after division of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the dividends.</param>
        /// <param name="right">The spatial integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell computes <see cref="int.MinValue"/> % <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator %(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] % right[index]);

            return new SpatialIntHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The topology-only integer source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(IntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left addends.</param>
        /// <param name="right">The topology-only floating-point source map containing the right addends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the minuends.</param>
        /// <param name="right">The topology-only floating-point source map containing the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left factors.</param>
        /// <param name="right">The topology-only floating-point source map containing the right factors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the dividends.</param>
        /// <param name="right">The topology-only floating-point source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the remainders after division of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the dividends.</param>
        /// <param name="right">The topology-only floating-point source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial integer source map containing the left values.</param>
        /// <param name="right">The topology-only floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(SpatialIntHexMap left, FloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left addends.</param>
        /// <param name="right">The spatial integer source map containing the right addends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new SpatialFloatHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the minuends.</param>
        /// <param name="right">The spatial integer source map containing the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new SpatialFloatHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left factors.</param>
        /// <param name="right">The spatial integer source map containing the right factors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new SpatialFloatHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the dividends.</param>
        /// <param name="right">The spatial integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new SpatialFloatHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the remainders after division of the corresponding cells
        /// in the two source maps.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the dividends.</param>
        /// <param name="right">The spatial integer source map containing the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point, and cell arithmetic follows
        /// IEEE 754 floating-point semantics.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new SpatialFloatHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The topology-only floating-point source map containing the left values.</param>
        /// <param name="right">The spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Neither source map is
        /// modified, and the geometry of the spatial operand is retained.
        /// </returns>
        /// <remarks>
        /// Integer cell values are converted to floating point. Comparisons involving
        /// <see cref="float.NaN"/> evaluate to <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(FloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }
    }
}
