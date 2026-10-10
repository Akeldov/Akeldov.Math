using System;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Hexes
{
    public partial class FloatHexMap
    {
        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator +(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator +(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the differences of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the minuends.</param>
        /// <param name="right">The source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator -(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the differences of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the minuends.</param>
        /// <param name="right">The source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator -(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator *(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator *(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The floating-point source map.</param>
        /// <param name="right">The integer source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator *(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The integer source map.</param>
        /// <param name="right">The floating-point source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator *(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] * right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the dividends.</param>
        /// <param name="right">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator /(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the dividends.</param>
        /// <param name="right">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator /(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The integer source map whose cell values are the divisors.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator /(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The integer source map whose cell values are the dividends.</param>
        /// <param name="right">The floating-point source map whose cell values are the divisors.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator /(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] / right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the floating-point remainders of corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the dividends.</param>
        /// <param name="right">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator %(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the floating-point remainders of corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The source map whose cell values are the dividends.</param>
        /// <param name="right">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator %(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the remainders of corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The integer source map whose cell values are the divisors.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator %(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a floating-point map whose cells contain the remainders of corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The integer source map whose cell values are the dividends.</param>
        /// <param name="right">The floating-point source map whose cell values are the divisors.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator %(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] % right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <=(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <=(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <=(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator <=(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >=(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >=(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The floating-point source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >=(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The floating-point source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator >=(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying equal cells in two floating-point maps.</summary>
        /// <param name="left">The first floating-point source map.</param>
        /// <param name="right">The second floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying equal cells in two floating-point maps.</summary>
        /// <param name="left">The first floating-point source map.</param>
        /// <param name="right">The second floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying different cells in two floating-point maps.</summary>
        /// <param name="left">The first floating-point source map.</param>
        /// <param name="right">The second floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(HexMap<float> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying different cells in two floating-point maps.</summary>
        /// <param name="left">The first floating-point source map.</param>
        /// <param name="right">The second floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(FloatHexMap left, HexMap<float> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying equal cells in floating-point and integer maps.</summary>
        /// <param name="left">The floating-point source map.</param>
        /// <param name="right">The integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying equal cells in integer and floating-point maps.</summary>
        /// <param name="left">The integer source map.</param>
        /// <param name="right">The floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying different cells in floating-point and integer maps.</summary>
        /// <param name="left">The floating-point source map.</param>
        /// <param name="right">The integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying different cells in integer and floating-point maps.</summary>
        /// <param name="left">The integer source map.</param>
        /// <param name="right">The floating-point source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding floating-point and integer cells.
        /// </summary>
        /// <param name="left">The floating-point source map.</param>
        /// <param name="right">The integer source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator +(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding integer and floating-point cells.
        /// </summary>
        /// <param name="left">The integer source map.</param>
        /// <param name="right">The floating-point source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator +(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the integer cell values subtracted from the corresponding floating-point cells.
        /// </summary>
        /// <param name="left">The floating-point source map whose cell values are the minuends.</param>
        /// <param name="right">The integer source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator -(FloatHexMap left, HexMap<int> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new FloatHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the floating-point cell values subtracted from the corresponding integer cells.
        /// </summary>
        /// <param name="left">The integer source map whose cell values are the minuends.</param>
        /// <param name="right">The floating-point source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static FloatHexMap operator -(HexMap<int> left, FloatHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new FloatHexMap(left.Topology, values);
        }
    }
}
