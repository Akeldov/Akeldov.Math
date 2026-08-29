using System;

namespace Akeldov.Math.Hexes
{
    public partial class FloatHexMap
    {
        /// <summary>
        /// Creates a Boolean map identifying cells whose values are less than the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value used as the right operand for every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator <(FloatHexMap map, float value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] < value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is less than the source cell value.
        /// </summary>
        /// <param name="value">The value used as the left operand for every cell.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator <(float value, FloatHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value < map[index];

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are greater than the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value used as the right operand for every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator >(FloatHexMap map, float value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] > value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is greater than the source cell value.
        /// </summary>
        /// <param name="value">The value used as the left operand for every cell.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator >(float value, FloatHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value > map[index];

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are less than or equal to the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value used as the right operand for every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator <=(FloatHexMap map, float value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] <= value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is less than or equal to the source cell value.
        /// </summary>
        /// <param name="value">The value used as the left operand for every cell.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator <=(float value, FloatHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value <= map[index];

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are greater than or equal to the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value used as the right operand for every cell.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator >=(FloatHexMap map, float value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] >= value;

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is greater than or equal to the source cell value.
        /// </summary>
        /// <param name="value">The value used as the left operand for every cell.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap operator >=(float value, FloatHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value >= map[index];

            return new BoolHexMap(map.Topology, values);
        }
    }
}
