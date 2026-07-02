using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    /// <summary>
    /// Provides binary serialization helpers for vector types.
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes an integer vector to the current stream.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        /// <param name="vector">The vector to write.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorXYInt vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.X);
            writer.Write(vector.Y);
        }

        /// <summary>
        /// Writes a floating-point vector to the current stream.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        /// <param name="vector">The vector to write.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorXY vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.X);
            writer.Write(vector.Y);
        }
    }
}
