using System.IO;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides binary writers for QRS vectors and sixfold angles.
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes an integer QRS vector as Q followed by R, using two 32-bit integers.
        /// </summary>
        /// <param name="writer">The binary writer that receives the components.</param>
        /// <param name="vector">The integer QRS vector to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRSInt vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        /// <summary>
        /// Writes a fractional QRS vector as Q followed by R, using two 32-bit floating-point values.
        /// </summary>
        /// <param name="writer">The binary writer that receives the components.</param>
        /// <param name="vector">The fractional QRS vector to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this BinaryWriter writer, VectorQRS vector)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(vector.Q);
            writer.Write(vector.R);
        }

        /// <summary>
        /// Writes a sixfold angle as its 32-bit enum value.
        /// </summary>
        /// <param name="writer">The binary writer that receives the encoded angle.</param>
        /// <param name="angle">The defined sixfold angle to write.</param>
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
