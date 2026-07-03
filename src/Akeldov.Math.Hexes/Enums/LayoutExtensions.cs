using System;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static class LayoutExtensions
    {
        /// <summary>
        /// Performs the IsPointyTop operation.
        /// </summary>
        /// <param name="layout">The layout value.</param>
        public static bool IsPointyTop(this Layout layout)
        {
            return layout == Layout.OddR || layout == Layout.EvenR;
        }

        /// <summary>
        /// Performs the IsFlatTop operation.
        /// </summary>
        /// <param name="layout">The layout value.</param>
        public static bool IsFlatTop(this Layout layout)
        {
            return layout == Layout.OddQ || layout == Layout.EvenQ;
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="layout">The layout value.</param>
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
