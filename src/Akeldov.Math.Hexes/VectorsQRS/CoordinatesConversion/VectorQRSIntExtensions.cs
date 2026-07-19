using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides conversions from integer QRS indexes to offset-storage indexes.
    /// </summary>
    public static partial class VectorQRSIntExtensions
    {
        /// <summary>
        /// Converts a QRS hex index to its row-and-column offset index.
        /// </summary>
        /// <param name="index">The QRS index to convert.</param>
        /// <param name="layout">The offset layout of the destination index.</param>
        /// <returns>The corresponding XY storage index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXYInt ToXYIndex(this VectorQRSInt index, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXYInt(index.Q + ((index.R - (index.R & 1)) / 2), index.R);
                case Layout.EvenR:
                    return new VectorXYInt(index.Q + ((index.R + (index.R & 1)) / 2), index.R);
                case Layout.OddQ:
                    return new VectorXYInt(index.Q, index.R + ((index.Q - (index.Q & 1)) / 2));
                case Layout.EvenQ:
                    return new VectorXYInt(index.Q, index.R + ((index.Q + (index.Q & 1)) / 2));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
