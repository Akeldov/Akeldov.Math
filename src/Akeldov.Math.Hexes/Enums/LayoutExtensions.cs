using System;

namespace Akeldov.Math.Hexes
{
    public static class LayoutExtensions
    {
        public static bool IsPointyTop(this Layout layout)
        {
            return layout == Layout.OddR || layout == Layout.EvenR;
        }

        public static bool IsFlatTop(this Layout layout)
        {
            return layout == Layout.OddQ || layout == Layout.EvenQ;
        }

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
