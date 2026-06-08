using Akeldov.Math.Spatial2D;
using System.IO;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class BinaryReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask ReadMask(this BinaryReader reader)
        {
            bool isNotNull = reader.ReadBoolean();
            if (!isNotNull)
                return null;

            var dim = reader.ReadVectorXYInt();
            var builder = new MaskBuilder(dim.X, dim.Y);

            for (int q = 0; q < dim.X; q++)
            {
                for (int r = 0; r < dim.Y; r++)
                {
                    builder[q, r] = reader.ReadBoolean();
                }
            }

            return builder.ToMask();
        }
    }
}
