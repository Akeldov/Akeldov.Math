using System;

namespace Akeldov.Math.Hexes
{
    public sealed partial class SpatialBoolHexMap
    {
        /// <summary>Creates a spatial Boolean map identifying equal cells in two source maps.</summary>
        /// <param name="left">The first spatial Boolean source map.</param>
        /// <param name="right">The second spatial Boolean source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator ==(SpatialBoolHexMap left, SpatialBoolHexMap right)
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

        /// <summary>Creates a spatial Boolean map identifying different cells in two source maps.</summary>
        /// <param name="left">The first spatial Boolean source map.</param>
        /// <param name="right">The second spatial Boolean source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. Neither source map is modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either source map is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the source maps do not have the same geometry.</exception>
        public static SpatialBoolHexMap operator !=(SpatialBoolHexMap left, SpatialBoolHexMap right)
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

        /// <summary>Creates a spatial Boolean map identifying cells equal to the specified value.</summary>
        /// <param name="map">The spatial Boolean source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator ==(SpatialBoolHexMap map, bool value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] == value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying cells equal to the specified value.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The spatial Boolean source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator ==(bool value, SpatialBoolHexMap map) => map == value;

        /// <summary>Creates a spatial Boolean map identifying cells different from the specified value.</summary>
        /// <param name="map">The spatial Boolean source map.</param>
        /// <param name="value">The value compared with every cell.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator !=(SpatialBoolHexMap map, bool value)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index] != value;

            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>Creates a spatial Boolean map identifying cells different from the specified value.</summary>
        /// <param name="value">The value compared with every cell.</param>
        /// <param name="map">The spatial Boolean source map.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller. The source map is not modified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is <see langword="null"/>.</exception>
        public static SpatialBoolHexMap operator !=(bool value, SpatialBoolHexMap map) => map != value;
    }
}
