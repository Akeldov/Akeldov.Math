using System;

#pragma warning disable CS0660, CS0661 // Equality operators return cell masks rather than object-equality values.

namespace Akeldov.Math.Hexes
{
    public partial class BoolHexMap
    {
        /// <summary>
        /// Creates a map whose cells contain the conjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator &(HexMap<bool> left, BoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the conjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator &(BoolHexMap left, HexMap<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the disjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator |(HexMap<bool> left, BoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the disjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator |(BoolHexMap left, HexMap<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the exclusive disjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator ^(HexMap<bool> left, BoolHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the exclusive disjunction of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The first source map.</param>
        /// <param name="right">The second source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static BoolHexMap operator ^(BoolHexMap left, HexMap<bool> right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new BoolHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are equal in two source maps.
        /// </summary>
        /// <param name="left">The first Boolean source map.</param>
        /// <param name="right">The second Boolean source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(HexMap<bool> left, BoolHexMap right)
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

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are equal in two source maps.
        /// </summary>
        /// <param name="left">The first Boolean source map.</param>
        /// <param name="right">The second Boolean source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(BoolHexMap left, HexMap<bool> right)
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

        /// <summary>
        /// Creates a Boolean map identifying cells whose values differ in two source maps.
        /// </summary>
        /// <param name="left">The first Boolean source map.</param>
        /// <param name="right">The second Boolean source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(HexMap<bool> left, BoolHexMap right)
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
        /// Creates a Boolean map identifying cells whose values differ in two source maps.
        /// </summary>
        /// <param name="left">The first Boolean source map.</param>
        /// <param name="right">The second Boolean source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(BoolHexMap left, HexMap<bool> right)
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
    }
}
