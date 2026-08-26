using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Stores one mutable integer value for every cell in a rectangular hex-map topology.
    /// </summary>
    public class IntHexMap : HexMap<int>, IIntHexMap
    {
        /// <summary>
        /// Initializes an empty map whose cells contain zero.
        /// </summary>
        /// <param name="topology">The layout and resolution of the map.</param>
        public IntHexMap(HexMapTopology topology)
            : base(topology)
        {
        }

        /// <summary>
        /// Initializes a new hex map that uses the specified array as its backing storage.
        /// </summary>
        /// <param name="topology">The map topology.</param>
        /// <param name="values">
        /// The backing array. Its length must equal the number of cells in <paramref name="topology"/>.
        /// Values must be stored in row-major order: X advances first, and the value at coordinates
        /// <c>(x, y)</c> is stored at <c>y * topology.Resolution.X + x</c>.
        /// </param>
        /// <remarks>
        /// <b>Ownership warning:</b> the array is retained by the map and is not copied. The caller and
        /// the map share the same mutable storage, so changes made through either one are visible through
        /// the other. Do not reuse or modify the array independently when exclusive map ownership is required.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of <paramref name="values"/> does not match the topology cell count.
        /// </exception>
        public IntHexMap(HexMapTopology topology, int[] values)
            : base(topology, values)
        {
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when the map contains no cells.</exception>
        public int Min
        {
            get
            {
                if (Topology.Count == 0)
                    throw new InvalidOperationException("Cannot get the minimum value of an empty map.");

                int min = this[0];
                for (int index = 1; index < Topology.Count; index++)
                    min = System.Math.Min(min, this[index]);

                return min;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Thrown when the map contains no cells.</exception>
        public int Max
        {
            get
            {
                if (Topology.Count == 0)
                    throw new InvalidOperationException("Cannot get the maximum value of an empty map.");

                int max = this[0];
                for (int index = 1; index < Topology.Count; index++)
                    max = System.Math.Max(max, this[index]);

                return max;
            }
        }

        /// <summary>
        /// Creates a map whose cells contain the arithmetic negation of the corresponding cells in the source map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell contains <see cref="int.MinValue"/>, whose negation does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator -(IntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(-map[index]);

            return new IntHexMap(map.Topology, values);
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
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator +(IntHexMap left, IntHexMap right)
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

            return new IntHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map by adding the specified value to every cell in the source map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="value">The value to add to every cell.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator +(IntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] + value);

            return new IntHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a map by adding the specified value to every cell in the source map.
        /// </summary>
        /// <param name="value">The value to add to every cell.</param>
        /// <param name="map">The source map.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator +(int value, IntHexMap map) => map + value;

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
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator -(IntHexMap left, IntHexMap right)
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

            return new IntHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map by subtracting the specified value from every cell in the source map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="value">The value to subtract from every cell.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator -(IntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] - value);

            return new IntHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a map by subtracting every source cell value from the specified value.
        /// </summary>
        /// <param name="value">The value used as the minuend for every cell.</param>
        /// <param name="map">The source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator -(int value, IntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value - map[index]);

            return new IntHexMap(map.Topology, values);
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
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator *(IntHexMap left, IntHexMap right)
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

            return new IntHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map by multiplying every cell in the source map by the specified value.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="value">The value by which to multiply every cell.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator *(IntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] * value);

            return new IntHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a map by multiplying every cell in the source map by the specified value.
        /// </summary>
        /// <param name="value">The value by which to multiply every cell.</param>
        /// <param name="map">The source map.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static IntHexMap operator *(int value, IntHexMap map) => map * value;

        /// <summary>
        /// Creates a map whose cells contain the integer quotients of the corresponding cells in two source maps.
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
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell divides <see cref="int.MinValue"/> by <c>-1</c>.
        /// </exception>
        public static IntHexMap operator /(IntHexMap left, IntHexMap right)
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

            return new IntHexMap(left.Topology, values);
        }

        /// <summary>
        /// Creates a map by dividing every cell in the source map by the specified value.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="value">The value by which to divide every cell.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when <paramref name="value"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when dividing <see cref="int.MinValue"/> by <c>-1</c>.
        /// </exception>
        public static IntHexMap operator /(IntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] / value);

            return new IntHexMap(map.Topology, values);
        }
    }
}
