using System;

namespace Akeldov.Math.Hexes
{
    public static partial class IntHexMapExtensions
    {
        /// <summary>
        /// Restricts every source cell value to the specified inclusive range.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="min">The inclusive minimum result value.</param>
        /// <param name="max">The inclusive maximum result value.</param>
        /// <returns>
        /// A new mutable integer hex map owned by the caller. The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="max"/> is less than <paramref name="min"/>.
        /// </exception>
        public static IntHexMap Clamp(this IHexMap<int> map, int min, int max)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (max < min)
                throw new ArgumentException("Maximum value must be greater than or equal to minimum value.", nameof(max));

            return new IntHexMap(map.Topology, CreateClampedValues(map, min, max));
        }

        /// <summary>
        /// Restricts every source cell value to the specified inclusive range while preserving
        /// the source spatial geometry.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="min">The inclusive minimum result value.</param>
        /// <param name="max">The inclusive maximum result value.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. The result preserves the
        /// source geometry, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="max"/> is less than <paramref name="min"/>, or when the
        /// source topology does not match its geometry topology.
        /// </exception>
        public static SpatialIntHexMap Clamp(this ISpatialHexMap<int> map, int min, int max)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            if (max < min)
                throw new ArgumentException("Maximum value must be greater than or equal to minimum value.", nameof(max));

            return new SpatialIntHexMap(map.Geometry, CreateClampedValues(map, min, max));
        }

        /// <summary>
        /// Linearly rescales the source value range to the specified inclusive range.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="newMin">The inclusive minimum result value.</param>
        /// <param name="newMax">The inclusive maximum result value.</param>
        /// <returns>
        /// A new mutable integer hex map owned by the caller. The source minimum maps to
        /// <paramref name="newMin"/>, the source maximum maps to <paramref name="newMax"/>, and
        /// intermediate values are rounded to the nearest integer using
        /// <see cref="MidpointRounding.ToEven"/>. An empty source produces an empty result. When
        /// all source values are equal, every result cell contains <paramref name="newMin"/>.
        /// The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="newMax"/> is less than <paramref name="newMin"/>.
        /// </exception>
        public static IntHexMap Rescale(this IHexMap<int> map, int newMin, int newMax)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (newMax < newMin)
                throw new ArgumentException("Maximum value must be greater than or equal to minimum value.", nameof(newMax));

            return new IntHexMap(map.Topology, CreateRescaledValues(map, newMin, newMax));
        }

        /// <summary>
        /// Linearly rescales the source value range to the specified inclusive range while
        /// preserving the source spatial geometry.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="newMin">The inclusive minimum result value.</param>
        /// <param name="newMax">The inclusive maximum result value.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. The result preserves the
        /// source geometry. The source minimum maps to <paramref name="newMin"/>, the source
        /// maximum maps to <paramref name="newMax"/>, and intermediate values are rounded to the
        /// nearest integer using <see cref="MidpointRounding.ToEven"/>. An empty source produces
        /// an empty result. When all source values are equal, every result cell contains
        /// <paramref name="newMin"/>. The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="newMax"/> is less than <paramref name="newMin"/>, or when
        /// the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialIntHexMap Rescale(this ISpatialHexMap<int> map, int newMin, int newMax)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            if (newMax < newMin)
                throw new ArgumentException("Maximum value must be greater than or equal to minimum value.", nameof(newMax));

            return new SpatialIntHexMap(map.Geometry, CreateRescaledValues(map, newMin, newMax));
        }

        private static int[] CreateClampedValues(IHexMap<int> map, int min, int max)
        {
            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
            {
                int value = map[index];
                values[index] = value < min ? min : value > max ? max : value;
            }

            return values;
        }

        private static int[] CreateRescaledValues(IHexMap<int> map, int newMin, int newMax)
        {
            var values = new int[map.Topology.Count];
            if (values.Length == 0)
                return values;

            int sourceMin = map[0];
            int sourceMax = sourceMin;
            values[0] = sourceMin;

            for (int index = 1; index < values.Length; index++)
            {
                int value = map[index];
                values[index] = value;
                sourceMin = System.Math.Min(sourceMin, value);
                sourceMax = System.Math.Max(sourceMax, value);
            }

            if (sourceMin == sourceMax)
            {
                for (int index = 0; index < values.Length; index++)
                    values[index] = newMin;

                return values;
            }

            decimal sourceSpan = (decimal)sourceMax - sourceMin;
            decimal resultSpan = (decimal)newMax - newMin;

            for (int index = 0; index < values.Length; index++)
            {
                int value = values[index];
                if (value == sourceMin)
                {
                    values[index] = newMin;
                    continue;
                }

                if (value == sourceMax)
                {
                    values[index] = newMax;
                    continue;
                }

                decimal rescaled = newMin + ((decimal)value - sourceMin) * resultSpan / sourceSpan;
                values[index] = decimal.ToInt32(decimal.Round(rescaled, 0, MidpointRounding.ToEven));
            }

            return values;
        }
    }
}
