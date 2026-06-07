using Akeldov.Math.Spatial2D;
using System.IO;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class BinaryReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool[,] ReadBoolMask(this BinaryReader reader)
        {
            var isNotNull = reader.ReadBoolean();
            if (isNotNull)
            {
                var dim = reader.ReadVectorXYInt();
                var mask = new bool[dim.X, dim.Y];
                for (int i = 0; i < dim.X; i++)
                {
                    for (int j = 0; j < dim.Y; j++)
                    {
                        mask[i, j] = reader.ReadBoolean();
                    }
                }
                return mask;
            }
            else
            {
                return null;
            }
        }
    }
}
