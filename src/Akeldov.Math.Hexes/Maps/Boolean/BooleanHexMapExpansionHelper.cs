using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    // Propagates true for dilation and false for erosion directly into the result array.
    internal static class BooleanHexMapExpansionHelper
    {
        internal static bool[] CreateExpandedValues(IHexMap<bool> source, int ringsCount, bool expandedValue)
        {
            HexMapTopology topology = source.Topology;
            int count = topology.Count;
            var values = new bool[count];
            if (ringsCount == 1)
            {
                if (expandedValue)
                    BooleanHexMapDilationHelper.FillDilatedValues(source, values);
                else
                    BooleanHexMapErosionHelper.FillErodedValues(source, values);
                return values;
            }

            int expandedCount = 0;
            for (int index = 0; index < count; index++)
            {
                bool value = source[index];
                values[index] = value;
                expandedCount += value == expandedValue ? 1 : 0;
            }

            if (ringsCount == 0 || expandedCount == 0 || expandedCount == count)
                return values;

            ExpandValues(values, topology, ringsCount, expandedCount, expandedValue);
            return values;
        }

        private static void ExpandValues(bool[] values, HexMapTopology topology, int ringsCount, int expandedCount, bool expandedValue)
        {
            int count = values.Length;
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
                // Dense masks are cheaper to expand by gathering their outer boundary than by
                // visiting all original source cells, each of which has up to six neighbors.
                int firstRing = expandedCount > count / 7 ? 1 : 0;
                if (firstRing == 1)
                {
                    tail = SeedExpansionFront(values, topology, queue, evenOffsets, oddOffsets, expandedValue);
                    for (int index = 0; index < tail; index++)
                        values[queue[index]] = expandedValue;
                    expandedCount += tail;
                }
                else
                {
                    for (int index = 0; index < count; index++)
                        if (values[index] == expandedValue)
                            queue[tail++] = index;
                }

                // Each layer contains one distance from the original source cells. The result doubles
                // as the visited set, so overlapping fronts never enqueue the same cell twice.
                for (int ring = firstRing; ring < ringsCount && head < tail && expandedCount < count; ring++)
                {
                    int layerEnd = tail;
                    while (head < layerEnd)
                    {
                        int currentIndex = queue[head++];
                        int y = currentIndex / width;
                        int x = currentIndex - y * width;
                        VectorXYInt[] offsets = BooleanHexMapTraversalHelper.GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);

                        for (int direction = 0; direction < offsets.Length; direction++)
                        {
                            if (!BooleanHexMapTraversalHelper.TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                                values[neighborIndex] == expandedValue)
                                continue;

                            values[neighborIndex] = expandedValue;
                            queue[tail++] = neighborIndex;
                            if (++expandedCount == count)
                                return;
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<int>.Shared.Return(queue);
            }
        }

        private static int SeedExpansionFront(
            bool[] values,
            HexMapTopology topology,
            int[] queue,
            VectorXYInt[] evenOffsets,
            VectorXYInt[] oddOffsets,
            bool expandedValue)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            int tail = 0;
            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                for (int x = 0; x < width; x++)
                {
                    int index = rowStart + x;
                    if (values[index] == expandedValue)
                        continue;

                    VectorXYInt[] offsets = BooleanHexMapTraversalHelper.GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);
                    for (int direction = 0; direction < offsets.Length; direction++)
                    {
                        if (BooleanHexMapTraversalHelper.TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) &&
                            values[neighborIndex] == expandedValue)
                        {
                            // Mark only after the scan, so newly found cells cannot extend this ring.
                            queue[tail++] = index;
                            break;
                        }
                    }
                }
            }

            return tail;
        }

        internal static bool[] CreateConstrainedExpandedValues<TIntMap>(IHexMap<bool> map, TIntMap maxDistanceMap, bool expandedValue)
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
                    if (values[index] == expandedValue)
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
                        VectorXYInt[] offsets = BooleanHexMapTraversalHelper.GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);
                        for (int direction = 0; direction < offsets.Length; direction++)
                        {
                            if (!BooleanHexMapTraversalHelper.TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
                                values[neighborIndex] == expandedValue || maxDistanceMap[neighborIndex] < distance)
                                continue;

                            values[neighborIndex] = expandedValue;
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
