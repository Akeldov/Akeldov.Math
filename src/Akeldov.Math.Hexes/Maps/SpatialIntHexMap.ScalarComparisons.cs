using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialIntHexMap
    {
        /// <summary>
        /// Creates a Boolean map identifying cells whose values are less than the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value on the right side of the comparison.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator <(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] < value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are greater than the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value on the right side of the comparison.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator >(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] > value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are less than or equal to the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value on the right side of the comparison.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator <=(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] <= value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells whose values are greater than or equal to the specified value.
        /// </summary>
        /// <param name="map">The source map containing the left values.</param>
        /// <param name="value">The value on the right side of the comparison.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator >=(SpatialIntHexMap map, int value)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] >= value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is less than the cell value.
        /// </summary>
        /// <param name="value">The value on the left side of the comparison.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator <(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value < map[index];

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is greater than the cell value.
        /// </summary>
        /// <param name="value">The value on the left side of the comparison.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator >(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value > map[index];

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is less than or equal to the cell value.
        /// </summary>
        /// <param name="value">The value on the left side of the comparison.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator <=(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value <= map[index];

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean map identifying cells where the specified value is greater than or equal to the cell value.
        /// </summary>
        /// <param name="value">The value on the left side of the comparison.</param>
        /// <param name="map">The source map containing the right values.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialBoolHexMap operator >=(int value, SpatialIntHexMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = value >= map[index];

            return new SpatialBoolHexMap(map.Geometry, values);
        }
    }
}
