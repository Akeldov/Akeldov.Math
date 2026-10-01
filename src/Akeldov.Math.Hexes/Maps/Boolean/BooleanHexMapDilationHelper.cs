using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using System.Buffers;

namespace Akeldov.Math.Hexes
{
    internal static class BooleanHexMapDilationHelper
    {
        internal static bool[] CreateDilatedValues(IHexMap<bool> source, int ringsCount)
        {
            HexMapTopology topology = source.Topology;
            int count = topology.Count;
            var values = new bool[count];
            if (ringsCount == 1)
            {
                FillDilatedValues(source, values);
                return values;
            }

            int trueCount = 0;
            for (int index = 0; index < count; index++)
            {
                bool value = source[index];
                values[index] = value;
                trueCount += value ? 1 : 0;
            }

            if (ringsCount == 0 || trueCount == 0 || trueCount == count)
                return values;

            ExpandDilatedValues(values, topology, ringsCount, trueCount);
            return values;
        }

        private static void ExpandDilatedValues(bool[] values, HexMapTopology topology, int ringsCount, int trueCount)
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
                // visiting all original true cells, each of which has up to six neighbors.
                int firstRing = trueCount > count / 7 ? 1 : 0;
                if (firstRing == 1)
                {
                    tail = SeedDilationFront(values, topology, queue, evenOffsets, oddOffsets);
                    for (int index = 0; index < tail; index++)
                        values[queue[index]] = true;
                    trueCount += tail;
                }
                else
                {
                    for (int index = 0; index < count; index++)
                        if (values[index])
                            queue[tail++] = index;
                }

                // Each layer contains one distance from the original true cells. The result doubles
                // as the visited set, so overlapping fronts never enqueue the same cell twice.
                for (int ring = firstRing; ring < ringsCount && head < tail && trueCount < count; ring++)
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
                                values[neighborIndex])
                                continue;

                            values[neighborIndex] = true;
                            queue[tail++] = neighborIndex;
                            if (++trueCount == count)
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

        private static int SeedDilationFront(
            bool[] values,
            HexMapTopology topology,
            int[] queue,
            VectorXYInt[] evenOffsets,
            VectorXYInt[] oddOffsets)
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
                    if (values[index])
                        continue;

                    VectorXYInt[] offsets = BooleanHexMapTraversalHelper.GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);
                    for (int direction = 0; direction < offsets.Length; direction++)
                    {
                        if (BooleanHexMapTraversalHelper.TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) &&
                            values[neighborIndex])
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

        internal static void FillDilatedValues(IHexMap<bool> source, bool[] destination)
        {
            HexMapTopology topology = source.Topology;
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                for (int x = 0; x < width; x++)
                {
                    int flatIndex = rowStart + x;
                    if (source[flatIndex])
                    {
                        destination[flatIndex] = true;
                        continue;
                    }

                    bool axisIsEven = ((parityUsesY ? y : x) & 1) == 0;
                    VectorXYInt[] offsets = axisIsEven ? evenOffsets : oddOffsets;
                    bool value = false;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        int neighborX = x + offsets[offsetIndex].X;
                        int neighborY = y + offsets[offsetIndex].Y;
                        if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
                            continue;

                        int neighborIndex = neighborY * width + neighborX;
                        if (source[neighborIndex])
                        {
                            value = true;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }

        internal static void FillDilatedValues(bool[] source, HexMapTopology topology, bool[] destination)
        {
            int width = topology.Resolution.X;
            int height = topology.Resolution.Y;
            bool parityUsesY = topology.Layout.IsPointyTop();
            VectorXYInt[] evenOffsets = true.GetSharedRelativeOffsets(topology.Layout);
            VectorXYInt[] oddOffsets = false.GetSharedRelativeOffsets(topology.Layout);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                for (int x = 0; x < width; x++)
                {
                    int flatIndex = rowStart + x;
                    if (source[flatIndex])
                    {
                        destination[flatIndex] = true;
                        continue;
                    }

                    bool axisIsEven = ((parityUsesY ? y : x) & 1) == 0;
                    VectorXYInt[] offsets = axisIsEven ? evenOffsets : oddOffsets;
                    bool value = false;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        int neighborX = x + offsets[offsetIndex].X;
                        int neighborY = y + offsets[offsetIndex].Y;
                        if ((uint)neighborX >= (uint)width || (uint)neighborY >= (uint)height)
                            continue;

                        int neighborIndex = neighborY * width + neighborX;
                        if (source[neighborIndex])
                        {
                            value = true;
                            break;
                        }
                    }

                    destination[flatIndex] = value;
                }
            }
        }

        internal static bool[] CreateConstrainedDilatedValues<TIntMap>(IHexMap<bool> map, TIntMap maxDilateDistanceMap)
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
                        VectorXYInt[] offsets = BooleanHexMapTraversalHelper.GetConnectivityOffsets(x, y, parityUsesY, evenOffsets, oddOffsets);
                        for (int direction = 0; direction < offsets.Length; direction++)
                        {
                            if (!BooleanHexMapTraversalHelper.TryGetNeighborFlatIndex(x, y, offsets[direction], width, height, out int neighborIndex) ||
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
