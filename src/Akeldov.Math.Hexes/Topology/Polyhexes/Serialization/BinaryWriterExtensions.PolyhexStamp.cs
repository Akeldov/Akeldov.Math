using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes the value to the specified binary writer.
        /// </summary>
        /// <param name="binaryWriter">The BinaryWriter value.</param>
        /// <param name="polyhexStamp">The PolyhexStamp value.</param>
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
