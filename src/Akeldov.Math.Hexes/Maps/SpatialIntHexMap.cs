using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Stores one mutable integer value for every cell in a spatial hex map.
    /// </summary>
    public sealed partial class SpatialIntHexMap : SpatialHexMap<int>, ISpatialIntHexMap
    {
        /// <summary>
        /// Initializes an empty map whose cells contain zero.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialIntHexMap(HexMapGeometry geometry)
            : base(geometry)
        {
        }

        /// <summary>
        /// Initializes a new hex map that uses the specified array as its backing storage.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        /// <param name="values">
        /// The backing array. Its length must equal the number of cells in <paramref name="geometry"/>.
        /// Values must be stored in row-major order: X advances first, and the value at coordinates
        /// <c>(x, y)</c> is stored at <c>y * geometry.Topology.Resolution.X + x</c>.
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
        /// Thrown when the length of <paramref name="values"/> does not match the geometry topology cell count.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialIntHexMap(HexMapGeometry geometry, int[] values)
            : base(geometry, values)
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
        public static SpatialIntHexMap operator -(SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(-map[index]);

            return new SpatialIntHexMap(map.Geometry, values);
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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell sum does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator +(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] + right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
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
        public static SpatialIntHexMap operator +(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] + value);

            return new SpatialIntHexMap(map.Geometry, values);
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
        public static SpatialIntHexMap operator +(int value, SpatialIntHexMap map) => map + value;

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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell difference does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator -(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] - right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
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
        public static SpatialIntHexMap operator -(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] - value);

            return new SpatialIntHexMap(map.Geometry, values);
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
        public static SpatialIntHexMap operator -(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value - map[index]);

            return new SpatialIntHexMap(map.Geometry, values);
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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell product does not fit in <see cref="int"/>.
        /// </exception>
        public static SpatialIntHexMap operator *(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] * right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
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
        public static SpatialIntHexMap operator *(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] * value);

            return new SpatialIntHexMap(map.Geometry, values);
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
        public static SpatialIntHexMap operator *(int value, SpatialIntHexMap map) => map * value;

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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell divides <see cref="int.MinValue"/> by <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator /(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] / right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
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
        public static SpatialIntHexMap operator /(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] / value);

            return new SpatialIntHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a map by dividing the specified value by every cell in the source map.
        /// </summary>
        /// <param name="value">The value used as the dividend for every cell.</param>
        /// <param name="map">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="map"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when <paramref name="value"/> is <see cref="int.MinValue"/> and a cell contains <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator /(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value / map[index]);

            return new SpatialIntHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the integer remainders after division by the corresponding cells in another map.
        /// </summary>
        /// <param name="left">The source map whose cell values are the dividends.</param>
        /// <param name="right">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="right"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell computes <see cref="int.MinValue"/> % <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator %(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new int[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(left[index] % right[index]);

            return new SpatialIntHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the integer remainders after division by the specified value.
        /// </summary>
        /// <param name="map">The source map whose cell values are the dividends.</param>
        /// <param name="value">The value used as the divisor for every cell.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when <paramref name="value"/> is zero and the map contains a cell.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when a cell contains <see cref="int.MinValue"/> and <paramref name="value"/> is <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator %(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(map[index] % value);

            return new SpatialIntHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a map by taking the remainder of the specified value divided by every cell in the source map.
        /// </summary>
        /// <param name="value">The value used as the dividend for every cell.</param>
        /// <param name="map">The source map whose cell values are the divisors.</param>
        /// <returns>A new mutable hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when a cell in <paramref name="map"/> is zero.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when <paramref name="value"/> is <see cref="int.MinValue"/> and a cell contains <c>-1</c>.
        /// </exception>
        public static SpatialIntHexMap operator %(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = checked(value % map[index]);

            return new SpatialIntHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator <(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] < right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator >(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] > right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator <=(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] <= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The integer source map containing the left values.</param>
        /// <param name="right">The integer source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator >=(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] >= right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }
    }
}
