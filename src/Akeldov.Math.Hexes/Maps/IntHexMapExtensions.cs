using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Fields;
using System;
using System.Collections.Generic;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides integer hex-map queries, transformations, and conversions.
    /// </summary>
    public static partial class IntHexMapExtensions
    {
        /// <summary>
        /// Computes the minimum and maximum values in a single pass over the map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>The minimum and maximum cell values.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the map contains no cells.</exception>
        public static (int Min, int Max) GetMinMax(this IHexMap<int> map)
        {
            if (!map.TryGetMinMax(out int min, out int max))
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static bool TryGetMinMax(this IHexMap<int> map, out int min, out int max)
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
                min = System.Math.Min(min, map[index]);
                max = System.Math.Max(max, map[index]);
            }

            return true;
        }

        /// <summary>
        /// Creates a spatial integer hex map by sampling a spatial field at the center of every hex
        /// in the specified geometry.
        /// </summary>
        /// <param name="field">The spatial integer field to sample.</param>
        /// <param name="geometry">The hex geometry that defines the sampled centers.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its values are sampled from
        /// <paramref name="field"/> at the hex centers defined by <paramref name="geometry"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="field"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public static SpatialIntHexMap ToSpatialHexMap(this IIntField field, HexMapGeometry geometry)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            return field.ToSpatialHexMap(new HexCenterMap(geometry));
        }

        /// <summary>
        /// Creates a spatial integer hex map by sampling a spatial field at each precomputed hex center.
        /// </summary>
        /// <param name="field">The spatial integer field to sample.</param>
        /// <param name="hexCenters">The precomputed hex centers that define the sampled geometry.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its values are sampled from
        /// <paramref name="field"/> at the points stored in <paramref name="hexCenters"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="field"/> or <paramref name="hexCenters"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialIntHexMap ToSpatialHexMap(this IIntField field, HexCenterMap hexCenters)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            var values = new int[hexCenters.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = field.Sample(hexCenters[index]);

            return new SpatialIntHexMap(hexCenters.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial integer hex map by sampling pointwise bounds and drawing a random value
        /// between them at the center of every hex in the specified geometry.
        /// </summary>
        /// <param name="range">The spatial fields that provide the pointwise minimum and maximum bounds.</param>
        /// <param name="geometry">The hex geometry that defines the sampled centers.</param>
        /// <param name="random">The random number generator used to draw each cell value.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its values are drawn from the
        /// pointwise ranges sampled at the hex centers defined by <paramref name="geometry"/>.
        /// </returns>
        /// <remarks>
        /// Hexes are processed in row-major order. Both sampled bounds are inclusive. Equal bounds
        /// produce that bound.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is the uninitialized default value.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="random"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a sampled minimum is greater than its maximum.
        /// </exception>
        public static SpatialIntHexMap ToSpatialHexMap(
            this IntFieldRange range,
            HexMapGeometry geometry,
            Random random)
        {
            if (range.MinField == null || range.MaxField == null)
                throw new ArgumentException("Integer field range must be initialized.", nameof(range));

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return range.ToSpatialHexMap(new HexCenterMap(geometry), random);
        }

        /// <summary>
        /// Creates a spatial integer hex map by sampling pointwise bounds and drawing a random value
        /// between them at each precomputed hex center.
        /// </summary>
        /// <param name="range">The spatial fields that provide the pointwise minimum and maximum bounds.</param>
        /// <param name="hexCenters">The precomputed hex centers that define the sampled geometry.</param>
        /// <param name="random">The random number generator used to draw each cell value.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its values are drawn from the
        /// pointwise ranges sampled at the points stored in <paramref name="hexCenters"/>.
        /// </returns>
        /// <remarks>
        /// Hexes are processed in row-major order. Both sampled bounds are inclusive. Equal bounds
        /// produce that bound.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is the uninitialized default value.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexCenters"/> or <paramref name="random"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a sampled minimum is greater than its maximum.
        /// </exception>
        public static SpatialIntHexMap ToSpatialHexMap(
            this IntFieldRange range,
            HexCenterMap hexCenters,
            Random random)
        {
            if (range.MinField == null || range.MaxField == null)
                throw new ArgumentException("Integer field range must be initialized.", nameof(range));

            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var values = new int[hexCenters.Topology.Count];
            var fullRangeBuffer = new byte[sizeof(int)];
            for (int index = 0; index < values.Length; index++)
            {
                int min = range.MinField.Sample(hexCenters[index]);
                int max = range.MaxField.Sample(hexCenters[index]);

                if (min > max)
                {
                    throw new InvalidOperationException(
                        $"Integer field range returned invalid bounds at hex index {index}. " +
                        "Minimum must be less than or equal to maximum.");
                }

                values[index] = NextInclusive(random, min, max, fullRangeBuffer);
            }

            return new SpatialIntHexMap(hexCenters.Geometry, values);
        }

        /// <summary>
        /// Creates a Boolean mask identifying cells whose integer value is present in the specified value list.
        /// </summary>
        /// <param name="map">The source integer map.</param>
        /// <param name="values">The integer values that should be marked in the result mask.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// when the source cell value is present in <paramref name="values"/>; otherwise, it is
        /// <see langword="false"/>. The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap ToValueMask(this IIntHexMap map, IReadOnlyList<int> values)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return new BoolHexMap(map.Topology, CreateValueMaskValues(map, values));
        }

        /// <summary>
        /// Creates a spatial Boolean mask identifying cells whose integer value is present in the
        /// specified value list while preserving the source spatial geometry.
        /// </summary>
        /// <param name="map">The source spatial integer map.</param>
        /// <param name="values">The integer values that should be marked in the result mask.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. A result cell is
        /// <see langword="true"/> when the source cell value is present in <paramref name="values"/>;
        /// otherwise, it is <see langword="false"/>. The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap ToValueMask(this ISpatialIntHexMap map, IReadOnlyList<int> values)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialBoolHexMap(map.Geometry, CreateValueMaskValues(map, values));
        }

        /// <summary>
        /// Creates an independent mutable copy of the specified integer hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <returns>
        /// A new mutable hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static IntHexMap ToIntHexMap(this IHexMap<int> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new IntHexMap(map.Topology, values);
        }

        /// <summary>
        /// Creates an independent mutable spatial copy of the specified integer hex map.
        /// </summary>
        /// <param name="map">The source map.</param>
        /// <param name="geometry">The spatial geometry to assign to the result.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its values are copied from
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
        public static SpatialIntHexMap ToSpatialHexMap(this IHexMap<int> map, HexMapGeometry geometry)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry radius must be finite and positive.");

            if (map.Topology != geometry.Topology)
                throw new ArgumentException("Hex map topology must match the spatial geometry topology.", nameof(geometry));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new SpatialIntHexMap(geometry, values);
        }

        /// <summary>
        /// Creates an independent mutable topology-only copy of the specified spatial integer hex map.
        /// </summary>
        /// <param name="map">The source spatial map.</param>
        /// <returns>
        /// A new mutable integer hex map owned by the caller. Its values are copied from
        /// <paramref name="map"/>, and subsequent changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static IntHexMap ToHexMap(this ISpatialHexMap<int> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = map[index];

            return new IntHexMap(map.Topology, values);
        }

        private static int NextInclusive(Random random, int min, int max, byte[] fullRangeBuffer)
        {
            if (min == max)
                return min;

            if (max < int.MaxValue)
                return random.Next(min, max + 1);

            if (min > int.MinValue)
                return random.Next(min - 1, max) + 1;

            random.NextBytes(fullRangeBuffer);
            uint value =
                (uint)fullRangeBuffer[0] |
                (uint)fullRangeBuffer[1] << 8 |
                (uint)fullRangeBuffer[2] << 16 |
                (uint)fullRangeBuffer[3] << 24;

            return unchecked((int)value);
        }

        private static bool[] CreateValueMaskValues(IHexMap<int> map, IReadOnlyList<int> includedValues)
        {
            var values = new bool[map.Topology.Count];
            if (includedValues.Count == 0)
                return values;

            var includedValueSet = new HashSet<int>(includedValues);
            for (int index = 0; index < values.Length; index++)
                values[index] = includedValueSet.Contains(map[index]);

            return values;
        }
    }
}
