using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
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
                    return HexOrientation.PointyTop;
                case Layout.EvenR:
                    return HexOrientation.PointyTop;
                case Layout.OddQ:
                    return HexOrientation.FlatTop;
                case Layout.EvenQ:
                    return HexOrientation.FlatTop;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
