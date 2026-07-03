using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VectorQRSIntExtensions
    {
        /// <summary>
        /// Converts the value to the requested representation.
        /// </summary>
        /// <param name="index">The index value.</param>
        /// <param name="layout">The layout value.</param>
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
