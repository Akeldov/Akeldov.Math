using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides morphology operations for spatial Boolean hex maps.
    /// </summary>
    public static partial class ISpatialHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one dilation over each spatial cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. It retains the source geometry.
        /// A result cell is <see langword="true"/> when the source cell itself or at least one existing
        /// neighbor is <see langword="true"/>. Neighbors outside the map domain are ignored, and the
        /// source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Dilate(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            var values = new bool[map.Topology.Count];
            BooleanHexMapDilationHelper.FillDilatedValues(map, values);
            return new SpatialBoolHexMap(map.Geometry, values);
        }

        /// <summary>
        /// Expands the true spatial cells by the specified number of edge-adjacent hex rings.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <param name="ringsCount">The non-negative number of rings. Zero creates an independent copy.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller, retaining the source geometry.
        /// A result cell is <see langword="true"/> when its shortest six-neighbor path within the map
        /// to a source true cell has at most <paramref name="ringsCount"/> steps.
        /// The source map is not modified.
        /// </returns>
        /// <remarks>
        /// Expansion is clipped to the map domain. A bounded multi-source breadth-first traversal
        /// visits each reached cell at most once, taking O(N) time and O(N) space for N map cells.
        /// One ring uses the direct dilation pass without a traversal queue.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ringsCount"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Dilate(this ISpatialHexMap<bool> map, int ringsCount)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (ringsCount < 0)
                throw new ArgumentOutOfRangeException(nameof(ringsCount));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialBoolHexMap(map.Geometry, BooleanHexMapExpansionHelper.CreateExpandedValues(map, ringsCount, expandedValue: true));
        }

        /// <summary>
        /// Expands the true spatial cells along paths allowed by per-cell distance limits.
        /// </summary>
        /// <typeparam name="TIntMap">The type of the integer hex map supplying distance limits.</typeparam>
        /// <param name="map">The source spatial Boolean map. Its true cells are zero-distance starting points.</param>
        /// <param name="maxDilateDistanceMap">
        /// The maximum allowed arrival distance at each cell, measured in edge-adjacent hex steps
        /// from any original true cell. It must have the same topology as <paramref name="map"/>;
        /// values are matched by cell index, independently of any geometry on the limit map.
        /// Zero and negative values block entry into false cells.
        /// </param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller, retaining the source geometry
        /// and all original true cells regardless of their limits. Neither input map is modified.
        /// </returns>
        /// <remarks>
        /// Every entered cell on a path must allow its arrival distance. Rejected cells cannot
        /// transmit expansion; an alternative path may reach cells behind them if its full length
        /// satisfies the limits. Expansion is clipped to the map domain. A multi-source breadth-first
        /// traversal takes O(N) time and O(N) space for N map cells, using a pooled queue.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either input map is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the input maps do not have the same topology, or when the source Boolean
        /// map's topology does not match its geometry topology.
        /// </exception>
        public static SpatialBoolHexMap Dilate<TIntMap>(this ISpatialHexMap<bool> map, TIntMap maxDilateDistanceMap)
            where TIntMap : IHexMap<int>
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            if (maxDilateDistanceMap is null)
                throw new ArgumentNullException(nameof(maxDilateDistanceMap));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            if (map.Topology != maxDilateDistanceMap.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(maxDilateDistanceMap));

            return new SpatialBoolHexMap(map.Geometry, BooleanHexMapExpansionHelper.CreateConstrainedExpandedValues(map, maxDilateDistanceMap, expandedValue: true));
        }
    }
}
