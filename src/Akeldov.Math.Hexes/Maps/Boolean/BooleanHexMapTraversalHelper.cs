using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    internal static class BooleanHexMapTraversalHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static VectorXYInt[] GetConnectivityOffsets(
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
        internal static bool TryGetNeighborFlatIndex(
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
