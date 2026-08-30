using Akeldov.Math.Hexes.Topology;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class ISpatialHexMapExtensions
    {
        /// <summary>
        /// Maps the six values adjacent to each spatial-map cell while preserving the source geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <typeparam name="TResult">The result map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">
        /// The function that maps the partial sextuplet adjacent to each source cell to a result value.
        /// </param>
        /// <returns>
        /// A new mutable spatial hex map owned by the caller. For every result cell, the selector receives
        /// neighbors in <c>Adjacent0</c> through <c>Adjacent5</c> positions corresponding to
        /// <see cref="HexEdge.Edge0"/> through <see cref="HexEdge.Edge5"/>. Neighbors outside the map
        /// are absent from the partial sextuplet and have <see langword="default"/> values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialHexMap<TResult> MapValues<TSource, TResult>(
            this ISpatialHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, TResult> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            HexMapTopology topology = map.Topology;
            var values = new TResult[topology.Count];
            int flatIndex = 0;
            for (int y = 0; y < topology.Resolution.Y; y++)
            {
                for (int x = 0; x < topology.Resolution.X; x++)
                {
                    values[flatIndex++] = selector(map.SamplePartialSextuplet(new(x, y)));
                }
            }

            return new SpatialHexMap<TResult>(map.Geometry, values);
        }
    }
}
