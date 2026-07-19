using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Writes polyhex masks to binary streams.
    /// </summary>
    public static partial class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a nullable polyhex mask, including its QRS resolution and Q-major cell values.
        /// </summary>
        /// <param name="binaryWriter">The writer that receives the presence flag and, when present, the polyhex data.</param>
        /// <param name="polyhexStamp">The polyhex to write, or <see langword="null"/> to write only an absent-value flag.</param>
        public static void Write(
            this BinaryWriter binaryWriter,
            Polyhex? polyhexStamp)
        {
            if (binaryWriter == null)
                throw new ArgumentNullException(nameof(binaryWriter));

            if (polyhexStamp != null)
            {
                binaryWriter.Write(true);
                binaryWriter.Write(polyhexStamp.QRSResolution);
                for (int q = 0; q < polyhexStamp.QRSResolution.Q; q++)
                {
                    for (int r = 0; r < polyhexStamp.QRSResolution.R; r++)
                    {
                        binaryWriter.Write(polyhexStamp[q, r]);
                    }
                }
            }
            else
            {
                binaryWriter.Write(false);
            }
        }
    }
}
