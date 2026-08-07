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
    }
}
