using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Chromatization
{
    /// <summary>
    /// Computes three-color classes for offset-grid hex indices.
    /// </summary>
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Gets the three-color class of the specified hex.
        /// </summary>
        /// <param name="hexIndex">The hex index to classify.</param>
        /// <param name="layout">The offset-coordinate layout.</param>
        /// <returns>A class index from 0 through 2; adjacent hexes have different classes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetChromaticClass(this VectorXYInt hexIndex, Layout layout)
        {
            int value;

            switch (layout)
            {
                case Layout.OddR:
                    value = hexIndex.X - (hexIndex.Y & 1);
                    break;
                case Layout.EvenR:
                    value = hexIndex.X + (hexIndex.Y & 1);
                    break;
                case Layout.OddQ:
                    value = hexIndex.Y - (hexIndex.X & 1);
                    break;
                case Layout.EvenQ:
                    value = hexIndex.Y + (hexIndex.X & 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }

            int result = value % 3;
            return result < 0 ? result + 3 : result;
        }

        /// <summary>
        /// Gets the chromatic class for the odd-row offset layout.
        /// </summary>
        /// <param name="hexIndex">The hex index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOddRChromaticClass(this VectorXYInt hexIndex)
        {
            int result = (hexIndex.X - (hexIndex.Y & 1)) % 3;
            return result < 0 ? result + 3 : result;
        }

        /// <summary>
        /// Gets the chromatic class for the even-row offset layout.
        /// </summary>
        /// <param name="hexIndex">The hex index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEvenRChromaticClass(this VectorXYInt hexIndex)
        {
            int result = (hexIndex.X + (hexIndex.Y & 1)) % 3;
            return result < 0 ? result + 3 : result;
        }

        /// <summary>
        /// Gets the chromatic class for the odd-column offset layout.
        /// </summary>
        /// <param name="hexIndex">The hex index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOddQChromaticClass(this VectorXYInt hexIndex)
        {
            int result = (hexIndex.Y - (hexIndex.X & 1)) % 3;
            return result < 0 ? result + 3 : result;
        }

        /// <summary>
        /// Gets the chromatic class for the even-column offset layout.
        /// </summary>
        /// <param name="hexIndex">The hex index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEvenQChromaticClass(this VectorXYInt hexIndex)
        {
            int result = (hexIndex.Y + (hexIndex.X & 1)) % 3;
            return result < 0 ? result + 3 : result;
        }
    }
}
