using System;

namespace Akeldov.Math.Hexes
{
    public partial class FloatHexMap
    {
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
        public static FloatHexMap operator +(FloatHexMap left, IntHexMap right)
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
        public static FloatHexMap operator +(IntHexMap left, FloatHexMap right)
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
        public static FloatHexMap operator -(FloatHexMap left, IntHexMap right)
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
        public static FloatHexMap operator -(IntHexMap left, FloatHexMap right)
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

            return new FloatHexMap(left.Topology, values);
        }
    }
}
