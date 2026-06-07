using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System.IO;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class BinaryWriterExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write(this BinaryWriter writer, bool[,] mask)
        {
            if (mask != null)
            {
                writer.Write(true);
                var dim = new VectorXYInt(mask.GetLength(0), mask.GetLength(1));
                writer.Write(dim);
                for (int i = 0; i < dim.X; i++)
                {
                    for (int j = 0; j < dim.Y; j++)
                    {
                        writer.Write(mask[i, j]);
                    }
                }
            }
            else
            {
                writer.Write(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write(this BinaryWriter writer, Mask mask)
        {
            if (mask != null)
            {
                writer.Write(true);
                var dim = new VectorXYInt(mask.QSize, mask.RSize);
                writer.Write(dim);
                for (int q = 0; q < dim.X; q++)
                {
                    for (int r = 0; r < dim.Y; r++)
                    {
                        writer.Write(mask[q, r]);
                    }
                }
            }
            else
            {
                writer.Write(false);
            }
        }
    }
}
