using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexIndex">The hexIndex value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetChromaticClass(this VectorXYInt hexIndex, Layout layout)
        {
            var qrsIndex = hexIndex.ToQRSIndex(layout);

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return PositiveModulo(qrsIndex.Q - qrsIndex.R, 3);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return PositiveModulo(qrsIndex.R - qrsIndex.Q, 3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int PositiveModulo(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }
    }
}
