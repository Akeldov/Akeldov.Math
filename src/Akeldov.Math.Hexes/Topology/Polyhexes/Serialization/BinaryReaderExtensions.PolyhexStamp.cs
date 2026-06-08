using Akeldov.Math.Hexes.Vectors.QRS;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    public static partial class BinaryReaderExtensions
    {
        public static Polyhex ReadPolyhexStamp(
            this BinaryReader binaryReader)
        {
            var isNotNull = binaryReader.ReadBoolean();
            if (isNotNull)
            {
                var dimension = binaryReader.ReadVectorQRSInt();
                var mask = binaryReader.ReadMask();
                return new Polyhex(mask);
            }
            else
            {
                return null;
            }
        }
    }
}
