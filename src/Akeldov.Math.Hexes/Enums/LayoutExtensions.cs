using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides orientation queries for hex-grid layouts.
    /// </summary>
    public static class LayoutExtensions
    {
        /// <summary>
        /// Determines whether the layout uses pointy-top hexagons.
        /// </summary>
        /// <param name="layout">The layout to inspect.</param>
        /// <returns><see langword="true"/> for odd-row and even-row layouts; otherwise, <see langword="false"/>.</returns>
        public static bool IsPointyTop(this Layout layout)
        {
            return layout == Layout.OddR || layout == Layout.EvenR;
        }

        /// <summary>
        /// Determines whether the layout uses flat-top hexagons.
        /// </summary>
        /// <param name="layout">The layout to inspect.</param>
        /// <returns><see langword="true"/> for odd-column and even-column layouts; otherwise, <see langword="false"/>.</returns>
        public static bool IsFlatTop(this Layout layout)
        {
            return layout == Layout.OddQ || layout == Layout.EvenQ;
        }

        /// <summary>
        /// Gets the hex orientation associated with a layout.
        /// </summary>
        /// <param name="layout">The layout whose orientation is required.</param>
        /// <returns>The pointy-top or flat-top orientation of the layout.</returns>
        public static HexOrientation GetHexOrientation(this Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return HexOrientation.PointyTop;

                case Layout.OddQ:
                case Layout.EvenQ:
                    return HexOrientation.FlatTop;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(layout),
                        layout,
                        $"Layout {layout} is not supported.");
            }
        }
    }
}
