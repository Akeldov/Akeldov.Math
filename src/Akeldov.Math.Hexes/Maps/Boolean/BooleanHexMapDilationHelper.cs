using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes
{
    internal static class BooleanHexMapDilationHelper
    {
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
    }
}
