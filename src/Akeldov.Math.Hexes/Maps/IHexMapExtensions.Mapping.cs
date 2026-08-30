using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class IHexMapExtensions
    {
        /// <summary>
        /// Maps each source value to a Boolean value while preserving the source topology.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each source value to a Boolean value.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<TSource, bool> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new BoolHexMap(map.Topology, CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each source value to an integer value while preserving the source topology.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each source value to an integer value.</param>
        /// <returns>A new mutable integer hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static IntHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<TSource, int> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new IntHexMap(map.Topology, CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each source value to a floating-point value while preserving the source topology.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each source value to a floating-point value.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static FloatHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<TSource, float> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new FloatHexMap(map.Topology, CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each value of the specified hex map while preserving the source topology.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <typeparam name="TResult">The result map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each source value to a result value.</param>
        /// <returns>A new mutable hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static HexMap<TResult> MapValues<TSource, TResult>(
            this IHexMap<TSource> map,
            Func<TSource, TResult> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new HexMap<TResult>(map.Topology, CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to a Boolean value.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to a Boolean value.</param>
        /// <returns>A new mutable Boolean hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, bool> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new BoolHexMap(map.Topology, CreateMappedPartialSextupletValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to an integer value.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to an integer value.</param>
        /// <returns>A new mutable integer hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static IntHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, int> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new IntHexMap(map.Topology, CreateMappedPartialSextupletValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to a floating-point value.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to a floating-point value.</param>
        /// <returns>A new mutable floating-point hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static FloatHexMap MapValues<TSource>(
            this IHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, float> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new FloatHexMap(map.Topology, CreateMappedPartialSextupletValues(map, selector));
        }

        /// <summary>
        /// Maps the six values adjacent to each cell to a new value while preserving the source topology.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <typeparam name="TResult">The result map value type.</typeparam>
        /// <param name="map">The source map.</param>
        /// <param name="selector">
        /// The function that maps the partial sextuplet adjacent to each source cell to a result value.
        /// </param>
        /// <returns>
        /// A new mutable hex map owned by the caller. For every result cell, the selector receives
        /// neighbors in <c>Adjacent0</c> through <c>Adjacent5</c> positions corresponding to
        /// <see cref="HexEdge.Edge0"/> through <see cref="HexEdge.Edge5"/>. Neighbors outside the map
        /// are absent from the partial sextuplet and have <see langword="default"/> values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        public static HexMap<TResult> MapValues<TSource, TResult>(
            this IHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, TResult> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new HexMap<TResult>(map.Topology, CreateMappedPartialSextupletValues(map, selector));
        }

        internal static TResult[] CreateMappedValues<TSource, TResult>(
            IHexMap<TSource> map,
            Func<TSource, TResult> selector)
        {
            var values = new TResult[map.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = selector(map[index]);

            return values;
        }

        internal static TResult[] CreateMappedPartialSextupletValues<TSource, TResult>(
            IHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, TResult> selector)
        {
            HexMapTopology topology = map.Topology;
            var values = new TResult[topology.Count];
            int flatIndex = 0;
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    values[flatIndex++] = selector(map.SamplePartialSextuplet(new VectorXYInt(x, y)));
                }
            }

            return values;
        }
    }
}
