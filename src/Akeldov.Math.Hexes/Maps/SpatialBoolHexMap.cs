using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Stores one mutable Boolean value for every cell in a spatial hex map.
    /// </summary>
    public sealed partial class SpatialBoolHexMap : SpatialHexMap<bool>
    {
        /// <summary>
        /// Initializes an empty map whose cells contain <see langword="false"/>.
        /// </summary>
        /// <param name="geometry">The spatial geometry of the map.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public SpatialBoolHexMap(HexMapGeometry geometry)
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
        public SpatialBoolHexMap(HexMapGeometry geometry, bool[] values)
            : base(geometry, values)
        {
        }

        /// <summary>
        /// Creates a map whose cells contain the logical negation of the corresponding cells in the source map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator !(SpatialBoolHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = !map[index];

            return new SpatialBoolHexMap(map.Geometry, values);
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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap operator &(SpatialBoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] & right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap operator |(SpatialBoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] | right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
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
        /// Thrown when the source maps do not have the same geometry.
        /// </exception>
        public static SpatialBoolHexMap operator ^(SpatialBoolHexMap left, SpatialBoolHexMap right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] ^ right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }
    }
}
