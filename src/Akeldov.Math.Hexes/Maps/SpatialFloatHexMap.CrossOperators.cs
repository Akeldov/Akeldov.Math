using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialFloatHexMap
    {
        /// <summary>
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map.</param>
        /// <param name="right">The non-spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map.</param>
        /// <param name="right">The spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map.</param>
        /// <param name="right">The non-spatial integer source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the sums of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial integer source map.</param>
        /// <param name="right">The spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator +(IntHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the minuends.</param>
        /// <param name="right">The non-spatial floating-point source map whose cell values are the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map whose cell values are the minuends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the minuends.</param>
        /// <param name="right">The non-spatial integer source map whose cell values are the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the differences between the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial integer source map whose cell values are the minuends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the subtrahends.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator -(IntHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map.</param>
        /// <param name="right">The non-spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map.</param>
        /// <param name="right">The spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map.</param>
        /// <param name="right">The non-spatial integer source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the products of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial integer source map.</param>
        /// <param name="right">The spatial floating-point source map.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator *(IntHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The non-spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The non-spatial integer source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the quotients of the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial integer source map whose cell values are the dividends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator /(IntHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the remainders of dividing the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The non-spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the remainders of dividing the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the remainders of dividing the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The spatial floating-point source map whose cell values are the dividends.</param>
        /// <param name="right">The non-spatial integer source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial floating-point map whose cells contain the remainders of dividing the corresponding cells in two source maps.
        /// </summary>
        /// <param name="left">The non-spatial integer source map whose cell values are the dividends.</param>
        /// <param name="right">The spatial floating-point source map whose cell values are the divisors.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialFloatHexMap operator %(IntHexMap left, SpatialFloatHexMap right)
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
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than the right value.
        /// </summary>
        /// <param name="left">The non-spatial integer source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <(IntHexMap left, SpatialFloatHexMap right)
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
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than the right value.
        /// </summary>
        /// <param name="left">The non-spatial integer source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >(IntHexMap left, SpatialFloatHexMap right)
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
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is less than or equal to the right value.
        /// </summary>
        /// <param name="left">The non-spatial integer source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator <=(IntHexMap left, SpatialFloatHexMap right)
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
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(SpatialFloatHexMap left, FloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The non-spatial floating-point source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(FloatHexMap left, SpatialFloatHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The spatial floating-point source map containing the left values.</param>
        /// <param name="right">The non-spatial integer source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(SpatialFloatHexMap left, IntHexMap right)
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
        /// Creates a spatial Boolean map identifying cells where the left value is greater than or equal to the right value.
        /// </summary>
        /// <param name="left">The non-spatial integer source map containing the left values.</param>
        /// <param name="right">The spatial floating-point source map containing the right values.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is copied from
        /// the spatial source map. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator >=(IntHexMap left, SpatialFloatHexMap right)
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
