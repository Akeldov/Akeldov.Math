using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    public static partial class BooleanHexMapExtensions
    {
        /// <summary>
        /// Selects the six-connected component containing the specified seed cell.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <param name="seed">The seed cell whose Boolean value determines the selected component.</param>
        /// <returns>
        /// A new mutable Boolean hex map owned by the caller. Cells in the selected component are
        /// <see langword="true"/> and all other cells are <see langword="false"/>. The source map
        /// is not modified.
        /// </returns>
        /// <remarks>
        /// Connectivity is defined by the six edge-adjacent hexes. The selected component may
        /// consist of either <see langword="true"/> or <see langword="false"/> source cells,
        /// according to the value at <paramref name="seed"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="seed"/> is outside the map topology.
        /// </exception>
        public static BoolHexMap FloodFill(this IHexMap<bool> map, VectorXYInt seed)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            if ((uint)seed.X >= (uint)topology.Resolution.X ||
                (uint)seed.Y >= (uint)topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(seed), seed, "Flood-fill seed must be inside the map topology.");

            int seedIndex = checked(seed.Y * topology.Resolution.X + seed.X);
            return new BoolHexMap(topology, CreateFloodFillValues(map, seedIndex));
        }

        /// <summary>
        /// Selects the six-connected component containing the specified seed cell in a spatial map.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <param name="seed">The seed cell whose Boolean value determines the selected component.</param>
        /// <returns>
        /// A new mutable spatial Boolean hex map owned by the caller. The source geometry is
        /// preserved, cells in the selected component are <see langword="true"/>, and all other
        /// cells are <see langword="false"/>. The source map is not modified.
        /// </returns>
        /// <remarks>
        /// Connectivity is defined by the six edge-adjacent hexes. The selected component may
        /// consist of either <see langword="true"/> or <see langword="false"/> source cells,
        /// according to the value at <paramref name="seed"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="seed"/> is outside the map topology.
        /// </exception>
        public static SpatialBoolHexMap FloodFill(this ISpatialHexMap<bool> map, VectorXYInt seed)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            HexMapTopology topology = map.Topology;
            if ((uint)seed.X >= (uint)topology.Resolution.X ||
                (uint)seed.Y >= (uint)topology.Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(seed), seed, "Flood-fill seed must be inside the map topology.");

            int seedIndex = checked(seed.Y * topology.Resolution.X + seed.X);
            return new SpatialBoolHexMap(map.Geometry, CreateFloodFillValues(map, seedIndex));
        }

        /// <summary>
        /// Labels every six-connected component of <see langword="true"/> cells.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <returns>
        /// A new mutable integer label map and the number of components. Source cells containing
        /// <see langword="false"/> receive label zero. Components of <see langword="true"/> cells
        /// receive consecutive labels from one through <c>Count</c> in deterministic row-major
        /// discovery order. The source map is not modified.
        /// </returns>
        /// <remarks>Connectivity is defined by the six edge-adjacent hexes.</remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static (IntHexMap Labels, int Count) ConnectedComponents(this IHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            int[] labels = CreateConnectedComponentLabels(map, out int count);
            return (new IntHexMap(map.Topology, labels), count);
        }

        /// <summary>
        /// Labels every six-connected component of <see langword="true"/> cells in a spatial map.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <returns>
        /// A new mutable spatial integer label map and the number of components. The source
        /// geometry is preserved. Source cells containing <see langword="false"/> receive label
        /// zero. Components of <see langword="true"/> cells receive consecutive labels from one
        /// through <c>Count</c> in deterministic row-major discovery order. The source map is not
        /// modified.
        /// </returns>
        /// <remarks>Connectivity is defined by the six edge-adjacent hexes.</remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology.
        /// </exception>
        public static (SpatialIntHexMap Labels, int Count) ConnectedComponents(this ISpatialHexMap<bool> map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            int[] labels = CreateConnectedComponentLabels(map, out int count);
            return (new SpatialIntHexMap(map.Geometry, labels), count);
        }

        /// <summary>
        /// Computes the six-neighbor graph distance to the nearest cell containing the target value.
        /// </summary>
        /// <param name="map">The source Boolean map.</param>
        /// <param name="targetValue">The source value whose cells serve as zero-distance targets.</param>
        /// <returns>
        /// A new mutable integer hex map owned by the caller. Every target cell has distance zero,
        /// and every other cell contains its shortest six-neighbor graph distance to a target. If
        /// the source contains no target cells, every result cell contains <see cref="int.MaxValue"/>.
        /// The source map is not modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
#pragma warning disable RS0026 // Paired extension overloads intentionally preserve the spatial result type.
        public static IntHexMap DistanceTransform(this IHexMap<bool> map, bool targetValue = false)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            return new IntHexMap(map.Topology, CreateDistanceTransformValues(map, targetValue));
        }

        /// <summary>
        /// Computes the six-neighbor graph distance to the nearest cell containing the target value
        /// in a spatial map.
        /// </summary>
        /// <param name="map">The source spatial Boolean map.</param>
        /// <param name="targetValue">The source value whose cells serve as zero-distance targets.</param>
        /// <returns>
        /// A new mutable spatial integer hex map owned by the caller. The source geometry is
        /// preserved. Every target cell has distance zero, and every other cell contains its
        /// shortest six-neighbor graph distance to a target. If the source contains no target
        /// cells, every result cell contains <see cref="int.MaxValue"/>. The source map is not
        /// modified.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the map topology differs from its geometry topology.
        /// </exception>
        public static SpatialIntHexMap DistanceTransform(
            this ISpatialHexMap<bool> map,
            bool targetValue = false)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (map.Topology != map.Geometry.Topology)
                throw new ArgumentException("Spatial hex map topology must match its geometry topology.", nameof(map));

            return new SpatialIntHexMap(map.Geometry, CreateDistanceTransformValues(map, targetValue));
        }
#pragma warning restore RS0026

        private static bool[] CreateFloodFillValues(IHexMap<bool> map, int seedIndex)
        {
            HexMapTopology topology = map.Topology;
            int count = topology.Count;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            var selected = new bool[count];
            int[] queue = ArrayPool<int>.Shared.Rent(count);

            try
            {
                bool selectedValue = map[seedIndex];
                int head = 0;
                int tail = 0;
                selected[seedIndex] = true;
                queue[tail++] = seedIndex;

                while (head < tail)
                {
                    int currentIndex = queue[head++];
                    int y = currentIndex / width;
                    int x = currentIndex - y * width;
                    VectorXYInt[] offsets = GetConnectivityOffsets(
                        x,
                        y,
                        parityUsesY,
                        evenOffsets,
                        oddOffsets);

                    for (int direction = 0; direction < offsets.Length; direction++)
                    {
                        if (!TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                            selected[neighborIndex] ||
                            map[neighborIndex] != selectedValue)
                            continue;

                        selected[neighborIndex] = true;
                        queue[tail++] = neighborIndex;
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(queue);
            }

            return selected;
        }

        private static int[] CreateConnectedComponentLabels(IHexMap<bool> map, out int componentCount)
        {
            HexMapTopology topology = map.Topology;
            int count = topology.Count;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            var labels = new int[count];
            int[] queue = ArrayPool<int>.Shared.Rent(count);

            componentCount = 0;
            try
            {
                for (int seedIndex = 0; seedIndex < count; seedIndex++)
                {
                    if (labels[seedIndex] != 0 || !map[seedIndex])
                        continue;

                    componentCount++;
                    int head = 0;
                    int tail = 0;
                    labels[seedIndex] = componentCount;
                    queue[tail++] = seedIndex;

                    while (head < tail)
                    {
                        int currentIndex = queue[head++];
                        int y = currentIndex / width;
                        int x = currentIndex - y * width;
                        VectorXYInt[] offsets = GetConnectivityOffsets(
                            x,
                            y,
                            parityUsesY,
                            evenOffsets,
                            oddOffsets);

                        for (int direction = 0; direction < offsets.Length; direction++)
                        {
                            if (!TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                                labels[neighborIndex] != 0 ||
                                !map[neighborIndex])
                                continue;

                            labels[neighborIndex] = componentCount;
                            queue[tail++] = neighborIndex;
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(queue);
            }

            return labels;
        }

        private static int[] CreateDistanceTransformValues(IHexMap<bool> map, bool targetValue)
        {
            HexMapTopology topology = map.Topology;
            int count = topology.Count;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);
            var distances = new int[count];
            int[] queue = ArrayPool<int>.Shared.Rent(count);

            try
            {
                int head = 0;
                int tail = 0;

                for (int index = 0; index < count; index++)
                {
                    if (map[index] == targetValue)
                    {
                        distances[index] = 0;
                        queue[tail++] = index;
                    }
                    else
                    {
                        distances[index] = int.MaxValue;
                    }
                }

                while (head < tail)
                {
                    int currentIndex = queue[head++];
                    int nextDistance = distances[currentIndex] + 1;
                    int y = currentIndex / width;
                    int x = currentIndex - y * width;
                    VectorXYInt[] offsets = GetConnectivityOffsets(
                        x,
                        y,
                        parityUsesY,
                        evenOffsets,
                        oddOffsets);

                    for (int direction = 0; direction < offsets.Length; direction++)
                    {
                        if (!TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                            distances[neighborIndex] != int.MaxValue)
                            continue;

                        distances[neighborIndex] = nextDistance;
                        queue[tail++] = neighborIndex;
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(queue);
            }

            return distances;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXYInt[] GetConnectivityOffsets(
            int x,
            int y,
            bool parityUsesY,
            VectorXYInt[] evenOffsets,
            VectorXYInt[] oddOffsets)
        {
            bool axisIsEven = ((parityUsesY ? y : x) & 1) == 0;
            return axisIsEven ? evenOffsets : oddOffsets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetNeighborFlatIndex(
            int x,
            int y,
            VectorXYInt offset,
            int width,
            int height,
            out int neighborIndex)
        {
            int neighborX = x + offset.X;
            int neighborY = y + offset.Y;
            if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
            {
                neighborIndex = default;
                return false;
            }

            neighborIndex = neighborY * width + neighborX;
            return true;
        }
    }
}
