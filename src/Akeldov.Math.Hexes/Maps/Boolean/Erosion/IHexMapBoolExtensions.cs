using System;

namespace Akeldov.Math.Hexes
{
    public static partial class IHexMapBoolExtensions
    {
        /// <summary>
        /// Applies radius-one erosion over each cell and its six existing edge-adjacent neighbors.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. A result cell is <see langword="true"/>
        /// only when the source cell itself and every existing neighbor are <see langword="true"/>.
        /// Neighbors outside the map domain are ignored, and the source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static BoolHexMap Erode(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var values = new bool[map.Topology.Count];
            BooleanHexMapErosionHelper.FillErodedValues(map, values);
            return new BoolHexMap(map.Topology, values);
        }

        /// <summary>
        /// Expands the false cells by the specified number of edge-adjacent hex rings.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <param name="ringsCount">The non-negative number of rings. Zero creates an independent copy.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller, with the source topology. A result cell
        /// is <see langword="false"/> when its shortest six-neighbor path within the map to a source
        /// false cell has at most <paramref name="ringsCount"/> steps. The source map is not modified.
        /// </returns>
        /// <remarks>
        /// Expansion is clipped to the map domain. A bounded multi-source breadth-first traversal
        /// visits each reached cell at most once, taking O(N) time and O(N) space for N map cells.
        /// One ring uses the direct erosion pass without a traversal queue.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ringsCount"/> is negative.
        /// </exception>
        public static BoolHexMap Erode(this IHexMap<bool> map, int ringsCount)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (ringsCount < 0)
                throw new ArgumentOutOfRangeException(nameof(ringsCount));

            return new BoolHexMap(map.Topology, BooleanHexMapExpansionHelper.CreateExpandedValues(map, ringsCount, expandedValue: false));
        }

        /// <summary>
        /// Expands the false cells along paths allowed by per-cell distance limits.
        /// </summary>
        /// <typeparam name="TIntMap">The type of the integer hex map supplying distance limits.</typeparam>
        /// <param name="map">The source Boolean map. Its false cells are zero-distance starting points.</param>
        /// <param name="maxErodeDistanceMap">
        /// The maximum allowed arrival distance at each cell, measured in edge-adjacent hex steps
        /// from any original false cell. It must have the same topology as <paramref name="map"/>;
        /// values are matched by cell index. Zero and negative values block entry into true cells.
        /// </param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller, retaining the source topology and all
        /// original false cells regardless of their limits. Neither input map is modified.
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
        /// Thrown when the input maps do not have the same topology.
        /// </exception>
        public static BoolHexMap Erode<TIntMap>(this IHexMap<bool> map, TIntMap maxErodeDistanceMap)
            where TIntMap : IHexMap<int>
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            if (maxErodeDistanceMap is null)
                throw new ArgumentNullException(nameof(maxErodeDistanceMap));

            if (map.Topology != maxErodeDistanceMap.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(maxErodeDistanceMap));

            return new BoolHexMap(map.Topology, BooleanHexMapExpansionHelper.CreateConstrainedExpandedValues(map, maxErodeDistanceMap, expandedValue: false));
        }
    }
}
