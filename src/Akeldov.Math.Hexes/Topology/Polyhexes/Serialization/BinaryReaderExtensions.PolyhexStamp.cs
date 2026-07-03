using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a value from the specified binary reader.
        /// </summary>
        /// <param name="binaryReader">The binaryReader value.</param>
        public static Polyhex? ReadPolyhexStamp(
            this BinaryReader binaryReader)
        {
            if (binaryReader == null)
                throw new ArgumentNullException(nameof(binaryReader));

            var isNotNull = binaryReader.ReadBoolean();
            if (!isNotNull)
                return null;

            var qrsResolution = binaryReader.ReadVectorQRSInt();
            var builder = new PolyhexBuilder(qrsResolution.Q, qrsResolution.R);

            for (int q = 0; q < qrsResolution.Q; q++)
            {
                for (int r = 0; r < qrsResolution.R; r++)
                {
                    builder[q, r] = binaryReader.ReadBoolean();
                }
            }

            return builder.ToPolyhex();
        }
    }
}
