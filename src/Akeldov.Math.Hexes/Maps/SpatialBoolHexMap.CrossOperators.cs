using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialBoolHexMap
    {
        /// <summary>
        /// Creates a spatial map whose cells contain the conjunction of corresponding cells in
        /// a spatial Boolean map and a topology-compatible Boolean map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The topology-only source map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator &(SpatialBoolHexMap left, BoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial map whose cells contain the conjunction of corresponding cells in
        /// a topology-compatible Boolean map and a spatial Boolean map.
        /// </summary>
        /// <param name="left">The topology-only source map.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator &(BoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial map whose cells contain the disjunction of corresponding cells in
        /// a spatial Boolean map and a topology-compatible Boolean map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The topology-only source map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator |(SpatialBoolHexMap left, BoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial map whose cells contain the disjunction of corresponding cells in
        /// a topology-compatible Boolean map and a spatial Boolean map.
        /// </summary>
        /// <param name="left">The topology-only source map.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator |(BoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial map whose cells contain the exclusive disjunction of corresponding
        /// cells in a spatial Boolean map and a topology-compatible Boolean map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The topology-only source map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator ^(SpatialBoolHexMap left, BoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial map whose cells contain the exclusive disjunction of corresponding
        /// cells in a topology-compatible Boolean map and a spatial Boolean map.
        /// </summary>
        /// <param name="left">The topology-only source map.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology.
        /// </exception>
        public static SpatialBoolHexMap operator ^(BoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolHexMap(right.Geometry, values);
        }
    }
}
