using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides Boolean operations for hex maps.
    /// </summary>
    public static class BooleanHexMapExtensions
    {
        /// <summary>
        /// Creates a hex map whose cells contain the conjunction of the corresponding cells in
        /// two source maps.
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
        public static HexMap<bool> And(
            this IHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            return new HexMap<bool>(left.Topology, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of the corresponding
        /// cells in two spatial source maps.
        /// </summary>
        /// <param name="left">The first spatial source map.</param>
        /// <param name="right">The second spatial source map.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this ISpatialHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of corresponding cells
        /// in a spatial source map and a topology-compatible source map.
        /// </summary>
        /// <param name="left">The spatial source map whose geometry is retained by the result.</param>
        /// <param name="right">The source map to combine with <paramref name="left"/>.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="left"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="right"/> is spatial and its geometry differs from <paramref name="left"/>.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this ISpatialHexMap<bool> left,
            IHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (right is ISpatialHexMap<bool> spatialRight && left.Geometry != spatialRight.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(left.Geometry, CreateConjunctionValues(left, right));
        }

        /// <summary>
        /// Creates a spatial hex map whose cells contain the conjunction of corresponding cells
        /// in a topology-compatible source map and a spatial source map.
        /// </summary>
        /// <param name="left">The source map to combine with <paramref name="right"/>.</param>
        /// <param name="right">The spatial source map whose geometry is retained by the result.</param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. Its geometry is taken from
        /// <paramref name="right"/>. Neither source map is modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source maps do not have the same topology, or when
        /// <paramref name="left"/> is spatial and its geometry differs from <paramref name="right"/>.
        /// </exception>
        public static SpatialHexMap<bool> And(
            this IHexMap<bool> left,
            ISpatialHexMap<bool> right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Topology != right.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(right));

            if (left is ISpatialHexMap<bool> spatialLeft && spatialLeft.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            return new SpatialHexMap<bool>(right.Geometry, CreateConjunctionValues(left, right));
        }

        private static bool[] CreateConjunctionValues(IHexMap<bool> left, IHexMap<bool> right)
        {
            var values = new bool[left.Topology.Count];
            for (int i = 0; i < values.Length; i++)
                values[i] = left[i] & right[i];

            return values;
        }
    }
}
