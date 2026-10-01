using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Fields;
using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides floating-point hex-map queries, transformations, and conversions.
    /// </summary>
    public static partial class FloatHexMapExtensions
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
        /// Creates a spatial floating-point hex map by sampling a spatial field at the center of every hex
        /// in the specified geometry.
        /// </summary>
        /// <param name="field">The spatial floating-point field to sample.</param>
        /// <param name="geometry">The hex geometry that defines the sampled centers.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its values are sampled from
        /// <paramref name="field"/> at the hex centers defined by <paramref name="geometry"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="field"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin contains a non-finite component or its radius is not finite and positive.
        /// </exception>
        public static SpatialFloatHexMap ToSpatialHexMap(this IFloatField field, HexMapGeometry geometry)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            return field.ToSpatialHexMap(new HexCenterMap(geometry));
        }

        /// <summary>
        /// Creates a spatial floating-point hex map by sampling a spatial field at each precomputed hex center.
        /// </summary>
        /// <param name="field">The spatial floating-point field to sample.</param>
        /// <param name="hexCenters">The precomputed hex centers that define the sampled geometry.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its values are sampled from
        /// <paramref name="field"/> at the points stored in <paramref name="hexCenters"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="field"/> or <paramref name="hexCenters"/> is <see langword="null"/>.
        /// </exception>
        public static SpatialFloatHexMap ToSpatialHexMap(this IFloatField field, HexCenterMap hexCenters)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            var values = new float[hexCenters.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = field.Sample(hexCenters[index]);

            return new SpatialFloatHexMap(hexCenters.Geometry, values);
        }

        /// <summary>
        /// Creates a spatial floating-point hex map by sampling pointwise bounds and drawing a random
        /// value between them at the center of every hex in the specified geometry.
        /// </summary>
        /// <param name="range">The spatial fields that provide the pointwise minimum and maximum bounds.</param>
        /// <param name="geometry">The hex geometry that defines the sampled centers.</param>
        /// <param name="random">The random number generator used to draw each cell value.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its values are drawn from
        /// the pointwise ranges sampled at the hex centers defined by <paramref name="geometry"/>.
        /// </returns>
        /// <remarks>
        /// Hexes are processed in row-major order. For each hex, the method linearly interpolates from
        /// the sampled minimum to the sampled maximum using <see cref="Random.NextDouble"/>.
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
        /// Thrown when a sampled bound is not finite or a sampled minimum is greater than its maximum.
        /// </exception>
        public static SpatialFloatHexMap ToSpatialHexMap(
            this FloatFieldRange range,
            HexMapGeometry geometry,
            Random random)
        {
            if (range.MinField == null || range.MaxField == null)
                throw new ArgumentException("Float field range must be initialized.", nameof(range));

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return range.ToSpatialHexMap(new HexCenterMap(geometry), random);
        }

        /// <summary>
        /// Creates a spatial floating-point hex map by sampling pointwise bounds and drawing a random
        /// value between them at each precomputed hex center.
        /// </summary>
        /// <param name="range">The spatial fields that provide the pointwise minimum and maximum bounds.</param>
        /// <param name="hexCenters">The precomputed hex centers that define the sampled geometry.</param>
        /// <param name="random">The random number generator used to draw each cell value.</param>
        /// <returns>
        /// A new mutable spatial floating-point hex map owned by the caller. Its values are drawn from
        /// the pointwise ranges sampled at the points stored in <paramref name="hexCenters"/>.
        /// </returns>
        /// <remarks>
        /// Hexes are processed in row-major order. For each hex, the method linearly interpolates from
        /// the sampled minimum to the sampled maximum using <see cref="Random.NextDouble"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is the uninitialized default value.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="hexCenters"/> or <paramref name="random"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a sampled bound is not finite or a sampled minimum is greater than its maximum.
        /// </exception>
        public static SpatialFloatHexMap ToSpatialHexMap(
            this FloatFieldRange range,
            HexCenterMap hexCenters,
            Random random)
        {
            if (range.MinField == null || range.MaxField == null)
                throw new ArgumentException("Float field range must be initialized.", nameof(range));

            if (hexCenters == null)
                throw new ArgumentNullException(nameof(hexCenters));

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var values = new float[hexCenters.Topology.Count];
            for (int index = 0; index < values.Length; index++)
            {
                float min = range.MinField.Sample(hexCenters[index]);
                float max = range.MaxField.Sample(hexCenters[index]);

                if (float.IsNaN(min) || float.IsInfinity(min) ||
                    float.IsNaN(max) || float.IsInfinity(max) || min > max)
                {
                    throw new InvalidOperationException(
                        $"Float field range returned invalid bounds at hex index {index}. " +
                        "Bounds must be finite and minimum must be less than or equal to maximum.");
                }

                values[index] = (float)((double)min + random.NextDouble() * ((double)max - min));
            }

            return new SpatialFloatHexMap(hexCenters.Geometry, values);
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
        /// Creates an independent mutable integer copy of the specified spatial floating-point hex map.
        /// </summary>
        /// <param name="map">The source spatial floating-point map.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. Its geometry is preserved,
        /// its values are converted to <see cref="int"/> by truncating toward zero, and subsequent
        /// changes to either map do not affect the other.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialIntHexMap ToSpatialIntHexMap(this ISpatialHexMap<float> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new int[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = (int)map[index];

            return new SpatialIntHexMap(map.Geometry, values);
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
