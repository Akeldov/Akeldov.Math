using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes the value to the specified binary writer.
        /// </summary>
        /// <param name="writer">The writer value.</param>
        /// <param name="vector">The vector value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRSInt vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        /// <summary>
        /// Writes the value to the specified binary writer.
        /// </summary>
        /// <param name="writer">The writer value.</param>
        /// <param name="vector">The vector value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRS vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        /// <summary>
        /// Writes the value to the specified binary writer.
        /// </summary>
        /// <param name="writer">The writer value.</param>
        /// <param name="angle">The angle value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, SixfoldAngle angle)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            int value = (int)angle;
            if ((uint)value >= 6u)
                throw new ArgumentOutOfRangeException(nameof(angle), angle, "The angle must be a defined sixfold angle.");

            writer.Write(value);
        }
    }
}
