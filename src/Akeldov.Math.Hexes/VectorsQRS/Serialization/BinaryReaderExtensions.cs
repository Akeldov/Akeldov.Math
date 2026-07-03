using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a value from the specified binary reader.
        /// </summary>
        /// <param name="reader">The reader value.</param>
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
        /// Reads a value from the specified binary reader.
        /// </summary>
        /// <param name="reader">The reader value.</param>
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
        /// Reads a value from the specified binary reader.
        /// </summary>
        /// <param name="reader">The reader value.</param>
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
