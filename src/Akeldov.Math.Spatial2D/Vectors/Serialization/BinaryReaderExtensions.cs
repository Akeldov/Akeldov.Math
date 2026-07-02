using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides binary deserialization helpers for vector types.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads an integer vector from the current stream.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <returns>The vector read from the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ReadVectorXYInt(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            return new VectorXYInt(x, y);
        }

        /// <summary>
        /// Reads a floating-point vector from the current stream.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <returns>The vector read from the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ReadVectorXY(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            return new VectorXY(x, y);
        }
    }
}
