using Akeldov.Math.Hexes.Geometry;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides floating-point hex-map queries and conversions.
    /// </summary>
    public static class FloatHexMapExtensions
    {
        /// <summary>
        /// Computes the minimum and maximum values in a single pass over the map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>The minimum and maximum cell values.</returns>
        /// <remarks>NaN values propagate according to <see cref="MathF.Min(float, float)"/> and <see cref="MathF.Max(float, float)"/>.</remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the map contains no cells.</exception>
        public static (float Min, float Max) GetMinMax(this IHexMap<float> map)
        {
            if (!map.TryGetMinMax(out float min, out float max))
                throw new InvalidOperationException("Cannot get the minimum and maximum values of an empty map.");

            return (min, max);
        }

        /// <summary>
        /// Attempts to compute the minimum and maximum values in a single pass over the map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="min">The minimum cell value, or zero when the map is empty.</param>
        /// <param name="max">The maximum cell value, or zero when the map is empty.</param>
        /// <returns><see langword="true"/> when the map contains at least one cell; otherwise, <see langword="false"/>.</returns>
        /// <remarks>NaN values propagate according to <see cref="MathF.Min(float, float)"/> and <see cref="MathF.Max(float, float)"/>.</remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static bool TryGetMinMax(this IHexMap<float> map, out float min, out float max)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology.Count == 0)
            {
                min = default;
                max = default;
                return false;
            }

            min = map[0];
            max = map[0];
            for (int index = 1; index < map.Topology.Count; index++)
            {
                min = MathF.Min(min, map[index]);
                max = MathF.Max(max, map[index]);
            }

            return true;
        }

        /// <summary>
        /// Creates an independent mutable copy of the specified floating-point hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static FloatHexMap ToFloatHexMap(this IHexMap<float> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new float[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new FloatHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates an independent mutable spatial copy of the specified floating-point hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="geometry">The spatial geometry to assign to the result.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its values are copied from
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
        public static SpatialFloatHexMap ToSpatialHexMap(this IHexMap<float> map, HexMapGeometry geometry)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            if (map.Topology != geometry.Topology)
                throw new ArgumentException("Hex map topology must match the spatial geometry topology.", nameof(geometry));

            var values = new float[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new SpatialFloatHexMap(geometry, values);
        }

        /// <summary>
        /// Creates an independent mutable topology-only copy of the specified spatial floating-point hex map.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <returns>
        /// A new mutable floating-point hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static FloatHexMap ToHexMap(this ISpatialHexMap<float> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new float[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new FloatHexMap(map.Topology, values);
        }
    }
}
