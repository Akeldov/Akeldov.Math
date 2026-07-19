using Akeldov.Math.Hexes.Vectors.QRS;
using System;
using System.IO;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Reads polyhex masks from binary streams.
    /// </summary>
    public static partial class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a nullable polyhex mask written by the matching binary-writer extension.
        /// </summary>
        /// <param name="binaryReader">The reader positioned at the polyhex presence flag.</param>
        /// <returns>The reconstructed polyhex, or <see langword="null"/> when the serialized value is absent.</returns>
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
