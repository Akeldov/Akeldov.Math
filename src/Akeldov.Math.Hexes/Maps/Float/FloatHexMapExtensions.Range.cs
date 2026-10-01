using System;

namespace Akeldov.Math.Hexes
{
    public static partial class FloatHexMapExtensions
    {
        /// <summary>
        /// Restricts every value in a floating-point hex map to the specified inclusive range.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>
        /// A new mutable floating-point hex map owned by the caller. Its topology is taken from
        /// <paramref name="map"/>. The source map is not modified.
        /// </returns>
        /// <remarks>
        /// Source values and bounds containing <see cref="float.NaN"/> follow the semantics of
        /// <see cref="MathF.Min(float, float)"/> and <see cref="MathF.Max(float, float)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="max"/> is less than <paramref name="min"/>.
        /// </exception>
        public static FloatHexMap Clamp(this IHexMap<float> map, float min, float max)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (max < min)
                throw new ArgumentException(
                    $"Cannot clamp map: min ({min}) must be less than or equal to max ({max}).",
                    nameof(max));

            return new FloatHexMap(map.Topology, CreateClampedValues(map, min, max));
        }

        /// <summary>
        /// Restricts every value in a spatial floating-point hex map to the specified inclusive range.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is taken from
        /// <paramref name="map"/>. The source map is not modified.
        /// </returns>
        /// <remarks>
        /// Source values and bounds containing <see cref="float.NaN"/> follow the semantics of
        /// <see cref="MathF.Min(float, float)"/> and <see cref="MathF.Max(float, float)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology, or when
        /// <paramref name="max"/> is less than <paramref name="min"/>.
        /// </exception>
        public static SpatialFloatHexMap Clamp(this ISpatialHexMap<float> map, float min, float max)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            if (max < min)
                throw new ArgumentException(
                    $"Cannot clamp map: min ({min}) must be less than or equal to max ({max}).",
                    nameof(max));

            return new SpatialFloatHexMap(map.Geometry, CreateClampedValues(map, min, max));
        }

        /// <summary>
        /// Linearly rescales a floating-point hex map from its current value range to a new range.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="newMin">The value assigned to the current finite minimum.</param>
        /// <param name="newMax">The value assigned to the current finite maximum.</param>
        /// <returns>
        /// A new mutable floating-point hex map owned by the caller. Its topology is taken from
        /// <paramref name="map"/>. The source map is not modified. An empty source produces an empty map.
        /// </returns>
        /// <remarks>
        /// A map with a constant finite value is filled with <paramref name="newMin"/>. Finite non-constant
        /// ranges are evaluated with double-precision intermediate values so subtraction does not overflow
        /// the floating-point source range; the original minimum and maximum are mapped exactly to the new
        /// bounds. Non-finite source values or bounds follow IEEE floating-point arithmetic.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="newMax"/> is less than <paramref name="newMin"/>.
        /// </exception>
        public static FloatHexMap Rescale(this IHexMap<float> map, float newMin, float newMax)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (newMax < newMin)
                throw new ArgumentException(
                    $"Cannot rescale map: newMin ({newMin}) must be less than or equal to newMax ({newMax}).",
                    nameof(newMax));

            return new FloatHexMap(map.Topology, CreateRescaledValues(map, newMin, newMax));
        }

        /// <summary>
        /// Linearly rescales a spatial floating-point hex map from its current value range to a new range.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <param name="newMin">The value assigned to the current finite minimum.</param>
        /// <param name="newMax">The value assigned to the current finite maximum.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its geometry is taken from
        /// <paramref name="map"/>. The source map is not modified. An empty source produces an empty map.
        /// </returns>
        /// <remarks>
        /// A map with a constant finite value is filled with <paramref name="newMin"/>. Finite non-constant
        /// ranges are evaluated with double-precision intermediate values so subtraction does not overflow
        /// the floating-point source range; the original minimum and maximum are mapped exactly to the new
        /// bounds. Non-finite source values or bounds follow IEEE floating-point arithmetic.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology, or when
        /// <paramref name="newMax"/> is less than <paramref name="newMin"/>.
        /// </exception>
        public static SpatialFloatHexMap Rescale(this ISpatialHexMap<float> map, float newMin, float newMax)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            if (newMax < newMin)
                throw new ArgumentException(
                    $"Cannot rescale map: newMin ({newMin}) must be less than or equal to newMax ({newMax}).",
                    nameof(newMax));

            return new SpatialFloatHexMap(map.Geometry, CreateRescaledValues(map, newMin, newMax));
        }

        private static float[] CreateClampedValues(IHexMap<float> map, float min, float max)
        {
            var values = new float[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = MathF.Min(MathF.Max(map[index], min), max);

            return values;
        }

        private static float[] CreateRescaledValues(IHexMap<float> map, float newMin, float newMax)
        {
            var values = new float[map.Topology.Count];
            if (!map.TryGetMinMax(out float sourceMin, out float sourceMax))
                return values;

            bool hasFiniteSourceRange =
                !float.IsNaN(sourceMin) && !float.IsInfinity(sourceMin) &&
                !float.IsNaN(sourceMax) && !float.IsInfinity(sourceMax);
            bool hasFiniteNewRange =
                !float.IsNaN(newMin) && !float.IsInfinity(newMin) &&
                !float.IsNaN(newMax) && !float.IsInfinity(newMax);

            if (hasFiniteSourceRange && sourceMin == sourceMax)
            {
                for (int index = 0; index < values.Length; index++)
                    values[index] = newMin;

                return values;
            }

            double sourceRange = (double)sourceMax - sourceMin;
            double newRange = (double)newMax - newMin;

            for (int index = 0; index < values.Length; index++)
            {
                float value = map[index];

                if (hasFiniteSourceRange && hasFiniteNewRange && value == sourceMin)
                {
                    values[index] = newMin;
                }
                else if (hasFiniteSourceRange && hasFiniteNewRange && value == sourceMax)
                {
                    values[index] = newMax;
                }
                else
                {
                    double position = ((double)value - sourceMin) / sourceRange;
                    values[index] = (float)((double)newMin + position * newRange);
                }
            }

            return values;
        }
    }
}
