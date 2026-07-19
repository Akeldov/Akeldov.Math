using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides binary readers for QRS vectors and sixfold angles.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads an integer QRS vector encoded as Q followed by R, using two 32-bit integers.
        /// </summary>
        /// <param name="reader">The binary reader positioned at the Q component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRSInt ReadVectorQRSInt(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var q = reader.ReadInt32();
            var r = reader.ReadInt32();
            return new VectorQRSInt(q, r);
        }

        /// <summary>
        /// Reads a fractional QRS vector encoded as Q followed by R, using two 32-bit floating-point values.
        /// </summary>
        /// <param name="reader">The binary reader positioned at the Q component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ReadVectorQRS(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var q = reader.ReadSingle();
            var r = reader.ReadSingle();
            return new VectorQRS(q, r);
        }

        /// <summary>
        /// Reads a sixfold angle encoded as its 32-bit enum value.
        /// </summary>
        /// <param name="reader">The binary reader positioned at the encoded angle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SixfoldAngle ReadSixfoldAngle(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var value = reader.ReadInt32();
            if ((uint)value >= 6u)
                throw new InvalidDataException($"Invalid sixfold angle value: {value}.");

            return (SixfoldAngle)value;
        }
    }
}
