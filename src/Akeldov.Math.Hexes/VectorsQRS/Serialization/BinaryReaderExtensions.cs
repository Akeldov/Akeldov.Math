using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static class BinaryReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRSInt ReadVectorQRSInt(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var q = reader.ReadInt32();
            var r = reader.ReadInt32();
            return new VectorQRSInt(q, r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ReadVectorQRS(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var q = reader.ReadSingle();
            var r = reader.ReadSingle();
            return new VectorQRS(q, r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle ReadSixfoldAngle(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var value = reader.ReadInt32();
            return (SixfoldAngle)value;
        }
    }
}
