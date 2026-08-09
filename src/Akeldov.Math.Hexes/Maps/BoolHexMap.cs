using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Stores one mutable Boolean value for every cell in a rectangular hex-map topology.
    /// </summary>
    public class BoolHexMap : HexMap<bool>
    {
        /// <summary>
        /// Initializes an empty map whose cells contain <see langword="false"/>.
        /// </summary>
        /// <param name="topology">The layout and resolution of the map.</param>
        public BoolHexMap(HexMapTopology topology)
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
        public BoolHexMap(HexMapTopology topology, bool[] values)
            : base(topology, values)
        {
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
        public static BoolHexMap operator &(BoolHexMap left, BoolHexMap right)
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

            return new BoolHexMap(left.Topology, values);
        }
    }
}
