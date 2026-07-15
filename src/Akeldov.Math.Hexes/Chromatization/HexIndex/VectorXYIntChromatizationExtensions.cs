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
