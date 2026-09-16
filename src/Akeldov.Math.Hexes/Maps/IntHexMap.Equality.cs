using System;

namespace Akeldov.Math.Hexes
{
    public partial class IntHexMap
    {
        /// <summary>Creates a Boolean map identifying equal cells in two integer maps.</summary>
        /// <param name="left">The first integer source map.</param>
        /// <param name="right">The second integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator ==(IntHexMap left, IntHexMap right)
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

        /// <summary>Creates a Boolean map identifying different cells in two integer maps.</summary>
        /// <param name="left">The first integer source map.</param>
        /// <param name="right">The second integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same topology.</exception>
        public static BoolHexMap operator !=(IntHexMap left, IntHexMap right)
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

        /// <summary>Creates a Boolean map identifying cells equal to the specified integer.</summary>
        /// <param name="map">The integer source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static BoolHexMap operator ==(IntHexMap map, int value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] == value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying cells equal to the specified integer.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static BoolHexMap operator ==(int value, IntHexMap map) => map == value;

        /// <summary>Creates a Boolean map identifying cells different from the specified integer.</summary>
        /// <param name="map">The integer source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static BoolHexMap operator !=(IntHexMap map, int value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] != value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>Creates a Boolean map identifying cells different from the specified integer.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The integer source map.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static BoolHexMap operator !=(int value, IntHexMap map) => map != value;
    }
}
