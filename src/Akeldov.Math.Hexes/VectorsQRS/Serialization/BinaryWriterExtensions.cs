using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static class BinaryWriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRSInt vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRS vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, SixfoldAngle angle)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write((int)angle);
        }
    }
}
