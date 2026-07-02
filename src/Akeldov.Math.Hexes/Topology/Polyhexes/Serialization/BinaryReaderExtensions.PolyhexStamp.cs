using Akeldov.Math.Hexes.Vectors.QRS;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class BinaryReaderExtensions
    {
        public static Polyhex? ReadPolyhexStamp(
            this BinaryReader binaryReader)
        {
            var isNotNull = binaryReader.ReadBoolean();
            if (!isNotNull)
                return null;

            var qrsResolution = binaryReader.ReadVectorQRSInt();
            var builder = new PolyhexBuilder(qrsResolution.Q, qrsResolution.R);

            for (int q = 0; q < qrsResolution.Q; q++)
            {
                for (int r = 0; r < qrsResolution.R; r++)
                {
                    builder[q, r] = binaryReader.ReadBoolean();
                }
            }

            return builder.ToPolyhex();
        }
    }
}
