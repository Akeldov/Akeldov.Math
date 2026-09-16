using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialIntHexMap
    {
        /// <summary>Creates a spatial Boolean map identifying equal cells in two integer maps.</summary>
        /// <param name="left">The first spatial integer source map.</param>
        /// <param name="right">The second spatial integer source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator ==(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] == right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying different cells in two integer maps.</summary>
        /// <param name="left">The first spatial integer source map.</param>
        /// <param name="right">The second spatial integer source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator !=(SpatialIntHexMap left, SpatialIntHexMap right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            if (left.Geometry != right.Geometry)
                throw new ArgumentException("Spatial hex maps must have the same geometry.", nameof(right));

            var values = new bool[left.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = left[index] != right[index];

            return new SpatialBoolHexMap(left.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying cells equal to the specified integer.</summary>
        /// <param name="map">The spatial integer source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator ==(SpatialIntHexMap map, int value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] == value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying cells equal to the specified integer.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The spatial integer source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator ==(int value, SpatialIntHexMap map) => map == value;

        /// <summary>Creates a spatial Boolean map identifying cells different from the specified integer.</summary>
        /// <param name="map">The spatial integer source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator !=(SpatialIntHexMap map, int value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] != value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying cells different from the specified integer.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The spatial integer source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator !=(int value, SpatialIntHexMap map) => map != value;
    }
}
