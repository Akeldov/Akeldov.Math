using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class IHexMapExtensions
    {
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

            HexMapTopology topology = map.Topology;
            var values = new TResult[topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = selector(map[index]);

            return new HexMap<TResult>(topology, values);
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

            return new HexMap<TResult>(topology, values);
        }
    }
}
