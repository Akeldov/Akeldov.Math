using Akeldov.Math.Hexes.Topology;
using System;

namespace Akeldov.Math.Hexes
{
    public static partial class ISpatialHexMapExtensions
    {
        /// <summary>
        /// Maps each source value to a Boolean value while preserving the source geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each source value to a Boolean value.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<TSource, bool> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialBoolHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each source value to an integer value while preserving the source geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each source value to an integer value.</param>
        /// <returns>A new mutable spatial integer hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialIntHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<TSource, int> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialIntHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each source value to a floating-point value while preserving the source geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each source value to a floating-point value.</param>
        /// <returns>A new mutable spatial floating-point hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialFloatHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<TSource, float> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialFloatHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each value of the specified spatial hex map while preserving the source geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <typeparam name="TResult">The result map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each source value to a result value.</param>
        /// <returns>A new mutable spatial hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialHexMap<TResult> MapValues<TSource, TResult>(
            this ISpatialHexMap<TSource> map,
            Func<TSource, TResult> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialHexMap<TResult>(
                map.Geometry,
                IHexMapExtensions.CreateMappedValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to a Boolean value while preserving geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to a Boolean value.</param>
        /// <returns>A new mutable spatial Boolean hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, bool> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialBoolHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedPartialSextupletValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to an integer value while preserving geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to an integer value.</param>
        /// <returns>A new mutable spatial integer hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialIntHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, int> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialIntHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedPartialSextupletValues(map, selector));
        }

        /// <summary>
        /// Maps each partial sextuplet adjacent to a source cell to a floating-point value while preserving geometry.
        /// </summary>
        /// <typeparam name="TSource">The source map value type.</typeparam>
        /// <param name="map">The source spatial map.</param>
        /// <param name="selector">The function that maps each partial adjacent sextuplet to a floating-point value.</param>
        /// <returns>A new mutable spatial floating-point hex map owned by the caller.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> or <paramref name="selector"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialFloatHexMap MapValues<TSource>(
            this ISpatialHexMap<TSource> map,
            Func<PartialSextuplet<TSource>, float> selector)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialFloatHexMap(
                map.Geometry,
                IHexMapExtensions.CreateMappedPartialSextupletValues(map, selector));
        }

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

            return new SpatialHexMap<TResult>(
                map.Geometry,
                IHexMapExtensions.CreateMappedPartialSextupletValues(map, selector));
        }
    }
}
