using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialFloatHexMap
    {
        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding floating-point and
        /// integer cells in two source maps.
        /// </summary>
        /// <param name="left">The floating-point source map.</param>
        /// <param name="right">The integer source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialFloatHexMap operator +(SpatialFloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the sums of the corresponding integer and
        /// floating-point cells in two source maps.
        /// </summary>
        /// <param name="left">The integer source map.</param>
        /// <param name="right">The floating-point source map.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialFloatHexMap operator +(SpatialIntHexMap left, SpatialFloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] + right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the differences between the corresponding
        /// floating-point and integer cells in two source maps.
        /// </summary>
        /// <param name="left">The floating-point source map whose cell values are the minuends.</param>
        /// <param name="right">The integer source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialFloatHexMap operator -(SpatialFloatHexMap left, SpatialIntHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a map whose cells contain the differences between the corresponding integer
        /// and floating-point cells in two source maps.
        /// </summary>
        /// <param name="left">The integer source map whose cell values are the minuends.</param>
        /// <param name="right">The floating-point source map whose cell values are the subtrahends.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialFloatHexMap operator -(SpatialIntHexMap left, SpatialFloatHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new float[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] - right[index];

            return new SpatialFloatHexMap(left.Geometry, values);
        }
    }
}
