using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Creates an independent mutable copy of the specified Boolean hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap ToBoolHexMap(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates an independent mutable spatial copy of the specified Boolean hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="geometry">The spatial geometry to assign to the result.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology differs from <paramref name="geometry"/> topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public static SpatialBoolHexMap ToSpatialHexMap(this IHexMap<bool> map, HexMapGeometry geometry)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            if (map.Topology != geometry.Topology)
                throw new ArgumentException("Hex map topology must match the spatial geometry topology.", nameof(geometry));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new SpatialBoolHexMap(geometry, values);
        }

        /// <summary>
        /// Creates an independent mutable topology-only copy of the specified spatial Boolean hex map.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap ToHexMap(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new BoolHexMap(map.Topology, values);
        }
    }
}
