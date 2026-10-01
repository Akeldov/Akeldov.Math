using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Expands the true cells along paths allowed by per-cell distance limits.
        /// </summary>
        /// <typeparam name="TIntMap">The type of the integer hex map supplying distance limits.</typeparam>
        /// <param name="map">The source Boolean map. Its true cells are zero-distance starting points.</param>
        /// <param name="maxDilateDistanceMap">
        /// The maximum allowed arrival distance at each cell, measured in edge-adjacent hex steps
        /// from any original true cell. It must have the same topology as <paramref name="map"/>;
        /// values are matched by cell index. Zero and negative values block entry into false cells.
        /// </param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller, retaining the source topology and all
        /// original true cells regardless of their limits. Neither input map is modified.
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
        public static BoolHexMap Dilate<TIntMap>(this IHexMap<bool> map, TIntMap maxDilateDistanceMap)
            where TIntMap : IHexMap<int>
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            if (maxDilateDistanceMap is null)
                throw new ArgumentNullException(nameof(maxDilateDistanceMap));

            if (map.Topology != maxDilateDistanceMap.Topology)
                throw new ArgumentException("Hex maps must have the same topology.", nameof(maxDilateDistanceMap));

            return new BoolHexMap(map.Topology, CreateConstrainedDilatedValues(map, maxDilateDistanceMap));
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

            return new SpatialBoolHexMap(map.Geometry, CreateConstrainedDilatedValues(map, maxDilateDistanceMap));
        }

        private static bool[] CreateConstrainedDilatedValues<TIntMap>(IHexMap<bool> map, TIntMap maxDilateDistanceMap)
            where TIntMap : IHexMap<int>
        {
            HexMapTopology topology = map.Topology;
            int count = topology.Count;
            var values = new bool[count];
            if (count == 0)
                return values;

            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            int[] queue = ArrayPool<int>.Shared.Rent(count);

            try
            {
                int head = 0;
                int tail = 0;
                for (int index = 0; index < count; index++)
                {
                    values[index] = map[index];
                    if (values[index])
                        queue[tail++] = index;
                }

                // FIFO layers give the shortest admissible arrival distance. A failed arrival
                // cannot succeed at a later distance, and only accepted cells transmit expansion.
                int distance = 0;
                while (head < tail && tail < count)
                {
                    int layerEnd = tail;
                    distance++;
                    while (head < layerEnd)
                    {
                        int currentIndex = queue[head++];
                        int y = currentIndex / width;
                        int x = currentIndex - y * width;
                        VectorXYInt[] offsets = GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);
                        for (int direction = 0; direction < offsets.Length; direction++)
                        {
                            if (!TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                                values[neighborIndex] || maxDilateDistanceMap[neighborIndex] < distance)
                                continue;

                            values[neighborIndex] = true;
                            queue[tail++] = neighborIndex;
                            if (tail == count)
                                return values;
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(queue);
            }

            return values;
        }
    }
}
